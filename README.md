<p align="center"><strong>English</strong> · <a href="README.zh-CN.md">简体中文</a></p>
<p align="center"><img src="assets/branding/citypress.svg" width="120" alt="CityPress newspaper icon"></p>
<h1 align="center">CityPress</h1>
<p align="center">Your city's daily newspaper for <strong>Cities: Skylines II</strong>.</p>
<p align="center">
 <img src="https://img.shields.io/badge/version-0.2.0--beta.1-00a8bb" alt="0.2.0-beta.1">
 <img src="https://img.shields.io/badge/status-beta-orange" alt="Beta">
 <img src="https://img.shields.io/badge/license-GPL--3.0--only-blue" alt="GPL-3.0-only">
</p>
<p align="center"><a href="https://github.com/Daotie/CS2-CityPress/releases">Downloads</a> · <a href="https://github.com/Daotie/CS2-CityPress/issues">Issues</a> · <a href="CHANGELOG.md">Changelog</a> · <a href="SECURITY.md">Privacy</a></p>

**0.2.0-beta.1:** First public beta: daily observation, editorial generation, in-game newspaper reading and editable Word archives. See [release notes](RELEASE_NOTES.md). The interface, default editorial policy and Word furniture currently use Simplified Chinese; public documentation is bilingual.

## What it does

- Records meaningful tool and management submissions, observed city changes and important event updates.
- Samples available city statistics, finances, transport, utilities, weather and pollution; combines repeated records.
- Excludes automatic vehicle renaming, individual boarding/alighting, UI navigation and empty previews.
- Sends eligible summaries to DeepSeek when requested. The editor chooses worthwhile stories, merges repetitions and can leave an issue without articles.
- Opens a newspaper with a paper background and columns in game, with matching editable `.docx` and JSON archives.
- Uses the city name for the masthead or a custom title, saved per city or as a global default.
- Keeps generation **manual by default**. Optional automatic publication generates the previous game day's issue and opens it in game.
- Caches completed issues locally and uses DeepSeek's prefix-cache reporting. Resampling unchanged metrics does not regenerate the same facts.

## Install and use

1. Download the complete `CityPress-<version>.zip` from [Releases](https://github.com/Daotie/CS2-CityPress/releases), not GitHub's source ZIP.
2. Exit the game, extract the full package into `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\CityPress`, and restart using your local-mod setup. Keep `Templates/`, `images/` and UI files beside the DLLs.
3. Open the newspaper button, then **编辑部设置** (editorial settings). Enter a DeepSeek API key and save. Click **生成本日新闻** to generate; daily automatic generation is optional.

Generation sends city facts to DeepSeek and may incur provider charges. The key is encrypted for the local Windows account, excluded from savegames and not displayed back to the UI. Review [privacy](SECURITY.md) before sharing a journal or newspaper.

Storage: `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\CityPress`. The settings, local journal, cache and `issues/<date>.json` / `.docx` are separate from the installation folder. Read the [detailed player guide](docs/USER_GUIDE.zh-CN.md).

## Coverage and beta status

Observed changes are separate from submitted intentions. Tool submissions are not completed-building counts; network edges are not whole roads. Existing city assets are baselined on load. Statistics and reconciliation snapshots run every five game minutes, using the game's underlying update cadence.

Independent third-party mod actions, transient intermediate states and unavailable internal data can be missed. The compatibility panel reports active management bindings and collection errors. Earlier unobserved history cannot be reconstructed. Generated articles are editable output based on available evidence, not verified reporting.

Automated core/cache/export tests, a callback harness and browser UI checks have been run locally. A complete in-game acceptance pass for the expanded collector has **not** been completed. See the validation scope in each prerelease.

## Build

Windows, .NET SDK 10, Node.js 20+ and a local game installation are required for a full build. Game assemblies are not in this repository.

```powershell
./build.ps1 -ManagedPath '<your Steam library>/steamapps/common/Cities Skylines II/Cities2_Data/Managed'
```

Alternatively set `CSII_MANAGED_PATH`. See [architecture](docs/ARCHITECTURE.md), [contributing](CONTRIBUTING.md) and [releasing](RELEASING.md).

## License and credits

Copyright (c) 2026 Daotie and contributors. CityPress is licensed under [GNU GPL v3.0 only](LICENSE). UI/build conventions follow [RouteFilter](https://github.com/Daotie/CS2-RouteFilter). Dependencies retain their licenses; see [third-party notices](THIRD_PARTY_NOTICES.md).

Not affiliated with Colossal Order, Paradox Interactive or DeepSeek. GitHub betas are local-mod packages; no Paradox Mods listing is claimed.
