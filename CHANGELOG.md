# Changelog / 更新日志

## 0.2.0-beta.2

### English

- Added offline core/cache/Word verification without proprietary game assemblies or API credentials; embedded-template fallback is checked explicitly.
- Added public Windows CI for core tests, UI types/build and runtime dependency audit.
- Full local packaging now runs the game-assembly/export checks and callback harness as required gates. Documented the distinction between offline, harness and live-game validation.
- Game behavior and save envelope are unchanged from beta.1; full in-game collector acceptance remains pending.

### 简体中文

- 核心/缓存/Word 检查可脱离游戏程序集和 API 密钥运行，并验证内嵌模板回退。
- 增加公开 Windows CI，检查核心、UI 类型/构建和运行依赖。
- 完整本地打包增加游戏程序集/导出检查和回调检查门槛，明确离线、模拟与真实游戏验收的区别。
- 相较 beta.1，游戏行为与存档结构保持不变，完整游戏内采集验收仍待完成。

## 0.2.0-beta.1

### English

- First public source-matched beta: manual-by-default DeepSeek generation, optional daily popups, per-save/global newspaper titles and settings, local cache and Word/JSON archives.
- Added meaningful stock gameplay management callbacks, tool submissions, world changes, event updates and city-statistics sampling; excluded automatic vehicle labels, boarding/alighting and navigation noise.
- Shared newspaper layout between in-game preview and editable Word export; live game clock and invalid-date filtering.
- Added GPL-3.0-only, bilingual public documentation, support templates and third-party notices.
- Validation: Release compilation, core/cache/Word tests, callback harness and browser mock checks; expanded in-game collector acceptance remains pending. UI/default publication language: Simplified Chinese.

### 简体中文

- 首个与源码对应的公开 Beta：默认手动生成、可选每日弹窗、单存档/全局报头与设置、本地缓存和 Word/JSON 归档。
- 接入原生管理回调、工具提交、城市变化、事件与统计快照，排除车辆自动改名、上下客和界面导航噪声。
- 统一游戏预览与可编辑 Word 的报纸版式，增加实时时钟并过滤无效日期。
- 添加 GPL-3.0-only、双语公开说明、反馈模板和依赖许可证。
- 验证范围：Release 编译、汇总/缓存/Word 自动检查、回调检查和浏览器模拟；扩大采集的完整游戏内验收待完成。目前界面及默认刊物语言为简体中文。
