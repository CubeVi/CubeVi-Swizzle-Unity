<h4 align="center">
  <img src="doc/src/512x512.png" alt="openstageAI logo" style="width:15%; ">
  
<h1 align="center">CubeVi-Swizzle-Unity</h1>

</h3>

**切换其他语言: [English](README.md), [中文](README_ZH.md).**



[![OpenStageAI](https://img.shields.io/badge/OpenStageAI-web-blue)](https://www.openstageai.com/)
[![Unity](https://img.shields.io/badge/Unity-download-red)](https://unity.com/download)
[![license](https://img.shields.io/badge/license-GPL-green)](https://github.com/CubeVi/CubeVi-Swizzle-Unity/blob/main/license)
[![license](https://img.shields.io/badge/license-commercial-red)](https://github.com/CubeVi/CubeVi-Swizzle-Unity/blob/main/license)
[![Chat](https://img.shields.io/badge/chat-discord-blue)](https://discord.gg/kAucVzbvQM)
 <!-- this badge is too long, please place it in the last one to make it pretty --> 

<p align="center">
    👋 加入我们的 <a href="https://discord.gg/kAucVzbvQM" target="_blank">Discord</a> 
</p>

## 项目介绍
CubeVi_Swizzle_Unity 是进行光场渲染的 Unity SDK。

在 `BatchCameraManager` 中，首先会创建多个相机（最多四十个），每个相机负责渲染不同的视图。相机的设置包括焦距、视口等参数。接着，创建一个 `DisplayCamera` 用于显示最终的渲染结果，并将所有相机的输出合并到一个 `Quad` 上。`Quad` 是一个平面对象，用于在场景中显示渲染后的画面。整个流程如下：
1. 初始化相机并设置其参数；
2. 创建 `DisplayCamera`，用于显示合成后的画面；
3. 创建 `Quad`，并将其材质设置为渲染结果的纹理；
4. 在每一帧更新中，更新相机的位置和焦点，确保渲染结果实时反映场景变化；

## 维护说明
提供 Unity 环境下交织算法的基础使用办法。

## 依赖
- Newtonsoft.Json 库，可在 Package Manager 中通过名称添加 `com.unity.nuget.newtonsoft-json`
- 支持**unity2017**以上版本
- 请请确保您的电脑已经正确连接的[**Companion 01**](https://www.openstageai.com/companion1)设备，教程和OpenstageAI客户端下载请[点击这里](https://www.openstageai.com/download)

## 使用说明与注意事项
1. 将 `BatchCameraManager` 组件挂载在场景中；
2. 设置一个 `Root` 作为宫格相机的中心位置；
3. 设置一个`TargetTransform` 锁定相机组的焦平面；
4. 以上两个变量是必须设置的，完成后运行场景即可

### BatchCameraManager 变量用法：
- `Root`: 设置相机的中心位置；

- `BatchCameraPrefab`: 选择使用的相机预制体；
- `useCameraPrefab`: 是否使用自定义相机预制体；

- `TargetTransform`: 目标物体的Transform，用于锁定焦平面；
- `FocalLength`: 相机的透镜焦距，影响视野范围；
- `useTartgetFocal`: 是否使用目标物体中心作为焦平面；

- `showFocalPlane`: 是否显示焦平面；
- `showFrustumFrame`: 是否显示视锥体；

## 讨论

如果有任何问题或者发现的漏洞请在[这里](TODO)告诉我们

## license

本项目已经添加license










