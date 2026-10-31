<p align="center"><a href="README.md">English</a> · <strong>简体中文</strong></p>
<p align="center"><img src="assets/branding/citypress.svg" width="120" alt="CityPress 报纸标志"></p>
<h1 align="center">CityPress 城市日报</h1>
<p align="center">为 <strong>《都市：天际线 II》</strong> 留下每天的城市新闻。</p>
<p align="center">
 <img src="https://img.shields.io/badge/version-0.2.0--beta.2-00a8bb" alt="0.2.0-beta.2">
 <img src="https://img.shields.io/badge/status-beta-orange" alt="Beta 预发行">
 <img src="https://img.shields.io/badge/license-GPL--3.0--only-blue" alt="GPL-3.0-only">
</p>
<p align="center"><a href="https://github.com/Daotie/CS2-CityPress/releases">下载</a> · <a href="https://github.com/Daotie/CS2-CityPress/issues">反馈问题</a> · <a href="CHANGELOG.md">更新日志</a> · <a href="SECURITY.md">隐私说明</a></p>

**0.2.0-beta.2：** 增加便携核心/导出检查与公开 CI，继续提供城市采集、新闻编辑、游戏内阅读和可编辑 Word 归档。见[版本说明](RELEASE_NOTES.md)。目前界面、默认编辑方针和 Word 版签使用简体中文，项目公开文档提供中英双语。

## 主要功能

- 采集有效的建设、拆除、地形和管理操作，同时观察实际城市变化、重要事件和城市统计趋势。
- 覆盖人口、经济、交通、水电、天气、污染等可用指标，重复操作合并保存。
- 排除上下客、车辆自动状态改名、界面导航和空预览。
- 根据真实可用素材交由 DeepSeek 编辑，自主合并、取舍与安排栏目，不为凑版报道琐事。
- 在游戏内打开带纸色背景、报头和分栏的报纸，同时保存可编辑 `.docx` 和 JSON；Word 与游戏预览共用排版参数。
- 报名可随城市名自动变化，也可自定义；设置支持单存档或全局默认。
- **默认手动生成**。可选开启每日自动发行，跨日后编印前一天的报纸并在游戏内弹出。
- 相同事实复用本地成稿缓存，展示 DeepSeek 前缀缓存用量；指标没变化的重复快照不触发重新生成。

## 安装与使用

1. 在 [Releases](https://github.com/Daotie/CS2-CityPress/releases) 下载完整 `CityPress-<版本>.zip`，不要把 GitHub 自动生成的源码包当作安装包。
2. 退出游戏，将安装包完整解压到 `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\CityPress`，在本地模组环境启用后重启。保留 DLL、界面文件、`images/` 与 `Templates/` 的相对位置。
3. 打开报纸按钮，在“编辑部设置”填写 DeepSeek API Key 并保存，点击“生成本日新闻”。自动发行需要自行开启。

生成会将城市事实摘要交给 DeepSeek，并可能产生服务商费用。密钥使用当前 Windows 账户加密保存，不写入城市存档，不回显。分享日志或报纸前请阅读[隐私说明](SECURITY.md)。

数据保存在 `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\CityPress`；报纸位于对应城市目录的 `issues/<日期>.json` 与 `.docx`。更多设置、采集范围与存档说明见[完整使用指南](docs/USER_GUIDE.zh-CN.md)。

## 采集范围与测试状态

操作提交和实际结果分开记录：提交次数不等于建成数量，路网边数不等于整条道路数。载入现有城市先建立基线，不把整座城市报成新建。指标与校对快照每 5 游戏分钟读取一次，原生数据仍遵循游戏自身刷新周期。

第三方模组的独立操作、快照之间的短暂状态和无法读取的内部数据可能缺失。兼容面板显示实际接入情况与采集错误，安装前未观测到的历史不能补采。新闻依据现有素材生成，不能视为经过独立核实的报道。

已完成本地汇总/缓存/导出自动检查、管理回调检查和浏览器界面检查。扩大采集后的完整游戏内验收尚未完成；每个 Beta 都会明确其实际验证范围。

## 开发构建

需要 Windows、.NET SDK 10、Node.js 20+ 和本机游戏程序集。仓库不包含游戏文件。

```powershell
./build.ps1 -ManagedPath '<你的 Steam 游戏库>/steamapps/common/Cities Skylines II/Cities2_Data/Managed'
```

也可设置环境变量 `CSII_MANAGED_PATH`。二次开发见[架构](docs/ARCHITECTURE.md)、[贡献指南](CONTRIBUTING.md)与[发布流程](RELEASING.md)。

不依赖游戏文件或 API 密钥的检查：`dotnet run --project Tests/CityPress.Tests.csproj -- --core-only`。它验证汇总、缓存和 Word 导出，明确跳过游戏程序集集成部分；公开 CI 同时检查 UI 类型、构建与运行依赖。

## 许可证与致谢

Copyright (c) 2026 Daotie and contributors。CityPress 自有代码使用 [GPL-3.0-only](LICENSE)。界面与构建约定参考 [RouteFilter](https://github.com/Daotie/CS2-RouteFilter)，依赖许可证见[第三方声明](THIRD_PARTY_NOTICES.md)。

本项目与 Colossal Order、Paradox Interactive、DeepSeek 无官方隶属关系。GitHub Beta 提供本地安装包，尚未声称上架 Paradox Mods。
