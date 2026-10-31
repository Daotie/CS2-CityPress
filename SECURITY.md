# Security and privacy / 安全与隐私

Do not post API keys, `key.dpapi`, environment files, private city journals or unredacted personal content in issues. For a vulnerability, use the repository's private vulnerability reporting when enabled, rather than sharing exploitation details publicly.

CityPress stores its key using Windows user-account encryption. It does not include the key in savegames or UI state. Manual generation, or explicitly enabled automatic generation, sends the city/newspaper name, game date, eligible activity summaries, selected city statistics and editorial instructions to DeepSeek. Provider billing and data handling apply. Local journals and newspapers can contain custom names and should be reviewed before sharing.

Default generation is manual. Collection alone does not request DeepSeek. See [the player guide](docs/USER_GUIDE.zh-CN.md) for storage paths.

请勿公开密钥、`key.dpapi`、环境文件、私人城市日志或未脱敏报纸。生成时会向 DeepSeek 发送城市名称、日期、有效操作摘要、筛选后的指标和编辑方针；仅采集不会请求 API，默认手动生成。分享日志或报纸前请自行检查自定义名称等内容。
