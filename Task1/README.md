# Task 1: VR Video Player that plays videos streamed by a server

## Prerequisites

- Unity 2022.3.x LTS installed with the following modules to run the Unity Project: (REQUIRED)
    - DEV TOOLS
        - Microsoft Visual Studio Community 2022
    - PLATFORMS
        - Android Build Support
            - OpenJDK
            - Android SDK & NDK Tools
        - Windows Build Support (IL2CPP)
- Python 3 to start the http server (REQUIRED)
- FFmpeg to convert the source videos to 1 .mpd file (OPTIONAL: don't need to use ffmpeg since I've already done it in the dash folder, but need if wanna change stuff)

## Installing Prerequisites

- Unity:
    - Install using the Unity Hub which can be found here: https://unity.com/download
    - Then open Unity Hub, add installs, and choose latest 2022.3.x version with the modules stated earlier
- Python (using WSL Ubuntu terminal):
    ```
    sudo apt update
    sudo apt install -y python3 python3-pip
    python3 --version
    pip3 --version
    ```
- FFmpeg (using WSL Ubuntu terminal):
    ```
    sudo apt update
    sudo apt install -y ffmpeg
    ffmpeg -version
    ```

## How to run/build the Project

1. Start Python HTTP Server
    ```
    cd Task1/dash_workspace/dash
    python3 -m http.server 8000
    ```

2. Open the Unity Hub, press Add -> Add project from disk -> add unity_project folder

3. Ensure that the Unity Project opens up to the "New Scene" Scene which consists of the following components:
    - Directional Light
    - OVRCameraRig
    - MetaSmallRoomGround
    - PCM
    - vr_video_player
    - DASHManager
    - FlatUnityCanvas

4. To test the scene in unity, first set the play mode to use the Meta XR Simulator, then press play, which should open the Meta XR Simulator, which will allow you to test the video player without using a VR headset 

5. If testing on an actual Meta VR headset, go to file -> Build and Run -> Save .apk file -> Install .apk file on headset -> Open and test .apk file on VR Headset

## Other notes

### FFmpeg
- For FFmpeg, the command I used to convert the 3 source videos to 1 .mpd file was
    ```
    ffmpeg \
    -i 144p.mp4 \
    -i 360p.mp4 \
    -i 720p.mp4 \
    -map 0 -b:v:0 250k \
    -map 1 -b:v:1 800k \
    -map 2 -b:v:2 2500k \
    -use_template 1 -use_timeline 1 \
    -seg_duration 2 \
    -f dash output_video.mpd
    ```

### Video Player
- If you look at the video player components, the video source is from the local server's URL to the manifest video file (in DASHManager script), which satisfies the requirement of the video being stored on a server and not locally
- Additionally, all of the scripts are commented and explained: DASHManager, SafetyFactorDisplay, VRVideoController

### Safety Factor Slider
- This slider was added to basically replicate your internet speed slowing down, the lower it is, the worse the quality it is. For me testing it, 144p would be at like sf = 0.01, 360p would be at like sf = 0.09, and anything above that would be 720p, so it is not the best, but it does work to replicate "slow" internet