# Releases / 发布流程

1. Select an unused SemVer version (stable `x.y.z` or prerelease `x.y.z-beta.n`), update `VERSION`, project/UI metadata, mod log and bilingual release notes. Keep historical changelog entries unchanged.
2. Build Release against a locally owned game installation. Run the core/export checks, UI types/build and runtime dependency audit. Record actual game validation separately from automated tests.
3. Keep GPL-3.0-only and third-party notices in both source and distribution. Exclude player data, game assemblies and internal reports.
4. Use focused signed Conventional Commits. Create a signed `v<version>` tag pointing at the exact packaged source commit.
5. Attach the full install archive and SHA-256 checksum to the matching GitHub release. Mark `-beta.n` builds as **prereleases**; stable `x.y.z` builds use a regular release. Never overwrite an existing public version; use a new patch or beta version for corrections.
6. Check remote tag/commit alignment, download assets and automated checks. Match release status to the selected version and never claim unperformed in-game tests.

发布流程遵循以下约定：版本一致、中英双语更新说明、签名提交与标签、完整安装包和 SHA-256；每批修改独立提交。正式版使用普通 Release，带 `-beta.n` 的版本标记为预发行。每个已公开版本保持不变，修正使用新的补丁版或 Beta 版本号。源码与安装包不包含个人数据、游戏文件或内部验收报告。GitHub 发布不等于已上架 Paradox Mods。

## Paradox Mods

CityPress uses the existing public listing **166126**. Keep English before Simplified Chinese, list game compatibility as `1.6.*`, and retain the thank-you and verified author-mod links at the end. Publishing metadata is maintained in `Properties/PublishConfiguration.xml`; screenshots reuse the approved assets under `assets/showcase/1.0.0/`.

After packaging committed source, run `./tools/publish.ps1` for a new version or `./tools/publish.ps1 -Command Update` for metadata-only changes. The wrapper invokes the official installed game publisher, reuses its existing login and never accepts or stores account passwords. Do not create another listing.

CityPress 更新既有条目 **166126**。简介英文在前、中文在后，支持游戏版本写为 `1.6.*`，末尾保留感谢和作者模组互链。先打包已提交的源码，再使用上述命令发布新版本或仅更新元数据；使用官方发布器已有登录，不重复创建条目。
