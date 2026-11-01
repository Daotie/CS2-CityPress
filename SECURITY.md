# Security and privacy / 安全与隐私

Do not post API keys, `key.dpapi`, `credentials/`, environment files, private city journals or unredacted personal content in issues. For a vulnerability, use the repository's private vulnerability reporting when enabled, rather than sharing exploitation details publicly.

CityPress stores its key using Windows user-account encryption. It does not include the key in savegames or UI state. Manual generation, or explicitly enabled automatic generation, sends the city/newspaper name, game date, eligible activity summaries, selected city statistics and editorial instructions to the configured API endpoint. Provider billing and data handling apply. Local journals and newspapers can contain custom names and should be reviewed before sharing.

Default generation is manual. Collection alone does not request a generation API. See [the player guide](docs/USER_GUIDE.zh-CN.md) for storage paths.

请勿公开密钥、`key.dpapi`、`credentials/`、环境文件、私人城市日志或未脱敏报纸。生成时会向配置的 API 服务发送城市名称、日期、有效操作摘要、筛选后的指标和编辑方针；仅采集不会请求 API，默认手动生成。分享日志或报纸前请自行检查自定义名称等内容。


Keys are bound to normalized request endpoints under credentials/<hash>.dpapi; the same endpoint shares its local credential across saves. Legacy key.dpapi and DEEPSEEK_API_KEY apply only to official HTTPS DeepSeek paths. The entered URL cannot contain credentials, a query string or a fragment; use the password field. Disabling authentication sends no Authorization header. HTTP is available for local/compatible services; HTTPS protects traffic when using remote services. Endpoint preferences are stored in saves, while key material is not. No automatic redirects or paid retries are performed.

密钥按归一化请求地址加密保存在 credentials/<哈希>.dpapi；同一地址在本机各存档共享凭据。旧 key.dpapi 与 DEEPSEEK_API_KEY 只用于官方 HTTPS DeepSeek 路径。地址不接受账号、查询参数或片段，密钥用密码框填写；关闭鉴权不发送 Authorization。HTTP 支持本地/兼容服务，远程 HTTPS 可保护传输。地址设置会随存档保存，密钥不会；不自动重定向或付费重试。
