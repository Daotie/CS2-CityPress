# CityPress 1.0.0

CityPress 城市日报首个正式版：把《都市：天际线 II》中实际记录到的城市变化，整理成可在游戏内阅读、归档并导出为 Word 的报纸。

- 采集城市操作与统计，在本地归并重复素材、过滤噪声，再交由玩家配置的 Chat Completions 兼容服务写作。
- 按内容组织要闻、专题与评论版，支持审校补稿、专题续版、往期阅读及自定义报纸名称。
- 默认手动生成，可选每日自动发行；界面支持简体中文与英文，报纸正文语言由编辑方针决定。
- 保存失败时保留游戏内成稿；Word 文件被占用时另存副本，导出目录不可用时回退至本地归档，可手动重试导出而不重新调用 API。
- 版本元数据统一为 1.0.0，沿用现有存档与报纸格式；Beta 历史更新见 [CHANGELOG.md](CHANGELOG.md)。

安装与升级：从 [GitHub Releases](https://github.com/Daotie/CS2-CityPress/releases) 下载完整安装包，关闭游戏后替换整个 CityPress 模组目录，再重新启动。GitHub 发布不代表已上架 Paradox Mods。

验证结果：C# Release 编译无警告、无错误；232 项回归检查、5 项实际游戏回调检查、界面类型检查及构建通过，运行依赖审计未发现漏洞。界面构建保留两项资源体积提示。以上属于离线验证；原生游戏内显示、真实存档运行和真实服务生成效果仍需玩家验收，本次未调用玩家付费 API。

## English

The first stable CityPress release turns recorded Cities: Skylines II activity into themed newspapers for in-game reading, local archives and editable Word export.

- Observe city activity and statistics, group duplicate material and filter noise locally before requesting copy from the player's configured Chat Completions-compatible service.
- Organize front news, themed continuation editions and commentary, with editorial review and repair, custom newspaper names and historical issue reading.
- Generate manually by default, with optional daily publication. The UI supports English and Simplified Chinese; editorial policy controls the language of the newspaper itself.
- Keep completed copy visible when saving fails. Locked Word files receive a new copy; unavailable export folders fall back to the local archive. Retry export without another API request.
- Align all current version metadata at 1.0.0 while preserving existing save and issue formats. Historical Beta changes remain in [CHANGELOG.md](CHANGELOG.md).

Download the full install archive from [GitHub Releases](https://github.com/Daotie/CS2-CityPress/releases), close the game, replace the complete mod folder and restart. A GitHub release does not imply a Paradox Mods listing.

Validation: C# Release builds with zero warnings and errors; 232 regression checks, five actual game-callback checks, UI type checking and UI build pass. The runtime dependency audit reports zero vulnerabilities. Two UI asset-size warnings remain. These are offline checks; native rendering, live-save behavior and real provider output still require in-game acceptance. No paid player API was used for this release work.

## Thank you / 感谢

感谢你使用 CityPress，也感谢每一位提出建议、反馈问题和分享报纸的玩家。如果你喜欢这个 Mod，也可以看看我的其他 Mod：[RouteFilter（Paradox Mods）](https://mods.paradoxplaza.com/mods/155839/Windows)，为《都市：天际线 II》的道路与轨道提供逐车辆资产通行限制；[源码与使用说明](https://github.com/Daotie/CS2-RouteFilter)见 GitHub。

Thank you for using CityPress and sharing your feedback, bug reports and newspapers. If you enjoy this mod, you can also check out my other mods, including [RouteFilter on Paradox Mods](https://mods.paradoxplaza.com/mods/155839/Windows), for individual vehicle-asset restrictions on roads and railways in Cities: Skylines II. Its [source and documentation](https://github.com/Daotie/CS2-RouteFilter) are on GitHub.
