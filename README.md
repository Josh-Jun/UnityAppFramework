# UnityAppFramework

一个面向 Unity 项目的模块化应用框架，核心目标是提供完整的热更新、资源管理、模块开发、编辑器自动化和常用业务能力封装。框架主体采用 HybridCLR 进行 C# 热更新，并结合 YooAsset 完成资源包管理、资源加载和资源热更流程。

## 远程仓库

- GitHub：[Josh-Jun/UnityAppFramework](https://github.com/Josh-Jun/UnityAppFramework)
- Gitee：[Josh-Jun/UnityAppFramework](https://gitee.com/Josh-Jun/UnityAppFramework)

## 框架特点

- 基于 HybridCLR 的高性能 C# 热更新方案
- 基于 YooAsset 的资源包、内置资源和热更资源管理
- 模块化开发结构，推荐以功能模块拆分 Logic 与 View
- 内置 UI、场景、事件、网络、计时、音频、视频、日志等 Master 管理类
- 提供独立可用的工具类，包括文件、事件、单例、UGUI 扩展、对象池、二维码、数据库等
- 集成多种 Unity 编辑器工具，覆盖打包、导表、资源路径生成、脚本模板、同名文件检查等流程
- 支持跨程序集事件调用，便于第三方插件或 UPM 包接入

## 技术栈与依赖

框架主要依赖以下第三方插件：

- [HybridCLR](https://hybridclr.doc.code-philosophy.com/)：Unity 全平台 C# 热更新方案
- [YooAsset](https://www.yooasset.com/docs/Introduce)：Unity 资源管理与热更新系统
- UniTask：Unity 高性能 async/await 异步库
- NativeGallery：Android / iOS 图库与相册交互插件
- NativeFilePicker：Android / iOS 文件交互插件
- UniWebView：Android / iOS 网页内嵌插件
- XCharts：UGUI表格插件
- SuperScrollView：无限滑动列表插件
- StompyRobot（SRDebugger）：真机调试工具

## 目录结构

```text
UnityAppFramework
├── 3rds
│   └── 第三方插件
│
├── Bundles
│   ├── Arts
│   │   └── 原始资源目录
│   │
│   ├── Builtin
│   │   ├── Audios       首包内置音频
│   │   ├── Configs      Excel 导表配置数据
│   │   ├── Dlls         HybridCLR 热更 DLL
│   │   ├── Images       首包内置图片
│   │   ├── Prefabs      首包内置预制体
│   │   ├── Scenes       首包内置场景
│   │   ├── Shaders      Shader 文件
│   │   └── Videos       首包内置视频
│   │
│   └── Hotfix
│       ├── Audios       可热更音频
│       ├── Images       可热更图片
│       ├── Prefabs      可热更预制体
│       ├── Scenes       可热更场景
│       └── Videos       可热更视频
│
├── Plugins
│   └── Unity 插件目录
│
├── Resources
│   ├── AppConfig.asset          应用配置文件
│   └── EnvironmentConfig.asset  开发环境配置文件
│
├── Scripts
│   ├── Builtin
│   │   └── 主要是默认程序集桥接脚本
│   │
│   ├── Core
│   │   └── 核心框架局部类
│   │
│   ├── Module
│   │   ├── Builtin
│   │   │   ├── Ask          弹窗功能
│   │   │   ├── Loading      Loading 加载场景功能
│   │   │   ├── Update       资源热更功能
│   │   │   ├── Background   图片背景功能
│   │   │   ├── Render3D2UI  3D对象渲染在ui上
│   │   │   └── Web          内嵌网页功能
│   │   └── 功能模块代码
│   │
│   └── Runtime
│       └── Runtime 脚本局部类
│
├── Settings
│   └── URP 等项目设置文件
│
└── StreamingAssets
    └── AssetBundles
        └── 内置 AssetBundle 存放目录
```

> 注意：框架核心代码通过 Unity Package 管理，通常不建议直接修改核心包内容。

## 启动流程

### 1. Launcher 启动场景

启动场景负责完成应用进入正式热更场景前的准备工作：

1. 加载 `AppConfig` 配置文件
2. 获取云控数据，可按需配置
3. 显示隐私页面，可按渠道需求开启或关闭
4. 初始化首包内置资源包，必要时自动下载
5. 加载热更脚本
6. 加载 `AppScene` 热更场景

### 2. AppScene 热更场景

`AppScene` 是热更后的主运行场景，入口脚本为 `App.cs`。

核心流程：

1. `App.cs` 初始化 `Root`，并处理 Logic 生命周期相关方法
2. `Root.cs` 初始化程序设置、Logic 脚本，并监听场景加载与卸载回调
3. `UpdateView` 展示热更界面，处理更新确认、取消与退出逻辑
4. `UpdateLogic` 执行资源热更流程
5. 热更完成后调用 `Root.Start()` 正式启动程序

正式启动后会依次完成：

1. 初始化所有 View
2. 初始化 Event 特性
3. 调用全局 Logic 的 `Begin` 生命周期方法
4. 加载主场景

## 热更新与资源管理

资源热更新基于 YooAsset 实现。框架已经封装资源加载、自动打包和常用更新流程，业务层通常通过 `AssetsMaster` 加载资源。

### 资源包

- `BuiltinPackage`：首包内置资源，位于 `Bundles/Builtin`，适合放置热更 DLL、Shader、配置表、基础场景等首次运行必需资源。
- `HotfixPackage`：热更资源，位于 `Bundles/Hotfix`，可选择是否随首包打入应用内。打入首包可减少首次下载等待，不打入首包可降低包体大小。

### 资源加载

常用加载方式：

- `AssetsMaster.LoadAssetSync`：同步加载资源
- `AssetsMaster.LoadAssetAsync`：异步加载资源

资源路径会自动生成到 `AssetPath.cs`，业务代码建议通过自动生成的路径常量访问资源。

## 核心功能

### Helper

- `AppHelper`：程序全局变量
- `Attributes`：框架自定义特性
- `Enums`：框架枚举
- `Interfaces`：框架接口

### Master 管理类

- `Config`：导表后自动生成的 C# 数据类，支持按 ID 获取、判断是否存在、获取全部配置
- `DebugMaster`：本地日志文件生成
- `GM` / `GM.Debug`：真机调试、日志查看和 GM 命令测试
- `HttpsMaster`：HTTP / HTTPS 请求管理，支持接口拆分、Token、数据库与文件下载缓存
- `Socket`：局域网 TCP / UDP 与基于 Protobuf 的网络通信
- `WebSocket`：多 WebSocket 连接管理
- `PlatformMaster`：平台抽象与原生消息接收
- `TimeTaskMaster`：延迟任务与定时任务
- `TimeUpdateMaster`：任意位置开启 Update / FixedUpdate / LateUpdate 回调
- `ViewMaster`：View 初始化、打开、关闭、返回、红点、3D 空节点等 UI 管理
- `AudioMaster`：背景音乐、音效、录音、音频流播放
- `AssetsMaster`：配置加载、资源加载、预制体添加
- `EventMaster`：事件注册、初始化、触发、移除
- `PlayableMaster`：动画创建、播放与移除
- `SceneMaster`：场景加载与返回上一场景
- `VideoMaster`：视频播放与帧图获取
- `XMaster`：跨程序集事件调用，常用于第三方插件桥接

### Tools 工具类

- `AndroidStatusBar`：Android 原生状态栏设置
- `SQLiteDataBase`：数据库工具
- `Event`：动画事件、碰撞事件、UI 事件、RectTransform 监听、全局事件分发
- `File`：AES、音频转换、文件操作、MD5、图片处理、XML 序列化
- `Log`：统一日志封装
- `ObjectPool`：对象池
- `QRCodeTool`：二维码生成与读取
- `Singleton`：C# / Mono 单例及事件单例
- `UGUI`：日期选择器、循环列表、树形菜单、摇杆、图片圆角、ScrollView 嵌套、滑动刷新、输入框缩放等 UI 扩展
- `UnityWebRequester`：HTTP 请求工具
- `Utils`：全局通用方法

## 模块开发规范

框架推荐按功能模块进行开发。一个功能通常包含：

- `Logic`：负责逻辑、数据处理与事件注册
- `View`：挂载在 GameObject 上，负责组件获取、数据显示与界面事件
- `Item`：用于列表项或局部 UI 单元，负责自身组件与局部逻辑

### View 脚本

- 主要负责获取 View 对象下的组件，并为组件添加事件
- 自动生成后通常无需大量修改
- 原则上不写核心业务逻辑，可保留少量 View 层相关逻辑

### Logic 脚本

Logic 构造方法通常用于注册事件。自动创建的 Logic 会默认添加 View 按钮等事件注册逻辑。

生命周期方法：

- `Begin`：Global Logic 只触发一次；场景 Logic 在首次进入场景时触发
- `End`：Global Logic 在退出程序时触发；场景 Logic 在退出场景时触发
- `AppPause`：移动端切到后台时触发
- `AppFocus`：应用获取或失去焦点时触发
- `AppQuit`：程序退出时触发
- `Open******View`：显示对应 View 时触发
- `Close******View`：隐藏对应 View 时触发

### Item 脚本

- 主要负责获取 Item 对象下的组件
- 可以包含与 Item 相关的逻辑代码

## 自动构建模块脚本

自动构建工具会识别命名规则并生成对应脚本：

- 以 `View` 结尾的 GameObject 会被识别为 View
- 以 `Item` 结尾的 GameObject 会被识别为 Item
- 需要在脚本中生成组件变量的对象，应以 `LV_` 前缀命名
- 创建脚本前建议先将 View 对象做成预制体，并更新 `AssetPath.cs`

使用注意：

- 已经存在对应 Logic 和 View 脚本的 GameObject，不建议继续在 Hierarchy 面板修改 View 特性
- View 子物体命名后缀避免包含 `View`
- 带 `LV_` 前缀的对象尽量避免重名，跨 View 重名可能造成事件同时触发

## 编辑器工具

框架提供多种基于 Unity UIToolkit 的编辑器工具，并整合到菜单栏和 Project Settings 中。

| 工具 | 功能 |
| --- | --- |
| AutoPlay Launcher | 无论当前处于哪个场景，运行时自动切到 Launcher，停止后回到原场景 |
| RestoreGameView | 编辑器运行时自动适配横竖屏 GameView |
| UpdateAssetPath | 更新资源路径字符串常量文件 `AssetPath.cs` |
| UpdateAssetPackage | 更新资源包名称字符串常量文件 `AssetPackage.cs` |
| Protobuf2CS | 将 `Tools/protobuf/proto` 下的 proto 文件转换成 C# 类 |
| UpdateKeystore | 更新 Android 签名别名与密码配置 |
| CopyTemplateScripts | 复制 Logic / View 脚本模板到 Unity 安装目录 |
| BuildApp | 编译热更 DLL、构建资源、构建应用、一键打包 |
| BuildConfig | 将 Excel 配置导出为 C# 数据类和 json / xml 数据 |
| CheckUpSameFileName | 检查指定目录下的同名文件 |
| DoTween Easing | DoTween 动画 Ease 类型演示 |
| EditorIcon | 查看和使用编辑器图标 |

## 导表工具

导表工具用于将 `Data/excel` 目录下的 Excel 文件导出为 C# 数据类和 json / xml 数据文件。

主要配置：

- `ConfigMold`：导表类型，可选择 json 或 xml
- 左侧列表：Excel 文件
- 右侧列表：当前 Excel 文件内的 Sheet

自动生成的数据类常用方法：

- `Get(int id)`：获取指定 ID 的数据
- `Contains(int id)`：判断指定 ID 是否存在
- `GetAll()`：获取全部数据

## 打包工具

打包工具可完成应用配置、热更 DLL 编译、资源构建与应用构建。

常用配置：

- `EnableLog`：日志开关
- `DevelopmentMold`：开发环境，支持 Sandbox、Test、Local、Release
- `AssetPlayMode`：资源加载模式，支持 EditorSimulateMode、OfflinePlayMode、HostPlayMode、WebPlayMode
- `IsFullBuiltinPackage`：是否全量包
- `AppFrameRate`：默认帧率
- `ChannelPackage`：渠道包
- `ExportProject`：是否构建原生项目
- `CloudCtrlCode`：云控版本号
- `OutputPath`：构建输出目录

常用操作：

- `ApplyConfig`：保存项目设置
- `GenerateAndCopyDll`：编译并拷贝热更 DLL
- `BuildAsset`：构建 AssetBundle
- `BuildApp`：构建应用
- `GenerateAndCopyDll & BuildAsset`：编译 DLL 并构建资源
- `OneKeyBuild`：一键打包

## 第三方插件接入

由于框架核心代码通过 Unity Package Manager 管理，导入第三方插件或新的 UPM 包时可能无法直接添加程序集引用。框架提供了跨程序集事件调用方案来处理这类场景。

接入流程：

1. 创建跨程序集事件的 ScriptableObject 配置表
2. 如事件需要参数，则创建继承自 `CrossAssemblyEventDataBase` 的数据类，这些类必须放到App.Runtime程序集下
3. 调用端通常在 `Modules` 脚本中通过 `XMaster.Execute` 传入配置表资源路径
4. 第三方功能在默认程序集或 `Builtin` 目录中创建桥接脚本
5. 桥接脚本注册监听事件，并根据参数执行第三方插件能力
6. 如需回调，可在事件数据类中增加回调参数

## 快速使用建议

1. 使用 Unity 打开项目
2. 检查 `Resources/AppConfig.asset` 与 `Resources/EnvironmentConfig.asset`
3. 在 Launcher 场景启动项目
4. 根据项目需求配置 YooAsset 资源加载模式
5. 将基础资源放入 `Bundles/Builtin`
6. 将可热更资源放入 `Bundles/Hotfix`
7. 使用编辑器工具更新 `AssetPath.cs` 和 `AssetPackage.cs`
8. 按模块创建 Logic、View 和 Item 脚本
9. 使用打包工具构建热更 DLL、资源包和应用

## 开发约定

- 功能模块建议独立拆分，减少多人协作冲突
- View 层负责表现与组件绑定，Logic 层负责数据与业务逻辑
- 资源路径优先使用自动生成的 `AssetPath.cs`
- 热更资源优先通过 `AssetsMaster` 加载
- 第三方插件调用优先使用 `XMaster` 跨程序集事件桥接
- 日志建议统一使用框架封装的 Log 工具，方便开关和输出管理
