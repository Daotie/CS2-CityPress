# Changelog / 更新日志

## 0.2.0-beta.10

- Compile editorial material locally before any model request: group construction categories, suppress routine line inventories and low-impact incidents, and exclude unverified statistics groups and units. Keep original journals intact.
- Format population and finance figures, block prefab codes and raw internal fields in finished copy, and apply the same selection instructions to news, commentary and revision tasks.
- Request several distinct opinions when warranted; multiple major domains require at least two accepted opinion pieces. Support extra long opinions and capacity-based continuation editions.
- Add a compact front-page guide with final page references; omit thin minor stories from the guide. Increase room for themed continuation pages.
- Bundle the OFL Ma Shan Zheng font for the UI masthead. Restrict vertical headlines to short Chinese titles and render old unsuitable vertical headlines horizontally in both UI and Word.


## 0.2.0-beta.9

### English

- Added English and Simplified Chinese localization for the in-game interface, status and error messages, edition/theme labels, page dates and Word furniture. The interface follows the game language; other game languages fall back to English. A built-in fallback table is bundled, so a missing or not-yet-loaded language file never renders raw translation keys.
- Added `lang/en-US.json` and `lang/zh-HANS.json` with a shared key set. Translation keys cover UI labels, placeholders, archive text, settings, operation status, API/quality errors, theme names and collection summaries.
- Backend user-facing failures now carry stable `CityPress.Error.*` keys and arguments; operation status uses `CityPress.Status.*`. The UI translates them and keeps the existing Chinese fallbacks for logs, tests and older callers.
- Word export now detects whether the issue is Chinese or English and selects matching furniture, page labels and date format, including English month/weekday names. Existing Chinese issues keep their original furniture.
- Added standard mod metadata (`mod.json`) and packaged `lang/` with the release, so the game and translators can discover the strings. The generated newspaper prose itself still follows the editorial policy; interface language does not auto-translate articles.
- Process/caveat wording no longer rejects an entire issue. It is flagged by the quality gate, sent to the model for one targeted revision, and any phrase that survives is removed sentence-by-sentence before layout. This avoids gambling on a full regeneration and prevents a single boilerplate sentence from costing the whole issue.
- Fixed settings-panel scrolling and input overlap: the settings form now forwards wheel scrolling to the panel, gives inputs an opaque clipped surface and explicit stacking, and uses a scrollable textarea. Long English labels no longer overlap neighbouring fields.
- Validation: full offline suite with **214 checks**, including language-file key parity, no hard-coded Chinese left in the UI entry file, every UI/backend key present, English Word furniture/date, Chinese fallbacks, one-model-revision process-wording repair, and deterministic phrase cleanup fallback. Not verified here: native in-game language switching, Cohtml layout with English strings, and real CI/browser rendering remain pending; the game must be restarted after installing.

### 简体中文

- 为游戏内界面、状态与错误消息、版面/主题标签、报纸日期和 Word 版签增加英文与简体中文。界面跟随游戏显示语言，其他游戏语言回退为英文；界面内置回退词条表，语言文件缺失或尚未加载时不会显示原始键名。
- 新增 `lang/en-US.json` 与 `lang/zh-HANS.json`，两者键集合一致。词条覆盖界面标签、输入占位、档案文案、设置项、操作状态、API/质量错误、主题名称和采集摘要。
- 后端用户可见错误改为携带稳定的 `CityPress.Error.*` 词条与参数，操作状态使用 `CityPress.Status.*`；界面负责翻译，同时保留中文回退文案供日志、测试和旧调用使用。
- Word 导出现在会检测报纸是中文还是英文，并选择对应的版签、版号和日期格式（含英文月份与星期）；已有中文报纸保持原版签。
- 新增标准模组元数据 `mod.json`，发行包附带 `lang/`，便于游戏与译者发现词条。生成的报纸正文仍由编辑方针决定，界面语言不会自动翻译文章内容。
- 制作口径/模板免责措辞不再导致整期拒绝：质量检查先要求模型进行一次针对性改写，仍有残留时在排版前按句清理。这样既避免“整期丢弃、重新生成靠运气”，也避免一句套话浪费整期 Token。
- 修复设置面板滚动与输入框重叠：设置表单会把鼠标滚轮转发给面板滚动，输入框使用不透明裁剪背景与明确层级，长文本域可独立滚动；选择文本时不再叠显其他输入框内容。
- 验证：完整离线测试共 **214 项**，包括中英文词条键一致、界面入口不再硬编码中文、所有界面/后端词条存在、英文 Word 版签与日期、中文回退、制作口径一次模型修订与确定性清理回退等。本环境未验证：原生游戏内语言切换、英文文案在 Cohtml 中的布局、真实 CI/浏览器渲染；安装后需重启游戏。

