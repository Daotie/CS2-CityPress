# CityPress 0.2.0-beta.2

## English

- Added game-free core/cache/Word checks and embedded-template fallback coverage.
- Fixed nested test outputs shadowing the game's JSON assembly during the .NET Framework mod build.
- Added public Windows CI for core tests, UI types/build and runtime dependency audit.
- Full local packaging now requires game-assembly/export and callback harness checks. Build instructions document which checks need the game.

Gameplay, the Chinese UI, editorial behavior and version-1 save envelope remain unchanged from beta.1. Expanded collection still needs a complete live-game acceptance pass; CI does not run the proprietary game. Generation remains manual by default and DeepSeek receives the eligible facts only when generating.

Install the complete `CityPress-0.2.0-beta.2.zip` in the local `Mods/CityPress` directory while the game is closed; keep `Templates/` and `images/`. Restart after upgrading. The accompanying `.sha256` verifies the archive. Existing journals, preferences and issues remain in `ModsData/CityPress`.

## 简体中文

- 新增不依赖游戏文件的核心/缓存/Word 检查，覆盖内嵌模板回退。
- 修复嵌套测试产物覆盖游戏 JSON 程序集、导致 .NET Framework 模组编译失败的问题。
- 新增公开 Windows CI，检查核心、UI 类型/构建和运行依赖。
- 完整本地打包必须通过游戏程序集/导出与回调检查；构建说明明确各项检查的适用范围。

游戏行为、中文界面、编辑逻辑与版本 1 存档结构保持不变。扩大采集仍待完整真实游戏验收，CI 不运行游戏。默认手动生成，仅在生成时向 DeepSeek 发送有效素材。

退出游戏后将完整 `CityPress-0.2.0-beta.2.zip` 解压到本地 `Mods/CityPress`，保留 `Templates/`、`images/`，升级后重启。随包 `.sha256` 可验证下载文件；原有日志、设置和报纸位于 `ModsData/CityPress`，继续保留。
