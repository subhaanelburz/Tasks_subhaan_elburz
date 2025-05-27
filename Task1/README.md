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

3. Ensure that the 