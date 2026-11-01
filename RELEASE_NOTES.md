# CityPress 0.2.0-beta.7

## English

Editorial-pipeline release: independent commentary as a real generation task, deterministic themed editions and an article quality gate.

- **First, why the previous report looked broken.** The running game had loaded CityPress 0.2.0-beta.4 at 10:56 and never restarted; the beta.6 files were only copied to `Mods/CityPress` at 12:52. The 13:08 newspaper was produced by the old in-memory beta.4 code, which has no page model and no commentary at all. This release still fixes the real gaps that beta.6 left, and **the game must be restarted before testing it**.
- The model no longer controls the number of editions. Breakthroughs, transit service changes, management actions and metric deltas are graded into news-value levels; a deterministic planner turns flat output (or a single "综合" edition) into themed editions and keeps the commentary edition separate and last. A single-domain or very thin day stays a single page instead of being padded.
- Major event with enough material = news + independent commentary. The commentary is a **separate API request** with its own contract; it is validated for length, analysis content, sourcing and overlap with news, retried once with the concrete problem, and then attached as its own article and page. If it still fails, generation reports an explicit error and does not cache or publish a news-only issue.
- New quality gate before layout: numbers must trace back to observations (including rounding, 万/亿 and percentage forms); fabricated interviews, attributable claims and official-sounding sources are fatal; repeated openings, mechanical evaluations, duplicated stories and commentary that reprints news are detected. Findings are scored, stored as per-article audit metadata, and at most one targeted revision request is made. A fatal fact problem still present after revision removes the unsafe article instead of publishing it.
- Cache policy is now v7. Earlier cached single-edition issues are not reused; archived issues remain readable and re-exportable in their original layout. Word exports all planned editions with explicit page breaks.
- Robustness: atomic replacement falls back to a safe delete-and-move when the filesystem denies `File.Replace`; empty editions returned by a model are dropped rather than failing the whole issue.
- Verified offline: core/cache/finance/transit/API-compatibility/layout/Word/archive tests, plus a new editorial suite (commentary second pass and failure, themed planning, fact and attribution checks, dedupe, one targeted revision, multi-page Word). A simulation over the real save's 2027-01-12 journal (486 actions, 813 metrics, one treasury breakthrough) produced five themed editions plus a commentary page and passed fact checks. **Not verified here:** native in-game display and a real paid model call; browser preview was blocked by the local sandbox (`spawn EPERM`). Treat those as pending player acceptance, not as done.
- Install: exit the game, extract the full `CityPress-0.2.0-beta.7.zip` into `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\CityPress`, then **restart the game** so the new assembly is loaded. Existing exports, archives, journals and keys are preserved. Re-generate a day to use the new pipeline; re-exporting an old issue does not rewrite its content.

## 简体中文

编辑流程版本：独立评论成为真正的生成任务，主题版面由确定性规划器决定，并加入文章质量检查。

- **先说明此前现象的原因。** 游戏在 10:56 已加载 CityPress 0.2.0-beta.4，之后一直没有重启；beta.6 文件到 12:52 才复制进 `Mods/CityPress`。13:08 生成的报纸由内存中的旧 beta.4 代码产生，而 beta.4 没有分版模型和评论功能。本版仍修复 beta.6 留下的真实缺口，并且**必须先重启游戏**才能验证这些改动。
- 版面数量不再由模型决定。突破、线路服务变化、管理操作与指标变化先评定新闻价值，再由确定性规划器把扁平输出（或单一“综合”版）拆成主题版，独立评论版始终单独排在最后。素材很少或只涉及一个领域时保持单版，不为凑版注水。
- 素材足够的重大事件 = 新闻 + 独立评论。评论由**单独的一次 API 请求**生成，拥有自己的写作约定；校验篇幅、分析内容、来源与新闻重复度，针对具体问题重试一次，然后作为独立文章和版面加入。若仍失败，明确报错并且不缓存、不发布只有新闻的残缺报纸。
- 排版前新增质量检查：数字必须可追溯（含四舍五入、万/亿与百分比写法）；虚构采访、可归属引述和类官方来源判为严重错误；雷同开头、机械评价、重复报道、抄写新闻的评论会被识别。结果计入评分并保存为逐篇审计元数据，最多发起一次针对性局部修订；修订后仍有严重事实错误的稿件会被删除而不是发布。
- 缓存规则升至 v7。旧的单版缓存不再复用；历史报纸可按原版式继续阅读和重新导出。Word 按全部规划版面显式分页导出。
- 稳健性：文件系统拒绝 `File.Replace` 时，原子替换回退为安全的删除后移动；模型返回的空版面会被丢弃，不会导致整份报纸失败。
- 离线已验证：核心/缓存/财政/线路/API 兼容/版式/Word/归档测试，以及新增编辑测试（评论二次生成与失败、主题分版、事实与来源核验、去重、一次局部修订、Word 多版）。用真实存档 2027-01-12 日志（486 条操作、813 项指标、1 项市库突破）模拟，得到五个主题版加评论版并通过事实核验。**本环境未验证**：原生游戏内显示、真实付费模型调用；浏览器预览被本地沙箱阻止（`spawn EPERM`）。这些属于待玩家验收项，不能算已完成。
- 安装：退出游戏，把完整 `CityPress-0.2.0-beta.7.zip` 解压到 `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\CityPress`，然后**重启游戏**以加载新程序集。原有导出、档案、日志与密钥保留。需重新生成某一天才会使用新流程；重新导出旧报不会改写其内容。
