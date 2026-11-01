# Contributing / 参与开发

Keep commits focused and use Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`, `build:`, `ci:`, `chore:`). Preserve save compatibility, keep collection separate from editorial selection, and distinguish submitted actions from observed outcomes. Include changes to collection coverage and API payloads in documentation.

Commit source, necessary resources, build configuration, maintained player guides and developer documentation. Keep private settings, API keys, player journals, generated newspapers, game assemblies, installation backups and internal test reports local. Review staged files before pushing.

Provide English and Simplified Chinese public release notes. Describe actual validation and game-test limitations accurately. See [RELEASING.md](RELEASING.md). Contributions are accepted under GPL-3.0-only.

When adding or changing player-visible text, update both `lang/en-US.json` and `lang/zh-HANS.json` with the same key set and keep `UI/src/index.tsx` free of hard-coded display text.

使用 Conventional Commits，按修改批次提交。保持存档兼容，区分操作意图与实际结果，采集与刊发筛选分开。公开说明和更新日志提供中英双语。不要提交密钥、个人报纸/日志、游戏程序集、备份或内部报告。贡献按 GPL-3.0-only 发布。新增或修改玩家可见文本时，请同步更新 `lang/en-US.json` 与 `lang/zh-HANS.json`（键集合保持一致），并避免在 `UI/src/index.tsx` 中硬编码显示文案。
