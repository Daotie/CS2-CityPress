# CityPress 0.2.0-beta.6

## English

Configurable compatible APIs, independent long commentary and five newspaper layout templates.

- Configure the endpoint, model, bearer authentication, JSON mode, output token parameter/limit and timeout. Base URLs and full chat/completions URLs are accepted. DeepSeek remains the initial default; compatible local endpoints can disable authentication. This is Chat Completions compatibility, not native support for every provider protocol.
- Credentials are encrypted per normalized request URL, outside savegames. Switching endpoints does not reuse the previous service key; old DeepSeek key/environment fallback stays restricted to official HTTPS DeepSeek. Requests do not follow redirects. Compatible services may return different usage fields; missing usage is shown as unavailable.
- Optional independent long commentary asks for a sustained, fact-based argument in natural paragraphs, usually 700–1400 Chinese characters when supported by the facts. It can occupy its own edition, with a layout designed for continuous reading. Short material should not be padded.
- Five templates: banner, lead/sidebar, vertical headline, brief-news digest and long commentary. Select content-based matching or suitable-template variation. A saved composition plan drives UI and Word; reopening, cache hits and re-exporting preserve the layout. Earlier issues use their original layout.

Verified: Release/core/export, custom API/auth/cache/usage/error cases, callback harness, UI types/build/audit, browser settings and five templates at three scales, and five Word pages rendered and inspected. Tests used offline fixtures, not paid provider calls. Native game/provider acceptance remains pending; Word and Cohtml can differ in exact glyph shapes and physical pagination.

Exit the game, install the complete CityPress-0.2.0-beta.6.zip into local Mods/CityPress, and restart. Existing exports and keys are preserved. Re-generate to use new editorial/layout options; re-exporting old text does not rewrite it. Review the player guide for compatible endpoint and credential behavior.

## 简体中文

新增自定义兼容 API、独立长评论与五种报纸布局。

- 可设置 API 地址、模型、Bearer 鉴权、JSON 模式、输出参数/上限及等待时间，支持基础地址和完整 chat/completions 地址。初始默认 DeepSeek，本地兼容接口可关闭鉴权；支持范围为 Chat Completions 兼容协议。
- 每个归一化请求地址分别加密保存凭据，不写入存档。切换地址不带走旧服务密钥，旧 DeepSeek 密钥和环境变量仅用于其官方 HTTPS 地址。请求不自动跳转，服务未返回用量时显示缺失。
- 可开启独立长评论版，用自然段展开有事实依据的论证，素材足够时通常700至1400汉字，不注水。长评论采用连续分栏，消息和观点区分。
- 五种模板：通栏、侧栏主稿、纵向头条、综合短讯、长评论专版。可按内容选版或从合适模板中变换选择，成稿固定保存布局；打开、命中缓存和重新导出都不重选。游戏与 Word 使用同一内容块/栏宽计划，旧刊保留原版式。

已验证 Release/核心/导出、自定义 API/鉴权/缓存/用量/错误、回调检查、UI 类型/构建/审计、三种缩放下的设置与五模板，以及五版 Word 渲染。使用离线样本，未执行付费服务请求；原生游戏/真实服务仍待验收。Word 与 Cohtml 的字形和物理分页可能有差异。

退出游戏后将完整 CityPress-0.2.0-beta.6.zip 安装到本地 Mods/CityPress，再重启。原有报纸与密钥保留。新评论/版式需重新生成，重新导出旧稿不改写内容；接口兼容和密钥行为见使用指南。