## 0.2.0-beta.8

### English

- The commentary edition is now a real opinion page. One long commentary occupies the whole or most of the edition and may sit beside up to two short comments. Long commentary can carry 2–4 subheadings: they are written as ordinary body lines starting with `## `, rendered as distinct heading blocks in the game preview and Word (smaller Heading style with keep-with-next), and never repeated in the body text.
- The dedicated commentary request can return `shortComments`. Long and short commentary are validated separately: the long piece must be substantial, analytical and layered; short comments only need a clear viewpoint and are dropped individually if invalid, without failing the issue. A failed long commentary still reports an explicit error and retries once.
- Commentary quality standards follow the role: the lead long commentary is checked for length, analysis, paragraphing and 2–4 (max 6) subheadings of reasonable length; short comments use their own shorter standard instead of being judged as an insufficient long piece. A commentary edition without a lead long piece is rejected when the event requires commentary, so a page of only short notes cannot pass as analysis.
- Page planning no longer limits an edition to one topic. A topic with enough copy continues onto extra editions labelled（续）/（续二）…, pages are split by an editorial capacity (~2400 characters), the paper can carry up to ten news editions, and thin pages are merged only when the limit is exceeded. Rich material therefore gets space instead of being crammed or dropped.
- New `opinion-mixed` layout for a long commentary plus short comments: two balanced main columns carry the long piece and its subheads, a narrow sidebar carries the short comments. Subhead blocks, Word rendering and UI styles were added; existing single-commentary `opinion`, other templates and legacy issues keep their behaviour.
- Cache policy advanced to v8. Old cached editions are not reused; archived issues remain readable and re-exportable in their original layout.
- Validation: full offline suite with **155 checks**, including commentary with subheads plus short comments end to end, role-based quality rules, opinion-mixed composition (main text preserved exactly once, sidebar titles), same-topic continuation editions and multi-page Word export with subheads. Not verified here: browser preview (local sandbox blocks browser spawn) and native in-game display; those still need a game restart and player acceptance.

### 简体中文

- 评论版改为真正的评论专版：一篇长评论占据该版全部或大部分篇幅，可与至多两篇短评同版。长评论可以用 2 至 4 个小标题分层：小标题就是正文中以“## ”开头的单独一行，在游戏预览和 Word 中渲染为独立小标题块（较小字号标题样式并保持与后文同页），不会在正文中重复出现。
- 独立评论请求可返回 `shortComments`。长评论与短评分别校验：长评论必须有分量、有分析、有层次；短评只需观点明确，不合格时单独丢弃，不影响整期。长评论失败仍会明确报错并重试一次。
- 评论质量按角色区分标准：主编长评论检查篇幅、论证、分段和 2 至 4 个（最多 6 个）长度合理的小标题；短评使用较短的独立标准，不再被当作“论证不足的长评论”误判。重大事件需要评论而评论版没有长评论时会被拒绝，只有短评的版面不能冒充深度分析。
- 版面规划不再限制“一版只放一类新闻”。同一主题素材多时会连续加版（版名“经济与财政（续）”“（续二）”等），按约 2400 字的编辑容量拆版，新闻版最多十个；只有超过上限时才合并薄版。素材多时给足版面，不再硬塞或删除。
- 新增 `opinion-mixed` 长评论加短评版式：两栏均衡承载长评论与小标题，窄侧栏放短评。新增小标题内容块、Word 渲染与 UI 样式；原有单篇评论版、其他版式与旧刊行为保持不变。
- 缓存规则升至 v8，旧缓存不重用；历史报纸仍按原版式阅读和重新导出。
- 验证：完整离线测试共 **155 项**，包括“长评论含小标题加短评”的端到端生成、按角色的评论质量规则、长评论加短评版式（正文只出现一次、短评标题在侧栏）、同主题连续加版以及含小标题的多版 Word 导出。本环境未验证：浏览器预览（沙箱禁止启动浏览器进程）与原生游戏内显示；仍需重启游戏并由玩家验收。

## 0.2.0-beta.7

### English

