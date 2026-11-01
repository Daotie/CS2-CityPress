<p align="center"><strong>English</strong> · <a href="README.zh-CN.md">简体中文</a></p>
<p align="center"><img src="assets/branding/citypress.svg" width="120" alt="CityPress newspaper icon"></p>
<h1 align="center">CityPress</h1>
<p align="center">A daily newspaper generated from a living <strong>Cities: Skylines II</strong> city.</p>
<p align="center">
 <img src="https://img.shields.io/badge/version-0.2.0--beta.10-00a8bb" alt="0.2.0-beta.10">
 <img src="https://img.shields.io/badge/status-beta-orange" alt="Beta prerelease">
 <img src="https://img.shields.io/badge/license-GPL--3.0--only-blue" alt="GPL-3.0-only">
</p>
<p align="center"><a href="https://github.com/Daotie/CS2-CityPress/releases">Releases</a> · <a href="https://github.com/Daotie/CS2-CityPress/issues">Issues</a> · <a href="CHANGELOG.md">Changelog</a> · <a href="SECURITY.md">Privacy and security</a></p>

CityPress is a Cities: Skylines II mod that turns a city's recorded activity and simulation statistics into a readable daily newspaper. It observes what changes in the city, applies editorial rules to decide what deserves coverage, asks a configured Chat Completions-compatible service to write the copy, and assembles the result into themed editions that can be read in game or exported to Word.

The base game exposes city information through statistics panels and event notifications, but it has no newspaper, editorial selection, issue archive or exportable record. CityPress adds that layer without changing city-management rules.

