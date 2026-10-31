# CityPress 0.2.0-beta.3

## English

- Fixed Chinese newspaper text displaying as boxes: use the game's registered font throughout the newspaper instead of Windows-only font families.
- Fixed fragmented archive row text and the wrapped brand subtitle; made the panel background opaque.
- Removed empty historical initialization dates while keeping real early-year activity, metric-only history and the current day. Issues mistakenly generated for these empty dates are hidden without deleting local archives; the suppression survives save/reload.
- Added source/UI/release version consistency checks.

Verified locally: Release compilation, core/cache/export and callback checks, UI type/build checks, runtime dependency audit, and browser regression checks for game-font inheritance, archive grouping and three UI scales. The updated native in-game display still needs confirmation after restart; automated/harness checks do not constitute a full in-game acceptance pass.

Download the complete `CityPress-0.2.0-beta.3.zip` and `.sha256`. Close the game, extract all files to local `Mods/CityPress`, retaining `images/` and `Templates/`, and restart. Journals, settings, keys and existing Word/JSON newspapers remain in `ModsData/CityPress`. The save envelope remains version 1 with additive fields. Generation is still manual by default.

## 简体中文

- 修复报纸中文变方框：整张报纸使用游戏已注册的字体，避免依赖 Windows 字体名。
- 修复档案行文字被拆散排布、标题副行换行，并使面板底板不透明。
- 清理无操作、无指标的过去初始化日期，保留真实早期操作、仅指标历史和当前日期。空日期误生成的报纸只隐藏入口，不删除本地归档，标记随存档保留。
- 新增源码、UI 和发行说明的版本一致性检查。

已完成本地 Release 编译、核心/缓存/导出和回调检查、UI 类型/构建、运行依赖审计，以及字体继承、档案分组和三种缩放的浏览器回归检查。更新后的原生游戏显示仍需重启确认；自动检查不等于完整游戏内验收。

下载完整 `CityPress-0.2.0-beta.3.zip` 与 `.sha256`。退出游戏后完整解压至本地 `Mods/CityPress`，保留 `images/`、`Templates/`，重启生效。`ModsData/CityPress` 下的日志、设置、密钥和已有 Word/JSON 报纸保留，存档外层版本仍为 1。默认仍为手动生成。
