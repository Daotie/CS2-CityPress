# CityPress 0.2.0-beta.9

## English

Localization release: the mod now follows the game language for its interface, messages and exported newspaper furniture.

- **Interface and messages.** All in-game labels, placeholders, archive text, settings, operation status, API/quality errors, theme names and collection summaries are available in English and Simplified Chinese. The interface follows the game language; other game languages fall back to English.
- **Built-in fallbacks.** `lang/en-US.json` and `lang/zh-HANS.json` share the same key set and are packaged with the mod. The UI also carries a fallback table, so a missing or not-yet-loaded language file never shows raw keys or blank controls. The game's own localization dictionary is used when available.
- **Backend status/error keys.** User-facing failures use stable `CityPress.Error.*` keys with arguments, and status uses `CityPress.Status.*`. The UI renders the translated text while preserving the previous Chinese fallback for logs and tests.
- **Word furniture.** The exporter detects the language of the issue and applies matching furniture: English kicker, masthead labels, page numbering, month names and weekdays for English issues; existing Chinese issues keep their current layout.
- **Generated newspaper prose.** The interface language does not auto-translate article content. Newspaper text follows the editorial policy and the configured model; Word furniture follows the language detected in the issue. The player guide explains this behavior.
- **Packaging.** The release includes `mod.json` and `lang/`; restart the game after installing so the new metadata and language files are loaded.
- **Process wording is repaired, not fatal.** Sentences that expose the production process or use template disclaimers are flagged, sent back to the model once as a targeted revision, and any phrase that survives is removed deterministically before layout. One boilerplate sentence no longer discards an otherwise usable issue or forces a full regeneration.
- **Settings panel fixes.** Scrolling now works while the pointer is over the settings form, and inputs use opaque clipped surfaces with explicit stacking so selecting text no longer shows overlapping field contents.
- **Verified offline:** 214 checks including language-file key parity, no hard-coded Chinese in the UI entry file, all UI/backend keys present, English Word furniture/date, Chinese fallbacks, one-model-revision process-wording repair and deterministic cleanup fallback. **Not verified here:** native in-game language switching, Cohtml layout with longer English strings, real browser/CI rendering, and native game acceptance; treat those as pending player validation rather than completed.

## 简体中文

本地化版本：模组界面、提示消息和导出报纸版签现在会跟随游戏语言。

- **界面与消息。** 游戏内标签、占位文字、档案文案、设置项、操作状态、API/质量错误、主题名称和采集摘要均提供英文与简体中文。界面跟随游戏显示语言，其他游戏语言回退为英文。
- **内置回退。** `lang/en-US.json` 与 `lang/zh-HANS.json` 使用同一套键并随包发布；界面还内置回退表，语言文件缺失或尚未加载时不会出现原始键名或空白控件。游戏本地化字典可用时优先使用。
- **后端状态与错误词条。** 用户可见错误使用稳定的 `CityPress.Error.*` 与参数，状态使用 `CityPress.Status.*`。界面负责翻译，同时保留原有中文回退供日志和测试使用。
- **Word 版签。** 导出时检测报纸语言并使用对应版签：英文报纸使用英文栏目、报头标签、版号、月份和星期；已有中文报纸保持现有版式。
- **生成的报纸正文。** 界面语言不会自动翻译文章内容。正文语言由编辑方针和所配置的模型决定，Word 版签跟随报纸检测到的语言；使用指南中已说明这一点。
- **制作口径可修复，不再整期作废。** 暴露制作过程或使用模板免责措辞的句子会被质量检查标出，先交给模型做一次针对性改写；仍有残留时在排版前按句清理。一句套话不再导致整期丢弃或强制重新生成。
- **设置面板修复。** 鼠标位于设置表单时也能正常滚动；输入框使用不透明裁剪层与明确层级，选择文本时不再叠显其他输入框内容。
- **打包。** 发行包包含 `mod.json` 与 `lang/`；安装后请重启游戏，以加载新的元数据和语言文件。
- **离线已验证：** 共 214 项检查，包括中英文语言文件键一致、界面入口不再硬编码中文、所有界面/后端词条存在、英文 Word 版签与日期、中文回退、制作口径的一次模型修订与确定性清理回退。**本环境未验证：** 原生游戏内语言切换、较长英文文案在 Cohtml 中的布局、真实浏览器/CI 渲染和原生游戏验收；这些属于待玩家验收项。
