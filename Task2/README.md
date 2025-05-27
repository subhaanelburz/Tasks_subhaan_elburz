# Task 2: Build and run dragonfly successfully

For this task, I was only able to build dragonfly. I was not able to get dragonfly to run on my machine (The server and client). So, I've included a screenshot listing the contents of the system/build directory as I believe I have done that correcrtly at least.

## Commands used to build Dragonfly

I essentially just ran the same commands stated in the Dragonfly GitHub README with slight modifications:

I first ran this (in WSL Ubuntu 18.04 Terminal), which is the same, except with sudo before the commands:
```
sudo apt-get update  && sudo apt-get install build-essential software-properties-common -y  && sudo add-apt-repository ppa:ubuntu-toolchain-r/test && sudo apt-get update  && sudo apt-get install gcc-9 g++-9 -y  && sudo update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-9 60 --slave /usr/bin/g++ g++ /usr/bin/g++-9
add-apt-repository ppa:jonathonf/ffmpeg-4
apt-get install -y  ffmpeg  libgflags-dev libgoogle-glog-dev libboost-all-dev libavcodec-dev libavformat-dev libswscale-dev libdouble-conversion-dev libfmt-dev libevent-dev libssl-dev cmake  mahimahi
```

Then, after I completed the pre-requisites I cloned the dragonfly repo by doing the following:
```
git clone https://github.com/Purdue-ISL/Dragonfly.git
```

After cloning the repo, I went to the fmt folder and ran the following commands:
```
mkdir build
cd build
cmake ..
sudo make -j2
sudo make install
```

Similarly, I went to the folly folder and ran the following commands:
```
mkdir _build
cd _build
cmake ..
sudo make -j2
sudo make install
```

Lastly, I built Dragonfly using the command
```
cd system && mkdir build && sudo make -f Makefile_ubuntu
```