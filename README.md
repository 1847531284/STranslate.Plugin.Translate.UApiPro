# STranslate.Plugin.Translate.UApiPro

基于 [UApiPro](https://uapis.cn) API 的 [STranslate](https://github.com/zggsong/stranslate) 翻译插件。

## 介绍

### STranslate

[STranslate](https://github.com/zggsong/stranslate) 是一款开源、免费、即开即用、即用即走的 Windows 翻译软件，支持多种翻译服务（百度、有道、DeepL、Google、OpenAI 等），提供划词翻译、输入翻译、截图 OCR 翻译、悬浮窗等功能，并支持通过插件扩展更多翻译服务。

### UApiPro

[UApiPro](https://uapis.cn) 是一个聚合多种 AI 能力的 API 服务平台，提供翻译、对话、图像等接口。其翻译接口支持 100+ 语言互译，源语言自动检测，无需指定源语言即可翻译。

- **官网**：<https://uapis.cn>
- **控制台**（获取 API Key）：<https://uapis.cn/console>
- **计费**：每 3500 积分/次，访客每月拥有 1500 积分的免费额度

## 功能

- 接入 UApiPro 翻译接口，支持 100+ 语言
- 源语言自动检测
- 支持 API Key 认证（付费）与免费层（按 IP 限流）
- 设置页提供连通性校验按钮
- 卡片式 UI，与 STranslate 内置插件风格一致
- 支持简体中文、繁体中文、英语三种界面语言

## 安装

### 方式一：下载编译好的 spkg（推荐）

1. 前往 [Releases](https://github.com/xiaopeng66/STranslate.Plugin.Translate.UApiPro/releases) 下载最新 `STranslate.Plugin.Translate.UApiPro.spkg`
2. 打开 STranslate → 设置 → 插件管理
3. 点击「导入」→ 选择下载的 `.spkg` 文件
4. 在翻译服务列表中添加「UApiPro」并填入 API Key（可选）

### 方式二：自行编译

```bash
git clone https://github.com/xiaopeng66/STranslate.Plugin.Translate.UApiPro.git
cd STranslate.Plugin.Translate.UApiPro
dotnet build -c Release
```

编译产物位于 `bin/Release/net10.0-windows7.0/`，将其打包为 zip 后重命名为 `.spkg` 即可导入。

## 配置说明

| 配置项 | 说明 |
|--------|------|
| **API Key** | 在 [uapis.cn/console](https://uapis.cn/console) 获取。付费接口必填，访客用户可不填。 |
| **官网** | 点击设置页链接跳转 [uapis.cn](https://uapis.cn) 注册使用。 |
| **校验** | 测试 API 连通性，发送翻译请求验证 Key 是否有效。 |

## 技术栈

- .NET 10 / WPF
- [STranslate.Plugin](https://github.com/zggsong/stranslate) SDK
- [iNKORE.UI.WPF.Modern](https://github.com/iNKORE/UI.WPF.Modern) 卡片式 UI
- CommunityToolkit.Mvvm（MVVM 模式）

## 目录结构

```
STranslate.Plugin.Translate.UApiPro/
├── Languages/                # 多语言资源
│   ├── en.xaml / en.json
│   ├── zh-cn.xaml / zh-cn.json
│   └── zh-tw.xaml / zh-tw.json
├── View/
│   ├── SettingsView.xaml     # 设置页 UI（SettingsCard 卡片）
│   └── SettingsView.xaml.cs
├── ViewModel/
│   └── SettingsViewModel.cs  # 设置页逻辑（绑定、校验命令）
├── Main.cs                   # 插件主类（翻译逻辑）
├── Settings.cs               # 配置模型
├── plugin.json               # 插件清单
├── icon.png                  # 插件图标
└── STranslate.Plugin.Translate.UApiPro.csproj
```

## 许可证

[MIT](LICENSE)

## 致谢

- [STranslate](https://github.com/zggsong/stranslate) - 由 zggsong 开发的 Windows 翻译软件
- [UApiPro](https://uapis.cn) - API 服务平台
- [iNKORE.UI.WPF.Modern](https://github.com/iNKORE/UI.WPF.Modern) - 现代 WPF UI 控件库
