# CityPress architecture / 架构

- `Core/Models.cs`: save-compatible daily records, duplicate aggregation, metric extrema and storage.
- `Systems/ActionHooks.cs` and `GameplayBindings.cs`: tool submissions and stock gameplay management callbacks. Recorder failures must not break the original game action; callback exceptions retain their original type.
- `Systems/CityObserver.cs`: observed world changes, event journals and five-game-minute metric snapshots. Loading establishes a baseline instead of reporting the existing city as newly built.
- `Core/EditorialInput.cs`, `CapturePolicy.cs`: exclude known noise by origin, while retaining real construction involving transport boarding areas.
- `Core/NewsService.cs`: professional editorial contract, configurable policy, factual payload, cache and DeepSeek request. Unchanged resampling does not invalidate a factual cache. No API call for noise-only days.
- `Systems/NewsSystem.cs`: UI state, per-save/global preferences, time rollover, cancellation and archive-before-publication.
- `Resources/paper-layout.json`, `Core/PaperLayout.cs`, `UI/src/paper-layout.ts`, `Templates/newspaper.docx`: shared newspaper layout and editable Word export.

The save envelope remains version 1 with additive metric fields. Invalid initialization dates are discarded; previous auto-renaming noise is filtered when generating new issues. Actual observed coverage is narrower than every internal game/mod event. Read [the collection guide](USER_GUIDE.zh-CN.md) before interpreting action counts as engineering quantities.

记录、观测、筛选、生成与排版分别维护。工具提交次数不代表建成数量；原生路网边不代表整条道路；事件累计数据不可相加。存档版本保持 1，新字段向后兼容。新增覆盖应同时补充文档、缓存规则和相应验证。
