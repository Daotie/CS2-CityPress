# CityPress 0.2.0-beta.8

## English

Commentary-edition release: a long analysis with subheadings, short comments on the same page, and page planning that adds editions when the material needs them.

- **Commentary edition.** One long commentary occupies the whole or most of the opinion page. It can be accompanied by up to two short comments. The long piece may carry 2–4 subheadings, written as ordinary body lines starting with `## `; the game preview shows them as distinct heading blocks and Word exports them as a smaller heading style that stays with the following paragraph. Headings are never duplicated in the body text.
- **Separate standards.** The dedicated commentary request may return `shortComments`. The long piece is checked for length, analysis, paragraphing and subheadings; short comments only need a clear viewpoint and are checked separately, so a 150–350 character note is no longer rejected as an "insufficient long commentary". Invalid short comments are dropped individually; a failed long commentary still retries once and otherwise reports a clear error without caching. A commentary edition with no lead long piece is rejected when the event requires commentary.
- **Pages follow the material.** An edition is no longer limited to one topic. A topic with enough copy continues onto extra editions labelled（续）/（续二）…, pages are split by an editorial capacity (about 2400 characters), the paper can carry up to ten news editions, and thin pages are merged only when the limit is exceeded. Rich days get more space instead of being crammed into one page or losing copy.
- **New layout.** `opinion-mixed` gives the long commentary two balanced main columns (with its subheadings) and a narrow sidebar for short comments. Existing single-commentary `opinion`, the other four templates and archived issues keep working unchanged. Cache policy is now v8; old cached editions are not reused, while archives stay readable and re-exportable in their original layout.
- **Verified offline:** 155 checks, including an end-to-end run where the commentary task returns a long piece with three subheadings plus two short comments and the result passes quality, gets the mixed layout, keeps every body paragraph exactly once, and exports to Word with headings and short-comment titles. Also covered: role-based commentary quality rules, same-topic continuation editions, and Word multi-page export. **Not verified here:** browser preview (the local sandbox denies browser launch) and native in-game display. The game must be restarted to load this build; in-game appearance still needs player acceptance.
- **Install:** exit the game, extract the full `CityPress-0.2.0-beta.8.zip` into `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\CityPress`, then start the game again. Existing journals, archives, exports and keys are preserved. Re-generate a day to use the new opinion layout; re-exporting an old issue does not rewrite it.

## 简体中文

评论专版版本：长评论可带小标题、同版可放短评，版面按素材自动加版。

- **评论专版。** 一篇长评论占据评论版全部或大部分篇幅，可与至多两篇短评同版。长评论可以有 2 至 4 个小标题：以正文中“## ”开头的单独一行书写，游戏预览中显示为独立小标题，Word 中导出为较小字号的标题样式并保持与后文同页；小标题不会在正文中重复出现。
- **独立标准。** 独立评论请求可返回 `shortComments`。长评论检查篇幅、论证、分段与小标题；短评只需观点明确，使用单独标准，不再被当作“论证不足的长评论”误判。不合格的短评单独丢弃；长评论失败仍会重试一次，否则明确报错、不缓存。重大事件需要评论而评论版没有长评论时会被拒绝，只有短评的版面不能冒充深度分析。
- **版面跟着素材走。** 一版不再只放一类新闻。同一主题素材足够时连续加版（版名“（续）”“（续二）”等），按约 2400 字的编辑容量拆版，新闻版最多十个；只有超过上限才合并薄版。素材丰富的日子获得更多版面，而不是硬塞进一版或丢失稿件。
- **新版式。** `opinion-mixed` 用两栏均衡承载长评论与小标题，窄侧栏放短评。原有单篇评论版、其余四种版式和历史旧刊保持兼容。缓存规则升至 v8，旧缓存不重用；历史报纸仍可按原版式阅读和重新导出。
- **离线验证：** 共 155 项检查，包括“独立评论任务返回含三个小标题的长评论加两篇短评”的端到端生成：通过质量检查、使用混合版式、每段正文只出现一次、Word 导出保留小标题与短评标题；另外覆盖按角色的评论质量规则、同主题连续加版与多版 Word 导出。**本环境未验证：**浏览器预览（本地沙箱禁止启动浏览器进程）与原生游戏内显示。必须先重启游戏加载本版本，游戏内效果仍需玩家验收。
- **安装：** 退出游戏，把完整 `CityPress-0.2.0-beta.8.zip` 解压到 `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\CityPress`，再启动游戏。原有日志、档案、导出和密钥保留。需重新生成某一天才会使用新的评论版式；重新导出旧报不会改写其内容。
