# Circuit Emulator for CS498 IoT - UNITY

This repo is the repo for JUST the unity VR frontend! If you want to access the rest of the codebase, you should be using the iot-virtualization repo, not this iot-virtualization-unity repo.

## Important Notes

- If you have trouble cloning this repository due to the connection terminating, please use `SSH` instead of `HTTPS` for the remote URL. You may need to setup ssh keys on your computer. Please see [this link](https://docs.github.com/en/free-pro-team@latest/github/authenticating-to-github/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent) for help on setting that up
- **This repo should not be used for issues**. We are enforcing this for consistency reasons to make sure all issues are tracked in the same place. Opening issues here will only create the potential for confusion and information being out of date. Please open and manage issues in the [main repo](https://github.com/mccaesar/iot-virtualization)

## Download Builds
### For Windows
  1. Download: https://tinyurl.com/yrcdk8xp
  2. Decompress and run the executable.

### For Mac
  1. Download: https://tinyurl.com/yrcdk8xp

## How to install:
### Unity
Unity Version : 2019.4.14f(REQUIRED) and 2020.3.1f(RECOMMENDED)

a. Download with unity hub: (Recommended if multiple versions of Unity are used)
  1. Download Unity Hub
  2. Go to https://unity3d.com/get-unity/download/archive
  3. Find version 2019.4.14f and select download with Unity Hub<br />
  4. Claim your personal license from Unity Hub if you do not own one
  5. Install and open the project folder with Unity Editor

b. Download installer: (This will remove the old version of Unity installed)
  1. Go to https://unity3d.com/get-unity/download/archive
  2. Find version 2019.4.14f and select download Unity Installer
  3. Open the installer and follow the instructions
  4. Open and run the project in the Unity installed

## Full walkthrough of user experience (as of 8/16/2021)
1. Connect to university VPN
2. Go to LoginPC scene (OR open the executable)
3. Go to <https://iot.actuallycolab.com/login> and log in to claim the temp code
4. Use the default IP address, follow instructions in the scene and log in
5. Click Play

There is also a project README in src/Assets for more details about the project itself.

## Important notes for WebGL build:
To speed up webGL build for testing purpose:
### For windows users
1. Add iot-virtualization-unity project folder to exclusion list of Windows anti-malware scans.
   Please follow the procedure at https://support.microsoft.com/en-us/windows/add-an-exclusion-to-windows-security-811816c0-4dfd-af4a-47e4-c301afe13b26#:~:text=Go%20to%20Start%20%3E%20Settings%20%3E%20Update,%2C%20file%20types%2C%20or%20process

### For All users
1. In Unity Editor, set File/Build Settings/Player Settings/Player/Publishing Settings/Compression Format to disable.
2. Note: If you want to release webGL build instead of for testing purpose, please change setting in #1 from disable to GZip
