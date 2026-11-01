# CityPress architecture / 架构

- `Core/Models.cs`: save-compatible daily records, duplicate aggregation, metric extrema and storage.
- `Systems/ActionHooks.cs` and `GameplayBindings.cs`: tool submissions and stock gameplay management callbacks. Recorder failures must not break the original game action; callback exceptions retain their original type.
- `Systems/CityObserver.cs`: observed world changes, event journals and five-game-minute metric snapshots. Loading establishes a baseline instead of reporting the existing city as newly built.
- `Core/EditorialInput.cs`, `CapturePolicy.cs`: exclude known noise by origin, while retaining real construction involving transport boarding areas.
- `Core/NewsService.cs`: professional editorial contract, configurable policy, factual payload, cache and DeepSeek request. Unchanged resampling does not invalidate a factual cache. No API call for noise-only days.
- `Systems/NewsSystem.cs`: UI state, per-save/global preferences, time rollover, cancellation and archive-before-publication.
- `Resources/paper-layout.json`, `Core/PaperLayout.cs`, `UI/src/paper-layout.ts`, `Templates/newspaper.docx`: shared newspaper layout and editable Word export.

The save envelope remains version 1 with additive metric and `OmittedEmptyDays` fields. Empty past initialization days are omitted only when they have neither actions nor sampled metrics. Their issue-list suppression survives reloading without deleting archived files; a genuine current day with that date remains valid. Previous auto-renaming noise is filtered when generating new issues. Actual observed coverage is narrower than every internal game/mod event. Read [the collection guide](USER_GUIDE.zh-CN.md) before interpreting action counts as engineering quantities.

记录、观测、筛选、生成与排版分别维护。工具提交次数不代表建成数量；原生路网边不代表整条道路；事件累计数据不可相加。存档版本保持 1，新字段向后兼容。新增覆盖应同时补充文档、缓存规则和相应验证。

- `Core/Milestones.cs`: trusted scalar high-water marks, per-day merged thresholds, persisted in the game save. Baselines do not generate historical claims.
- `Core/ExportStore.cs`: configurable Documents exports with a per-save location index; internal issue JSON remains stable for the UI.
- `Core/JournalSnapshot.cs` and `JournalStore.cs`: immutable worker snapshots, per-day journal files and a lightweight version-2 disk index. The game-save envelope remains version 1.
- Observer reconciliation is staged with row/time targets; native query allocation/job synchronization is still synchronous. Clock and full UI state have separate bindings.

- `Core/TransitFacts.cs`: line-level service evidence, baseline-safe transitions and same-day merged changes. CityObserver stages an independent one-game-minute line cycle, using actual vehicle states rather than assignment counts.
- `Core/FinanceFacts.cs`: signed monthly balance uses income minus the positive native expense-array sum. Old erroneous monthlyBalance samples/highs are excluded from v5 editorial facts; monthlyNetIncome has a clean baseline.
- `Core/EditionRules.cs`: normalize themed pages and legacy flat articles, bound response size, distinguish genres and reject process caveats. `Pages` is authoritative when present; `Articles` is flattened for backwards-compatible readers. Theme order is shared by UI and Word.