- Reworked the editorial pipeline so the model no longer decides how many editions exist. News value is graded from breakthroughs, transit service changes, actions and metric deltas; a deterministic page planner splits flat or single-"综合" output into themed editions (front news, economy and finance, construction and transport, livelihoods and society, environment and observation) and keeps an independent commentary edition last.
- Fixed the missing-commentary defect at its source: when a major event has enough material, a separate commentary API task now writes an independent article, validates length, analysis, sourcing and overlap with news, retries once with the specific problem, and only then attaches it to its own edition. A failed commentary is reported as a failure and never published as a complete issue; nothing is cached.
- Added an article quality gate that runs before layout: numbers must trace back to collected facts (rounding, 万/亿 and percentage forms included), invented interviews/sources are fatal, mechanical openings/evaluations and duplicated openings are flagged, near-duplicate news is merged out, commentary that reprints news is rejected, and titles/genre/commentary structure are checked. Issues yield a weighted score, per-article audit metadata and at most one targeted revision request; if a fatal fact problem remains, that unsafe article is removed rather than published.
- Added an explicit editorial brief and value grading (routine, notable, major, milestone, critical) covering population/treasury/household/tourism/land breakthroughs, transit openings, public-service and incident records. A bare threshold without other evidence no longer forces a commentary.
- Cache policy advanced to v7, so earlier single-page editions cannot be reused; old archives stay readable and re-exportable with their original layout. Word exports every planned edition with explicit page breaks.
- Fixed a write-path failure: when the sandboxed/network filesystem denies `File.Replace`, JSON and Word writes now fall back to a safe delete-and-move instead of throwing. Empty editions returned by a model are dropped instead of rejecting the whole issue.
- Root cause note for the reported P0 symptoms: the game had been running the already-loaded beta.4 assembly since 10:56 while beta.6 files were only placed on disk at 12:52. The 13:08 issue was therefore produced by beta.4's single-page code, not by beta.6. This release still fixes the real pipeline gaps that beta.6 left: flat-list fallback, no dedicated commentary task, no quality gate, and over-strict rejection without safe degradation. A game restart is required before judging these changes.
- Validation: full offline suite (journal, cache, finance, transit, API compatibility, five layouts, Word, archive reload) plus a new editorial suite covering commentary second pass/failure, themed page planning, fact and attribution checks, deduplication, revision and Word multi-page export. A simulation over the real save's 2027-01-12 journal (486 actions, 813 metrics, one treasury breakthrough) produced five themed editions with a commentary page and a passing fact check. Native in-game display and a real paid model call remain unverified here; browser preview was blocked by the local sandbox (`spawn EPERM`).

### 简体中文

- 重构编辑流程，版面数量不再由模型决定。先从突破、线路服务变化、操作记录和指标变化评定新闻价值，再由确定性版面规划器把模型返回的扁平文章或单一“综合”版拆分为要闻、经济与财政、城市建设与交通、民生与社会、环境与观察等主题版，并始终把独立评论版放在最后。
- 从根源修复“重大事件没有评论版”：当重大事件素材足够时，由**独立的评论生成任务**另发一次 API 请求撰写评论，校验篇幅、分析性、来源与新闻重复度，针对具体问题重试一次后才放入独立评论版。评论失败会明确报错、不缓存，不会把残缺报纸当作完成品发布。
- 新增排版前文章质量检查：数字必须可追溯到采集事实（含四舍五入、万/亿和百分比写法）；虚构采访与消息来源判为严重错误；机械开头、套话、雷同开头被标记；近似重复的新闻被合并删除；抄写新闻的评论被拒绝；同时检查标题、体裁和评论结构。检查给出加权评分、逐篇审计元数据，最多进行一次针对性局部修订；若严重事实错误仍存在，删除该稿件而不是发布。
- 新增编辑简报与事件分级（一般、重要、重大、里程碑、特别重大），覆盖人口/财政/家庭/游客/用地突破、线路开通、公共服务与事故记录。仅有单个阈值、缺少其他事实支撑时不再强行配评论。
- 缓存规则升至 v7，旧的单版成稿不会被复用；历史报纸仍按原版式阅读和导出。Word 按规划后的全部主题版显式分页导出。
- 修复写入路径：沙箱、网络盘或杀软拒绝 `File.Replace` 时，JSON 与 Word 写入回退为安全的删除后移动，不再直接失败。模型返回的空版面会被丢弃，而不是拒绝整份报纸。
- 关于用户报告的两个 P0 现象的真实原因：游戏自 10:56 起一直在运行内存中的 beta.4 程序集，beta.6 文件到 12:52 才写入磁盘，13:08 生成的报纸实际由 beta.4 的单版代码产生，并非 beta.6。本版仍修复了 beta.6 留下的真实缺口：扁平文章回退、没有独立评论任务、没有质量检查、失败即整份拒绝且无安全降级。判断这些改动前必须重启游戏。
- 验证：完整离线测试（日志、缓存、财政、线路、API 兼容、五种版式、Word、归档重载）加新增编辑测试（评论二次生成/失败、主题分版、事实与来源核验、去重、局部修订、Word 多版导出）。用真实存档 2027-01-12 日志（486 条操作、813 项指标、1 项市库突破）模拟，得到五个主题版加评论版并通过事实核验。原生游戏内显示与真实付费模型调用本环境仍未验收；浏览器预览被本地沙箱阻止（`spawn EPERM`），已在交付说明中标注。

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
