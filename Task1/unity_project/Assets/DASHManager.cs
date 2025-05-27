using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class DASHManager : MonoBehaviour {
    [Header("DASH Manifest (.mpd) file URL")]
    public string manifestURL = "http://localhost:8000/output_video.mpd";   // the URL to a local server with the mpd file

    [Header("Probe Interval")]
    public float probeInterval = 2f;        // the amount of seconds between measuring each throughput (real-time download speed)

    [Header("Safety Factor")]               // lower = slower simulated internet speed essentially
    public float safetyFactor = 1f;         // the fraction of how much throughput to trust when picking quality

    [Header("Video Player")]
    public VideoPlayer videoPlayer;         // the video player

    // list that holds the required bandwidth and the URL for each quality level
    private List<(int bandwidth, string url)> repUrls = new List<(int, string)>();

    // -1 = auto, 0 = 144p, 1 = 360p, 2 = 720p
    private int forcedQualityIndex = -1;    // field that will force the video to play at a certain quality 

    private string logPath;                 // the file path where we will save the log.txt file
    public void SetSafetyFactor(float sf){  // method called by SafetyFactorDisplay.cs when the slider value is changed
        safetyFactor = Mathf.Clamp01(sf);   // sets safety factor to the value determined by the slider and locks it between 0 and 1
        UnityEngine.Debug.Log($"[DASHManager] safetyFactor is now {safetyFactor:F2}");  // logs to unity console the value change
    }

    void Awake(){   // sets up formatting for the log file before anything else is run essentially
        logPath = Path.Combine(Application.dataPath, "log.txt");        // sets the file path for the log.txt file
        string h0 = "time (s)".PadLeft(10);                             // sets first column as time in seconds
        string h1 = "quality".PadLeft(10);                              // sets second column as the quality (ex: 144p)
        string h2 = "quality (bps)".PadLeft(14);                        // sets third column as the quality in bps
        string h3 = "throughput (kbps)".PadLeft(18);                    // sets fourth column as the throughput in kbps
        string h4 = "delay (ms)".PadLeft(10);                           // sets last column as the delay in 
        File.WriteAllText(logPath, $"{h0}  {h1}  {h2}  {h3}  {h4}\n");  // formats all of the columns in the file together
    }

    void Start(){
        StartCoroutine(InitializeAndRun());                             // starts coroutine/calls InitializeAndRun method (allow pauses)
    }

    public void SetForcedQuality(int idx){                              // called by quality control buttons/toggles in unity
        if (idx >= -1 && idx < repUrls.Count) forcedQualityIndex = idx; // sets forced quality to corresponding index
    }

    IEnumerator InitializeAndRun(){
        yield return ParseManifestSimple();     // pause and call ParseManifestSimple method which assembles all video quality links
        StartCoroutine(AdaptiveProbe());        // starts coroutine/calls AdaptiveProbe method which determines quality based on throughput

        var first = repUrls[0];                 // sets first to the lowest quality url (index 0)
        videoPlayer.url = first.url;            // sets the video player url to the lowest quality video url initially
    }

    IEnumerator ParseManifestSimple(){
        using UnityWebRequest www = UnityWebRequest.Get(manifestURL);   // create the Get request for the URL 
        yield return www.SendWebRequest();                              // wait until mpd file is fully downloaded from URL
        if (www.result != UnityWebRequest.Result.Success){              // if download error, send console log and break from method
            UnityEngine.Debug.LogError($"Manifest download error: {www.error}");
            yield break;
        }

        var xml = new XmlDocument();                // creates an XML object to read the mpd/xml file
        xml.LoadXml(www.downloadHandler.text);      // load the mpd text into the XML object
        string basePath = manifestURL.Substring(0, manifestURL.LastIndexOf('/') + 1); // sets basePath to "http://localhost:8000/"

        XmlNodeList reps = xml.GetElementsByTagName("Representation");  // creates a list of all the <Representation> elements in the XML file
        foreach (XmlNode rep in reps){                                  // loops over each <Representation> element
            int bw = int.Parse(rep.Attributes["bandwidth"].Value);      // stores bandwidth attribute and converts from string to int
            if (bw < 200000) continue;                                  // if bandwidth < 200000 skip since its audio only

            string id = rep.Attributes["id"].Value;                     // stores id attribute which tells us the qualities of the 3 videos
            string mp4name = id switch {
                "0" => "144p.mp4",                                      // id 0 = 144p video (ids 1,3,4)
                "2" => "360p.mp4",                                      // id 2 = 360p video
                "4" => "720p.mp4",                                      // id 4 = 720p video
                _ => null
            };
            if (mp4name == null) continue;                              // skip rest of iteration if null (error)

            repUrls.Add((bw, basePath + mp4name));                      // store each video's bandwidth and url in the repUrls list
        }

        repUrls.Sort((a, b) => a.bandwidth.CompareTo(b.bandwidth));     // sort the list from least to greatest bandwidths
        UnityEngine.Debug.Log($"[DASHManager] {repUrls.Count} qualities loaded:");  // log amount of qualities (should be 3)
        foreach (var (bw, url) in repUrls)                                          // logs bitrate in bps of each quality
            UnityEngine.Debug.Log($"Bitrate: {bw}bps    URL: {url}");
    }

    IEnumerator AdaptiveProbe(){
        var sw = new Stopwatch();   // creates Stopwatch object to log time
        while (true){               // run infinite loop for video player streaming
            sw.Restart();           // restart the stopwatch
            int currentIdx = repUrls.FindIndex(r => r.url == videoPlayer.url);              // find index of quality that's currently playing
            string testUrl = (currentIdx >= 0) ? repUrls[currentIdx].url : repUrls[0].url;  // set URL to corresponding index or to 0 if false

            using var req = UnityWebRequest.Get(testUrl);       // send web request to URL
            yield return req.SendWebRequest();                  // wait until download completed
            sw.Stop();                                          // stop timer once download finishes

            if (req.result == UnityWebRequest.Result.Success){  // if download is successful
                double seconds = sw.Elapsed.TotalSeconds;       // seconds it took for web request to complete
                double kbps = (req.downloadedBytes * 8.0 / 1024.0) / seconds;   // calculate throughput in kbps: bytes*8 = bits/1024 = kb
                long delayMs = sw.ElapsedMilliseconds;                          // set delay to elapsed time in ms

                int chosenIndex;
                if (forcedQualityIndex >= 0){                           // if statement to decide what quality to use
                    chosenIndex = forcedQualityIndex;                   // set quality to what the user forced it to
                } else {
                    chosenIndex = 0;                                    // if user did not force, set index to 0 initially
                    double effKbps = kbps * safetyFactor;               // determine the effective internet speed
                    for (int i = 0; i < repUrls.Count; i++)             // determine quality depending on bandwidth
                        if (repUrls[i].bandwidth / 1000.0 < effKbps)    // will pick highest bandwidth that is less than internet speed
                            chosenIndex = i;
                }

                var chosen = repUrls[chosenIndex];                      // sets chosen quality to chosen index
                if (videoPlayer.url != chosen.url){                     // if video playing differs, change it to correct quality
                    UnityEngine.Debug.Log($"[DASH] Switching to {chosen.bandwidth}bps ({chosen.url})"); // log quality switch
                    StartCoroutine(SwitchQualityKeepingTime(chosen.url));   // start coroutine to keep video time when quality switches
                }

                string label = Path.GetFileNameWithoutExtension(chosen.url);    // get video file name ex: 720p
                string f0 = Time.time.ToString("F1").PadLeft(10);               // time passed in Unity overall
                string f1 = label.PadLeft(10);                                  // quality label ex: 720p
                string f2 = chosen.bandwidth.ToString().PadLeft(14);            // chosen quality bit rate 
                string f3 = ((int)kbps).ToString().PadLeft(18);                 // throughput in kbps
                string f4 = delayMs.ToString().PadLeft(10);                     // the delay in ms

                string line = $"{f0}  {f1}  {f2}  {f3}  {f4}\n";                // formats data into columns
                File.AppendAllText(logPath, line);                              // append log.txt with the data
            }

            yield return new WaitForSeconds(probeInterval);                     // wait probe interval length (2s) before repeating loop
        }
    }

    IEnumerator SwitchQualityKeepingTime(string newUrl){
        double currentTime = videoPlayer.time;                      // save current video time before switching quality
        bool wasPlaying = videoPlayer.isPlaying;                    // determine if video was playing

        videoPlayer.Pause();                                        // pause video
        videoPlayer.url = newUrl;                                   // change video url / change qualities
        videoPlayer.Prepare();                                      // ensure video is prepared for playback
        yield return new WaitUntil(() => videoPlayer.isPrepared);   // wait until video is prepared for playback

        videoPlayer.time = currentTime;                             // set video time to same time as before
        if (wasPlaying) videoPlayer.Play();                         // if video was playing before, play again since we paused it earlier
    }
}