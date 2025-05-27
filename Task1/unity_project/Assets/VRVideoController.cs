using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VRVideoController : MonoBehaviour {
    [Header("Video Player")]
    public VideoPlayer videoPlayer;     // the video player

    [Header("Video Quick Controls")]
    public Toggle playPauseToggle;      // the play/pause/replay button
    public Image playPauseIconImage;    // the actual play/pause/replay image to change
    public Sprite playIcon;             // the play icon to display when video paused
    public Sprite pauseIcon;            // the pause icon to display when video playing
    public Sprite replayIcon;           // the replay icon to display when video ends
    public Toggle back10Toggle;         // the -10s button
    public Toggle forward10Toggle;      // the +10s button

    [Header("Time Slider Controls")]
    public Slider timeSlider;           // the video time slider
    public TMP_Text timeElapsedText;    // the time elapsed text
    public TMP_Text timeRemainingText;  // the time remaining text

    [Header("Volume Slider Controls")]
    public Slider volumeSlider;         // the video volume slider
    public Image volumeIconImage;       // the actual volume image to change
    public Sprite muteIcon;             // the mute icon to display when volume = 0
    public Sprite lowIcon;              // the low volume icon to display when volume < 33%
    public Sprite midIcon;              // the mid volume icon to display when volume between 33% and 66%
    public Sprite highIcon;             // the high volume icon to display when volume > 66%

    // booleans that determine whether the user is draging the sliders, initially set to false
    bool isDraggingTime = false;
    bool isDraggingVolume = false;

    void Start(){
        videoPlayer.playOnAwake = false;            // prevents auto play
        videoPlayer.Prepare();                      // prepares video for playback

        videoPlayer.loopPointReached += EndReached; // when the video ends, call the EndReached method

        // when the play button is pressed, AddListener calls the method (lambda in-line method)
        playPauseToggle.onValueChanged.AddListener(on => {  
            if (on) TogglePlayPause();              // if switched to play/on, then call the toggle method
            playPauseToggle.isOn = false;           // reset toggle to off so it acts as a button
        });

        UpdatePlayPauseIcon();                      // call update play icon image so that it matches intial video state (paused)

        // similarly, when the -10s button pressed, AddListener calls the in line method below it
        back10Toggle.onValueChanged.AddListener(on => {
            if (on) Seek(-10f);                     // if pressed, call the Seek function with -10f (-10 seconds)
            back10Toggle.isOn = false;              // reset toggle to off so it acts as a button
        });

        // similarly, when the +10s button pressed, AddListener calls the in line method below it
        forward10Toggle.onValueChanged.AddListener(on => {
            if (on) Seek(10f);                      // if pressed, call the Seek function with 10f (+10 seconds)
            forward10Toggle.isOn = false;           // reset toggle to off so it acts as a button
        });

        // when time slider changes, call the method
        timeSlider.onValueChanged.AddListener(HandleTimeSliderValueChanged);

        // when the volume slider changes, call the method
        volumeSlider.onValueChanged.AddListener(HandleVolumeSliderValueChanged);

        // in line method for once video is loaded/prepared
        videoPlayer.prepareCompleted += _ => {
            // starts coroutine on IEnumerator method which allows pauses on "yield return" statements
            StartCoroutine(ProgressUpdater());
            videoPlayer.SetDirectAudioVolume(0, volumeSlider.value);    // sets the volume to initial volume
            UpdateVolumeIcon(volumeSlider.value);                       // sets the volume icon to corresponding icon
        };
    }

    void TogglePlayPause(){
        if (videoPlayer.isPlaying) videoPlayer.Pause(); // if playing, pause
        else videoPlayer.Play();                        // if paused, play

        UpdatePlayPauseIcon();                          // call update icon method
    }

    void UpdatePlayPauseIcon(){
        if (!playPauseIconImage) return;                // if no image return (error)

        // if video ended, then change to replay button
        if (videoPlayer.isPrepared && videoPlayer.time >= videoPlayer.length){
            playPauseIconImage.sprite = replayIcon;
        }
        else {
            playPauseIconImage.sprite = videoPlayer.isPlaying
                ? pauseIcon                             // if playing, change to pause button
                : playIcon;                             // if paused, change to play button
        }
    }
    
    void EndReached(VideoPlayer vp){                    // have this twice for automatic video completetion (when update method not called)
        if (playPauseIconImage) playPauseIconImage.sprite = replayIcon; // when video ends, change to replay icon
    }

    void Seek(float delta){
        double t = videoPlayer.time + delta;                            // adds the time +10s or -10s
        videoPlayer.time = Math.Clamp(t, 0, videoPlayer.length);        // ensures slider not out of bounds and sets it to correct time
        UpdatePlayPauseIcon();                                          // update icon if slider moved to end
    }

    public void HandleTimeSliderValueChanged(float val){
        if (isDraggingTime) videoPlayer.time = val;         // if time slider dragged, set video time to time slider value
    }

    public void HandleVolumeSliderValueChanged(float val){
        if (isDraggingVolume){
            videoPlayer.SetDirectAudioVolume(0, val);       // if volume slider dragged, set volume to volume slider value
            UpdateVolumeIcon(val);                          // update the volume icon
        }
    }

    public void BeginTimeDrag() => isDraggingTime = true;   // when time slider pressed down, set true
    public void EndTimeDrag(){
        isDraggingTime = false;                             // when time slider pressed up, set false
        videoPlayer.time = timeSlider.value;                // change video time to time slider value
    }

    public void BeginVolumeDrag() => isDraggingVolume = true;       // when volume slider pressed down, set true
    public void EndVolumeDrag(){
        isDraggingVolume = false;                                   // when volume slider pressed up, set false
        videoPlayer.SetDirectAudioVolume(0, volumeSlider.value);    // set video volume to volume slider value 
    }

    IEnumerator ProgressUpdater(){
        while (!videoPlayer.isPrepared) yield return null;  // pause coroutine until the video is prepared

        timeSlider.minValue = 0f;                           // sets the video min value to 0
        timeSlider.maxValue = (float)videoPlayer.length;    // sets the video max value to video length

        while (true){
            if (!isDraggingTime){
                float t = (float)videoPlayer.time;          // when slider not touched set t as time
                timeSlider.SetValueWithoutNotify(t);        // update slider value as same as video time

                TimeSpan ct = TimeSpan.FromSeconds(videoPlayer.time);                       // sets ct to current video time
                TimeSpan rt = TimeSpan.FromSeconds(videoPlayer.length - videoPlayer.time);  // sets rt to remaining video time
                timeElapsedText.text = $"{ct:mm\\:ss}";     // formats elapsed text as current video time
                timeRemainingText.text = $"{rt:mm\\:ss}";   // formats remaining text as remaining video time
            }
            yield return null;
        }
    }

    void UpdateVolumeIcon(float v){
        if (!volumeIconImage) return;                               // if no volume image return (error)
        if (v <= 0f) volumeIconImage.sprite = muteIcon;             // if volume 0 set to muted icon
        else if (v <= 0.33f) volumeIconImage.sprite = lowIcon;      // if volume < 33% set to low icon
        else if (v <= 0.66f) volumeIconImage.sprite = midIcon;      // if volume between 33% and 66% set to mid icon
        else volumeIconImage.sprite = highIcon;                     // if volume > 66% set to high icon
    }
}