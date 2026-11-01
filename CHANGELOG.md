# Changelog / 更新日志

## 0.2.0-beta.4

### English

- Added configurable Word/JSON exports under Documents/CityPress, grouped by masthead and save. Open the actual folder from the game or re-export an existing issue without an API request; old files remain readable.
- Added persistent high-water marks for population, treasury, monthly budget income/balance, households, visitors and land. Initial readings establish a baseline; rebounds do not repeat breakthroughs and daily crossings are merged.
- Included breakthroughs in editorial facts and cache keys, with explicit units and budget/stock distinctions. Unlimited-money display values and unexplained native metrics cannot become fiscal milestones.
- Moved export, archive reads and incremental per-day journal writes to workers; indexed aggregation, staged world/statistics/pollution scans and isolated clock updates reduce repeated work.
- Validation: export/migration/cancellation, milestone baseline/rebound/cache, aggregation/snapshot/journal checks, game-assembly callback checks, UI types/build/audit and browser controls at three scales. Native in-game frame-rate and display acceptance remain pending.

### 简体中文

- 可自定义 Word/JSON 导出目录，默认系统文档中的 CityPress，按报头与存档分类。游戏内可打开实际文件夹，已有报纸重新导出不调用 API，旧文件保留可读。
- 人口、市库、月度预算收入/结余、家庭数、游客和用地支持持久化突破检测。首次采样建立基线，回落反弹不重复报道，同日多次越线合并。
- 突破纳入编辑素材与缓存，明确预算/存量及单位；无限资金显示值和未解释的原生指标不冒充财政里程碑。
- 导出、档案读取、分日增量日志改为后台；索引合并、城市/统计/污染分帧扫描和独立时钟减少重复工作。旧 journal.json 保留，游戏存档外层仍为版本 1。
- 验证涵盖导出/旧档/取消、突破/去重/缓存、汇总/快照/日志、实际程序集回调与 UI 类型/构建/审计和三种缩放。原生游戏帧率及显示仍需实测。

## 0.2.0-beta.3

### English

- Fixed missing Chinese glyphs in the newspaper by using the game's registered UI font instead of Windows-only font names. Newspaper layout and editable Word export are retained; exact typefaces can differ by renderer.
- Kept archive summaries as complete text elements so Cohtml cannot spread the date, count and label across the row. Kept the title subtitle on one line and made the panel background opaque.
- Removed unobserved historical initialization days, including a stale default date such as 2021-01-06. Real early-year activity, metric-only history and the current day are retained.
- Hid issues mistakenly generated for those empty initialization days; the suppression persists in save data without deleting local newspaper files.
- Added source/UI/release version consistency checks. Save envelope remains version 1, with an additive empty-day suppression field.
- Validation: core/export and callback checks, UI types/build and browser layout regression checks. Native in-game display still requires confirmation after restarting the updated mod.

### 简体中文

- 报纸改用游戏已注册的中文字体，修复 Windows 字体无法在游戏网页界面加载而出现方框的问题。保留报纸版式和可编辑 Word；不同渲染环境的字形可能存在差异。
- 档案摘要使用完整文本元素，修复日期、记录数量和文字被 Cohtml 分散排布；标题副行保持单行，面板底板改为不透明。
- 清理没有任何观测数据的过去初始化日期，例如遗留的 2021-01-06；保留真实早期操作、仅指标的历史和当前日期。
- 对这些空日期误生成的报纸隐藏档案入口，标记随存档保存，不删除本地报纸文件。
- 新增源码、UI 与发行说明版本一致性检查。存档外层版本保持 1，追加空日期隐藏字段。
- 验证：核心/导出、回调、UI 类型/构建与浏览器排版回归检查；原生游戏显示需更新后重启确认。

## 0.2.0-beta.2

### English

- Added offline core/cache/Word verification without proprietary game assemblies or API credentials; embedded-template fallback is checked explicitly.
- Excluded nested build outputs from assembly discovery, preventing the modern offline test JSON library from shadowing the game's .NET Framework-compatible reference.
- Added public Windows CI for core tests, UI types/build and runtime dependency audit.
- Full local packaging now runs the game-assembly/export checks and callback harness as required gates. Documented the distinction between offline, harness and live-game validation.
- Game behavior and save envelope are unchanged from beta.1; full in-game collector acceptance remains pending.

### 简体中文

- 核心/缓存/Word 检查可脱离游戏程序集和 API 密钥运行，并验证内嵌模板回退。
- 排除嵌套构建产物的程序集扫描，避免现代测试依赖覆盖游戏所需的 .NET Framework 引用。
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
