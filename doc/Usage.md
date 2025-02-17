# BatchCameraManager 使用文档

## 简介
`BatchCameraManager` 是一个用于批量管理相机的Unity脚本。它可以根据给定的参数创建和管理一组相机，并将这些相机的渲染结果合并到一个网格纹理中。此外，它还可以显示视锥体框架和焦平面，有助于调试和可视化。

## 功能概述
- 批量创建和管理相机。
- 将多个相机的渲染结果合并到一个网格纹理中。
- 显示视锥体框架和焦平面。

## 集成步骤

1. 将 `BatchCameraManager.cs` 脚本添加到你的Unity项目中。
2. 创建一个空的GameObject，并将该脚本附加到这个GameObject上。
3. 在Unity编辑器中设置脚本的公共变量。
4. 将必要的Shader和预制件添加到项目中。

## 公共变量说明

### Batch Camera Settings
- **Root**: 作为所有相机和目标物体的父节点的Transform。
- **BatchCameraPrefab**: (可选) 如果`useCameraPrefab`设置为true，则使用该预制件来实例化相机。
- **useCameraPrefab**: 布尔值，决定是否使用预制件来实例化相机。

### Batch Camera Foces Settings
- **TargetTransform**: 作为焦平面目标的Transform。
- **FocalPlane**: 焦平面的距离，范围为0.1到500.0。
- **useTartgetFocal**: 布尔值，决定焦平面是否使用`TargetTransform`的位置。

**注意**: 代码中`useTartgetFocal`拼写错误，应该是`useTargetFocal`。

### Focus and Frustum
- **showFocalPlane**: 布尔值，决定是否显示焦平面。
- **showFrustumFrame**: 布尔值，决定是否显示视锥体框架。

## 示例用法

### 1. 创建根节点和目标节点
- 在Unity编辑器中创建两个空的游戏对象，分别命名为`Root`和`Target`。
- 获取这两个游戏对象的Transform，并在`BatchCameraManager`脚本中设置`Root`和`TargetTransform`。

### 2. 配置预制件
- 如果你希望使用预制件来实例化相机，创建一个Camera预制件，并命名为`BatchCameraPrefab`。
- 在`BatchCameraManager`脚本中设置`BatchCameraPrefab`，并将`useCameraPrefab`设置为true。

### 3. 显示视锥体框架和焦平面
- 将`showFrustumFrame`设置为true，以在场景中显示视锥体框架。
- 将`showFocalPlane`设置为true，以在场景中显示焦平面。

### 4. 运行项目
- 在Unity编辑器中点击播放按钮，查看批量相机的效果。
- 如果你在编辑器中运行项目，默认情况下输出将显示在第二个显示器上。
- 如果在运行时环境中运行项目，脚本将自动查找分辨率为1440x2560的显示器并输出到该显示器。
  
## 日志
- 使用 `SwizzleLog` 记录信息、警告和错误。
- 在编辑器模式下，默认输出到第二个显示器。
- 在运行时模式下，如果找不到分辨率为1440x2560的显示器，脚本将禁用自身。

## 注意事项
- 确保 `CustomRenderTexture/MultiView` Shader 已添加到项目中。
- 确保 `OpenstageAI/deviceConfig.json` 文件存在且格式正确。
- 代码中存在一个拼写错误（`useTartgetFocal`），请更正为`useTargetFocal`。

## 联系
如果你有任何问题或建议，请通过以下方式联系：

- 邮箱: support@fitten-tech.com
- GitHub: https://github.com/fitten-tech/Swizzle_SDK

