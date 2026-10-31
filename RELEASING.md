# Releases / 发布流程

1. Select an unused SemVer prerelease, update `VERSION`, project/UI metadata, mod log and bilingual release notes.
2. Build Release against a locally owned game installation. Run the core/export checks, UI types/build and runtime dependency audit. Record actual game validation separately from automated tests.
3. Keep GPL-3.0-only and third-party notices in both source and distribution. Exclude player data, game assemblies and internal reports.
4. Use focused signed Conventional Commits. Create a signed `v<version>` tag pointing at the exact packaged source commit.
5. Attach the full install archive and SHA-256 checksum to a GitHub **prerelease**. Never overwrite an existing public version; publish a new beta for corrections.
6. Check remote tag/commit alignment, download assets and automated checks. Do not label a beta stable or claim unperformed in-game tests.

遵循 RouteFilter 的发布约定：版本一致、中英双语更新说明、签名提交与标签、完整安装包和 SHA-256；每批修改独立提交。每个已公开 Beta 保持不变，修正使用新版本号。源码与安装包不包含个人数据、游戏文件或内部验收报告。GitHub 发布不等于已上架 Paradox Mods。
