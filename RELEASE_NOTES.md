# CityPress 0.2.0-beta.1

## English

First public beta for daily city newspapers: optional DeepSeek editing, in-game paper layout, Word/JSON archives, per-save/global mastheads, broad gameplay and city observation, and noise filtering. Generation is manual by default. The UI and default newspaper furniture currently use Simplified Chinese.

Download `CityPress-0.2.0-beta.1.zip` and its `.sha256` file. Extract the entire package into the local `Mods/CityPress` folder while the game is closed, then restart. Keep `Templates/` and `images/` beside the DLLs. GitHub source archives are for developers.

Verified locally: C# Release build, UI types/production build, core aggregation/cache/Word checks and callback/browser harnesses. Expanded collection has not completed a full in-game acceptance pass. Third-party internal actions and brief intermediate states may be missed. DeepSeek generation sends eligible facts to the provider; review `SECURITY.md`.

Save envelope remains version 1, with additive metric fields. Existing archives are retained; collection begins with observed gameplay and cannot reconstruct past days. See `CHANGELOG.md` and the bilingual READMEs for details.

## 简体中文

首个公开 Beta，提供 DeepSeek 新闻编辑、游戏内报纸、Word/JSON 归档、自定义报头、单存档/全局设置、广泛城市采集与降噪。默认手动生成；当前界面与默认版签为简体中文。

下载 `CityPress-0.2.0-beta.1.zip` 和 `.sha256` 校验文件，退出游戏后完整解压到本地 `Mods/CityPress`，重启生效。保留 `Templates/`、`images/` 与 DLL 的相对位置，源码包不等于安装包。

已通过本地 C# Release、UI 类型/构建、汇总/缓存/Word、回调与浏览器模拟检查。扩大采集尚未完成完整游戏内验收，其他模组内部动作和短暂状态可能缺失。生成时会向 DeepSeek 发送有效素材，隐私说明见 `SECURITY.md`。

存档外层版本保持 1，指标字段为追加扩展，已有报纸保留。采集从实际观测开始，不能补回安装前历史。
