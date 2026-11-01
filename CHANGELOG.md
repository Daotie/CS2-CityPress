# Changelog / 更新日志

## 0.2.0-beta.6

### English

- Added configurable Chat Completions-compatible endpoint/model, optional bearer authentication and JSON response-format mode, output-token parameter/limit and request timeout. DeepSeek is still the initial default; local compatible endpoints can use HTTP without authentication. Native non-compatible protocols are not implemented.
- Bound encrypted credentials to normalized request endpoints. Legacy DeepSeek key/environment fallback is limited to official HTTPS DeepSeek paths. API addresses/credentials never enter editorial prompts; automatic redirects are disabled. Provider/model/output/format/layout settings isolate caches; unknown usage is not reported as zero cache usage.
- Added an independent long-commentary option, requesting a developed argument in natural paragraphs, normally 700–1400 Chinese characters when material supports it. Commentary moves to its own edition; major-event issues with this option need both news and a dedicated commentary page.
- Added five substantive templates: banner, lead/sidebar, vertical headline, digest and long commentary. Content-based or suitable-template variation chooses once at generation. Saved block/column plans drive both UI and Word, with no redraw-time randomness or duplicated long-article bodies. Legacy issues retain their layout.
- Verified offline custom-API/auth/cache/usage/error/legacy tests, complete body preservation for each template, native callback harness, UI types/build/audit, five-template browser controls/scales and five rendered Word pages. Real provider calls and native game display remain pending.

### 简体中文

- 可配置 Chat Completions 兼容接口地址、模型、Bearer 鉴权、JSON 模式、输出参数/上限与等待时间。初始默认仍为 DeepSeek；本地兼容服务可使用 HTTP 并关闭鉴权，不声称支持其他原生协议。
- 密钥按归一化后的请求地址加密隔离；旧 DeepSeek 密钥/环境变量仅用于其官方 HTTPS 地址。地址和凭据不写入提示词，禁止自动重定向。服务/模型/输出/格式/版式参数进入缓存隔离，用量缺失时不假报缓存。
- 独立长评论选项要求有完整论证和自然段，素材足够时通常700至1400汉字，不强行注水。评论单独成版，重大事件须同时有消息与评论版。
- 增加通栏头条、侧栏主稿、纵向头条、综合短讯、长评论五种模板。按内容或合适模板变换选择，生成时固定保存。游戏与 Word 使用同一栏宽/内容块计划，长文连续分栏不重复，旧刊沿用旧排版。
- 完成离线 API/鉴权/缓存/用量/错误/旧刊测试、模板正文完整性、原生回调检查、UI 类型/构建/审计、五模板浏览器交互/缩放和五版 Word 渲染。真实服务请求与原生游戏显示仍待验收。

## 0.2.0-beta.5

### English

- Added dynamically themed editions with news, briefs, analysis and commentary. Topic count follows the available material instead of mandatory filler pages. In-game page navigation and Word page breaks share page order, themes, headlines and article content; legacy issues remain readable.
- Rewrote the default editorial policy for publishable real-world newspaper copy. Technical collection caveats stay internal; incomplete conclusions are omitted. Recognizable process/disclaimer phrases are rejected before caching. Existing stock policies upgrade; custom editorial instructions remain intact.
- Added independent, staged one-game-minute passenger-line checks: actual names, transport modes, distinct stopping points, fares and service evidence. Enabled routes or assigned vehicles alone do not establish operation; testing/returning/dummy/disabled vehicles are excluded. Same-day opening facts survive later fare changes. Baselines and routine vehicle gaps cannot create fake openings.
- Fixed monthly budget balance: native expenses are positive and must be subtracted from revenue. Earlier incorrect balance samples/highs are excluded from new editorial input by using a corrected metric key; original archives are retained. Cache policy advanced to v5.
- Validation: core/cache/finance/transit/legacy compatibility, game-assembly callback harness, UI types/build/audit, browser page navigation and three scales, and three rendered Word theme pages. Native game/save acceptance and real DeepSeek editorial output remain untested; test responses are offline fixtures.

### 简体中文

- 新增主题分版，消息、简讯、分析和评论明确标识；版面按素材组织，不强制凑齐栏目。游戏翻版与 Word 主题分页共用版序、版名、头条和文章，兼容旧刊。
- 重写默认编辑方针，按现实报纸专业语言成稿，采集限制只作内部判断，缺乏依据的结论直接舍弃；技术/免责模板措辞在缓存前拦截。旧默认方针自动升级，自定义保留。
- 客运线路独立分帧检查，完成后间隔一游戏分钟。记录原名、方式、去重停靠点、票价与实际服务；仅启用/配车不作为运营，排除试车、返场、虚拟及停用车辆。后续调价不覆盖当日开通，既有基线和车辆短暂无服务不冒充新开通。
- 修正财政结余：原生支出为正数，改为收入减支出。旧错误结余指标及其突破不再进入新稿，原档案保留；正确指标重新建立高位基线，缓存规则升至 v5。
- 验证：核心/缓存/财政/线路/旧刊、实际程序集回调、UI 类型/构建/审计、翻版/三种缩放和三版 Word 渲染。原生游戏实测与真实 DeepSeek 成稿验收待完成，自动测试使用离线样本。

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
