<h4 align="center">
  <img src="doc/src/512x512.png" alt="openstageAI logo" style="width:15%; ">
  
<h1 align="center">CubeVi-Swizzle-Unity</h1>

</h3>

**Read this in other languages: [English](README.md), [中文](README_ZH.md).**



[![OpenStageAI](https://img.shields.io/badge/OpenStageAI-web-blue)](https://www.openstageai.com/)
[![Unity](https://img.shields.io/badge/Unity-download-red)](https://unity.com/download)
[![license](https://img.shields.io/badge/license-GPL-green)](https://github.com/CubeVi/CubeVi-Swizzle-Unity/blob/main/license)
[![license](https://img.shields.io/badge/license-commercial-red)](https://github.com/CubeVi/CubeVi-Swizzle-Unity/blob/main/license)
[![Chat](https://img.shields.io/badge/chat-discord-blue)](https://discord.gg/kAucVzbvQM)
 <!-- this badge is too long, please place it in the last one to make it pretty --> 

<p align="center">
    👋 Join our  <a href="https://discord.gg/kAucVzbvQM" target="_blank">Discord</a> 
</p>

## Project Introduction
CubeVi_Swizzle_Unity is a Unity SDK for light field rendering.

In `BatchCameraManager`, multiple cameras (up to forty) are first created, with each camera responsible for rendering different views. Camera settings include focal length, viewport, and other parameters. Then, a `DisplayCamera` is created to display the final rendering result, and all camera outputs are merged onto a `Quad`. The `Quad` is a plane object used to display the rendered image in the scene. The entire workflow is as follows:
1. Initialize cameras and set their parameters;
2. Create `DisplayCamera` for displaying the composite image;
3. Create `Quad` and set its material to the rendering result texture;
4. In each frame update, update camera positions and focus to ensure the rendering result reflects scene changes in real-time;

## Maintenance Instructions
Provides basic usage methods for interlacing algorithms in Unity environment.

## Dependencies
- Newtonsoft.Json library, which can be added in Package Manager by name `com.unity.nuget.newtonsoft-json`
- Supports **Unity 2017** and above versions
- Please ensure your computer is properly connected to the [**Companion 01**](https://www.openstageai.com/companion1) device. For tutorials and OpenstageAI client download, please [click here](https://www.openstageai.com/download)

## Usage Instructions and Notes
1. Attach the `BatchCameraManager` component to the scene;
2. Set a `Root` as the center position of the grid cameras;
3. Set a `TargetTransform` to lock the focal plane of the camera group;
4. The above two variables are mandatory. After setting them, run the scene.

### BatchCameraManager Variable Usage:
- `Root`: Set the center position of the cameras;

- `BatchCameraPrefab`: Select the camera prefab to use;
- `useCameraPrefab`: Whether to use custom camera prefab;

- `TargetTransform`: Target object's Transform, used to lock the focal plane;
- `FocalLength`: Camera lens focal length, affects field of view range;
- `useTartgetFocal`: Whether to use the target object center as the focal plane;

- `showFocalPlane`: Whether to display the focal plane;
- `showFrustumFrame`: Whether to display the frustum;