**Status:** the current source version is `0.2.0-beta.10` (prerelease). Published builds are listed on the [Releases](https://github.com/Daotie/CS2-CityPress/releases) page. The mod is under active development; save data is versioned additively, but APIs, prompts and generated layouts may change between betas. See [release notes](RELEASE_NOTES.md) for the current validation scope.

## Features

### City observation

- Records stock gameplay tool submissions and management callbacks for roads, buildings, zones, terrain, policies, budgets, taxes, loans, map tiles and transport line settings.
- Reconciles observed buildings, network segments, areas, transport lines and trees, including construction, completion markers, abandonment, damage, fires and removals.
- Samples city statistics for population, economy, employment, education, health, tourism, freight and other available categories, plus treasury, loans, land, utilities, weather and pollution.
- Merges repeated actions and filters out known noise such as automatic vehicle renaming, individual boarding/alighting events, UI navigation and empty previews.
- Separates submitted intentions from observed outcomes: a tool submission is not a completed building count, and a network edge is not a whole road.

### Editorial pipeline

- Grades city material by news value (routine, notable, major, milestone, critical), covering population, treasury, households, visitors, land, transit service changes, public services and incidents.
- Uses a deterministic page planner on top of the model response: a flat article list or a single "综合" edition is split into themed editions, and rich material can continue onto an extra edition labelled（续）.
- Generates one independent commentary edition when a major event has enough factual material. The commentary is produced by a separate API task, validated for length, analysis, sourcing and overlap with news, and retried once with a concrete error if needed.
- Commentaries can carry two to four subheadings written as `## ` lines, and the opinion page can include short comments beside the long analysis. The dedicated commentary task adds up to two.
- Runs a quality gate before layout: numbers are checked against collected data (including rounding and unit forms), invented interviews or sources are rejected, repeated openings, mechanical phrasing and duplicate stories are detected, and commentary that merely reprints news is refused.
- Attempts at most one targeted revision request. If a fatal factual problem remains, the unsafe article is removed rather than published; a commentary that fails is reported instead of silently publishing a news-only issue. When a major event requires commentary, an edition containing only short notes is rejected.

### Newspaper, archive and export

- Builds themed editions such as front news, economy and finance, construction and transport, livelihoods and society, environment and observation, and an independent opinion edition.
- Provides in-game page navigation and six layout templates: banner headline, lead/sidebar, vertical headline, brief-news digest, long commentary, and long commentary with short comments.
- Saves the composition plan with the issue, so the in-game preview and Word export use the same columns and content blocks. Reopening, cache hits and re-exporting do not redraw the layout.
- Keeps every issue as JSON and Word files. Word exports all planned editions with explicit page breaks and editable text.
- Supports a custom newspaper name, a custom export directory, a per-save or global settings scope, and opening the actual export folder from the game.
- Localizes the interface and status/error messages with the game language. English and Simplified Chinese are included with built-in fallbacks; other game languages use English. Newspaper prose follows the editorial policy, and Word furniture follows the language detected in the issue.
- Keeps generation **manual by default**. Optional automatic publication generates the previous game day's issue and opens it in game.
- Caches completed issues locally, isolates caches by provider/model/settings, and shows cache or token usage when the service returns it. Unchanged metric snapshots do not trigger regeneration.

## How it works

1. **Observe.** Game systems record actions, world changes, event-journal updates and sampled statistics. Loading an existing city establishes a baseline instead of reporting the whole city as new.
2. **Journal.** Records are aggregated per game day and saved both in the game save and in local journal files.
3. **Assess.** The editorial brief grades events and facts, filters noise, and decides whether a major event warrants independent commentary.
4. **Write.** Eligible facts and the player's editorial instructions are sent to the configured Chat Completions-compatible endpoint. The service returns structured JSON with editions and articles.
5. **Check.** The quality gate verifies numbers against collected data, checks sourcing and wording, refuses copied commentary, and may request one targeted revision.
6. **Plan and lay out.** A deterministic planner decides edition themes, article distribution, page continuation and the opinion layout. It never invents filler pages.
7. **Save and export.** The issue is written to the internal archive, exported as Word and JSON, and can be reopened later.

The language model writes prose; the mod decides structure, validates facts and enforces publication rules. Generated articles are based on available observations and are not independently verified reporting.

## Compared with the base game

- **Persistent record instead of transient panels.** CityPress keeps a per-day journal and publishes issues, with archives and Word exports.
- **Editorial selection instead of raw data dumps.** Repeated actions are merged, noise is filtered, and only material with a plausible news angle is sent for writing.
- **Explicit distinction between intent and result.** Tool submissions, construction flags, completed facilities and actual transit service are recorded as different facts.
- **Configurable writing service.** The player chooses the endpoint, model and output options; the project is not tied to a single provider.
- **No simulation changes.** CityPress adds observation, editorial and export tools; it does not add buildings, policies or new economic mechanics.

## Installation

CityPress installs as a local mod. It is not distributed through Steam Workshop or Paradox Mods.

1. Download the complete `CityPress-<version>.zip` from [Releases](https://github.com/Daotie/CS2-CityPress/releases). Do not use GitHub's automatically generated source ZIP as an install package.
2. Close the game.
3. Extract the whole archive into `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\CityPress`. Keep the DLL, UI files, `Templates/` and `images/` in their relative positions.
4. Start the game again and enable CityPress in your local mod setup if it is not enabled automatically.

The package includes the built DLL, its Harmony dependency, UI assets and the Word template. Game assemblies are not included. To upgrade, replace the mod folder while the game is closed; existing journals, settings, keys and exports are kept.

To confirm which build loaded, check `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\CityPress.log` after starting the game.

## Usage

1. Open the newspaper button in the game UI.
2. In editorial settings, configure the API endpoint, model and key, or keep the DeepSeek default. A local compatible service can use HTTP and disable authentication.
3. Save the settings, then click **生成本日新闻** to generate an issue for the current game day. Generation is manual unless automatic publication is enabled.
4. Use the page directory or previous/next controls to read editions.
5. Open the archive tab to read past issues or generate an issue for a recorded day.
6. Use **导出当前报纸** to re-export the open issue without calling the API, and **打开导出文件夹** to open the actual export directory.

The interface and system messages follow the game language. English and Simplified Chinese are included; other languages fall back to English. Generated newspaper text follows the editorial policy, not the interface language; Word furniture follows the language detected in the newspaper itself. The in-game page navigation, archive rows and settings are documented in the [player guide](docs/USER_GUIDE.zh-CN.md) (Simplified Chinese).

## Configuration

Settings can be stored globally or per save. The main options are:

| Setting | Purpose | Default |
| --- | --- | --- |
| Automatic publication | Generate the previous game day's issue after rollover and open it in game | Off |
| Newspaper name | Custom masthead; follows the city name when empty | City name + 日报 |
| Export directory | Absolute folder for Word/JSON exports; empty uses Documents/CityPress | Documents/CityPress |
| API endpoint | Base URL or full `chat/completions` URL of a compatible service | `https://api.deepseek.com/chat/completions` |
| Model | Model name exposed by the service | `deepseek-chat` |
| API key | Bearer key for the endpoint; local services can disable authentication | Empty |
| JSON mode | Send `response_format: {"type":"json_object"}` | On |
| Maximum output tokens | Output limit sent to the service | 8192 |
| Timeout | Maximum wait for one request | 180 seconds |
| Token parameter | `max_tokens` or `max_completion_tokens` | `max_tokens` |
| Layout mode | Content-based template selection or suitable-template variation | Auto |
| Long commentary | Enable the dedicated commentary task for major events | On |
| Editorial policy | Additional instructions for the editor | Built-in policy |

Compatible endpoints must accept the OpenAI-style Chat Completions request and JSON response used by the mod. Base URLs are normalized to a `/chat/completions` path; local endpoints can disable authentication. The JSON-mode switch can be turned off for services that do not support `response_format`, but the model must still return the requested JSON structure.

API keys are stored per normalized endpoint, encrypted with Windows DPAPI for the current user, and kept outside savegames. Requests do not follow redirects. Generated issues only send the data needed for editing: city/newspaper name, game date, eligible action summaries, selected city statistics and editorial instructions. Provider billing and data handling apply.

## Compatibility

- **Platform:** Windows.
- **Game version:** the current build is compiled against assemblies from Cities: Skylines II `1.6.3f1`; this is the only version used for local checks. Other game versions have not been verified.
- **Runtime:** packaged as a `net48` game mod; Harmony 2.2.2 is bundled. The UI is built as a game UI module; React, React DOM and the UI runtime are supplied by the game.
- **API protocol:** Chat Completions-compatible HTTP services. This is protocol compatibility, not native support for every provider-specific API.
- **Save data:** the save envelope remains version 1 with additive fields. Existing installations and earlier issues remain readable.
- **Third-party mods:** independent mod behavior, transient intermediate states and unavailable internal data can be missed. The compatibility panel reports active management bindings and collection errors.
- **Rendering:** in-game Cohtml and Word can differ in glyph metrics and physical pagination. Word content is editable text, not a pixel-identical copy of the in-game view.

## Performance

CityPress is designed to keep observation and generation off the critical simulation path:

- Collection is split into bounded per-frame slices instead of scanning the whole city in one tick.
- Snapshot reconciliation and statistics sampling run on scheduled intervals after the previous cycle completes, using the game's own update cadence.
- Journal writes, archive reads, Word export and issue generation run on background workers; the UI state and clock are updated separately.
- Completed issues are cached locally, and unchanged metric snapshots do not trigger another generation request.

No frame-rate benchmark or fixed performance guarantee is claimed. Actual impact depends on city size, entity count, other mods, disk speed and the configured API service. A request may take up to the configured timeout; generation is asynchronous, but network latency still affects how quickly an issue appears.

## Troubleshooting

**No issue is generated.** The default is manual. Check that city time is ready, the day has eligible material, and no generation is already running. A day with no eligible facts produces a short empty issue without an API request.

**The API request fails.** Check the endpoint, model name, authentication setting, output-token parameter and timeout. Some services do not support JSON mode; disable it if the service rejects `response_format`. Truncated responses are not cached.

**The commentary is missing.** Independent commentary requires a major event with enough material and the long-commentary option enabled. A failed commentary is reported as an error and is not cached; a commentary edition containing only short notes is rejected.

**An old issue still uses the old layout.** Archived issues keep their saved layout. Re-generate the day to apply current layout and editorial rules; re-exporting an old issue does not rewrite it.

**A number or claim looks different from the in-game panel.** Monthly budget income, budget balance and treasury cash are different measures and are labelled separately. Generated articles use the facts captured for that game day; they are not a live panel and do not reconstruct unobserved history.

**Where are my files?** Settings, journals, cache and internal issue JSON are under `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\CityPress`. Word/JSON exports default to `Documents/CityPress/<newspaper>/存档-<save ID>/`.

**Can I use a local model?** Yes, if it exposes a Chat Completions-compatible endpoint. Disable authentication if the local service does not require it. The project does not bundle or run a model itself.

## AI-assisted development

This project uses AI-assisted development tools to support code analysis, debugging, documentation drafting, and development workflow tasks. AI-generated suggestions are reviewed, edited and validated as part of the development process; they are not accepted as finished work without checking.

The project direction, architecture, feature decisions, test strategy, release decisions and ongoing maintenance remain the responsibility of the project author. AI tools are not project authors, maintainers or credited contributors.

## Feedback and contributing

- Reproducible bugs and feature requests: [GitHub Issues](https://github.com/Daotie/CS2-CityPress/issues). Include the CityPress version, game version, reproduction steps and relevant errors with secrets removed.
- Security or privacy reports: follow [SECURITY.md](SECURITY.md) and avoid posting keys, `key.dpapi`, `credentials/` or private journals publicly.
- Contributions: see [CONTRIBUTING.md](CONTRIBUTING.md). Contributions are accepted under GPL-3.0-only.
- Release process: see [RELEASING.md](RELEASING.md).

## License

CityPress source code, newspaper template and branding are licensed under [GPL-3.0-only](LICENSE), copyright (c) 2026 Daotie and contributors. Bundled and referenced dependencies retain their own licenses; see [third-party notices](THIRD_PARTY_NOTICES.md).

The project is not affiliated with Colossal Order, Paradox Interactive, Unity, Cohtml or any API provider. No Paradox Mods listing is claimed.

## Author and maintenance

CityPress is independently developed and maintained by [Daotie](https://github.com/Daotie). The author also maintains a separate Cities: Skylines II mod, [RouteFilter](https://github.com/Daotie/CS2-RouteFilter); the two projects are independent and CityPress does not depend on RouteFilter.

## Development

A full build requires Windows, .NET SDK 10, Node.js 20+ and a local Cities: Skylines II installation that provides the game assemblies. Game assemblies and generated release archives are not tracked in this repository.

```powershell
./build.ps1 -ManagedPath '<Steam library>/steamapps/common/Cities Skylines II/Cities2_Data/Managed'
```

Alternatively set `CSII_MANAGED_PATH`. The script builds the mod, runs the test suite, executes the callback harness against game assemblies, builds and type-checks the UI, audits runtime dependencies, and packages the version in `VERSION`.

Checks that do not need game files:

```powershell
dotnet run --project Tests/CityPress.Tests.csproj -c Release -- --core-only
npm ci --prefix UI
npm run typecheck --prefix UI
npm run build --prefix UI
npm audit --prefix UI --omit=dev
```

The public GitHub Actions workflow runs the offline core checks and the UI checks; it does not run the game. Build details are in [BUILD.md](docs/BUILD.md).

## Documentation

- [Player guide (Simplified Chinese)](docs/USER_GUIDE.zh-CN.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Build and verification](docs/BUILD.md)
- [Changelog](CHANGELOG.md) · [Release notes](RELEASE_NOTES.md)
