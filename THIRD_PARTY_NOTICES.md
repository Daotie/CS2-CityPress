# Third-party notices

CityPress original source, its newspaper template and branding are licensed under **GPL-3.0-only**, copyright (c) 2026 Daotie and contributors. See [LICENSE](LICENSE).

The author also maintains another Cities: Skylines II project, [RouteFilter](https://github.com/Daotie/CS2-RouteFilter). CityPress and RouteFilter share some UI and build conventions; both are authored by Daotie and licensed GPL-3.0-only. They are independent projects, and CityPress does not depend on RouteFilter.

| Component | Use | License / notice |
| --- | --- | --- |
| Harmony 2.2.2 | Bundled `0Harmony.dll`, gameplay hooks | MIT, [Andreas Pardeike](https://github.com/pardeike/Harmony/tree/v2.2.2.0), [notice](licenses/Harmony-MIT.txt) |
| React 18.3.1 | JSX runtime bundled in the UI; main React provided by the game | MIT, [notice](licenses/React-MIT.txt) |
| React DOM 18.3.1 | Declared UI dependency; main runtime supplied by the game | MIT, [notice](licenses/React-DOM-MIT.txt) |
| classnames 2.3.2 | Declared UI dependency | MIT, [notice](licenses/classnames-MIT.txt) |
| Newtonsoft.Json | Supplied by the game; NuGet for offline development tests | MIT; not distributed as a game runtime assembly in CityPress packages |

Game, Colossal, Unity and Cohtml assemblies are not included. Developers must obtain game references from their own installation. The `UI/types` files describe game UI interfaces for compilation, not game runtime code. Dependencies retain their respective licenses; GPL-3.0-only describes CityPress itself.

## 简体中文

CityPress 自有源码、报纸模板和标志以 GPL-3.0-only 发布。作者的另一项目 [RouteFilter](https://github.com/Daotie/CS2-RouteFilter) 与本项目共享部分界面与构建约定；两个项目均为 Daotie 开发并以 GPL-3.0-only 发布，相互独立，CityPress 不依赖 RouteFilter。Harmony 与前端依赖继续遵循各自许可证，完整声明见 `licenses/`。发行包不包含游戏、Colossal、Unity 或 Cohtml 程序集。项目与 Colossal Order、Paradox Interactive、DeepSeek 无官方隶属关系。
