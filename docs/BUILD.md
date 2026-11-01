# Build and verify / 构建与验证

## Full local build

Use Windows, .NET SDK 10, Node.js 20+ and your own Cities: Skylines II installation. Set `CSII_MANAGED_PATH` to `Cities2_Data/Managed`, or pass `-ManagedPath` to `build.ps1`. MSBuild also accepts `-p:ManagedPath=...`.

```powershell
./build.ps1 -ManagedPath '<game installation>/Cities2_Data/Managed'
```

The script builds C#, runs full core/export and game-assembly integration checks, runs the callback harness, installs/builds/type-checks the UI, audits runtime dependencies, then packages the version in `VERSION`. Existing release archives are not overwritten. Staging and archives remain under ignored `dist/`.

`tools/BindingsSmoke` exercises the actual game's callback field/delegate types with constructors bypassed outside Unity. It verifies wrapping, argument forwarding, original exceptions, raw-reader forwarding and restoration. This is a harness, not native Cohtml or live-save validation.

## Checks without game files

```powershell
dotnet run --project Tests/CityPress.Tests.csproj -c Release -- --core-only
npm ci --prefix UI
npm run typecheck --prefix UI
npm run build --prefix UI
npm audit --prefix UI --omit=dev
```

Core tests use a NuGet JSON dependency and embedded layout/template resources; no game installation, live API or provider key is needed. `--core-only` explicitly skips native assembly metadata and byte-loaded game-mod integration checks. Public GitHub Actions runs this scope, not the proprietary game build.

## 简体中文

完整构建需要本地游戏程序集，路径可通过 `CSII_MANAGED_PATH`、`build.ps1 -ManagedPath` 或 MSBuild 参数指定。构建脚本会执行完整核心/导出检查与实际绑定类型的回调检查，再构建 UI、审计运行依赖、生成安装包。已有发行压缩包不覆盖。

`--core-only` 可在没有游戏文件和 API 密钥的环境验证汇总、降噪、缓存及 Word 导出，并明确跳过游戏程序集集成。公开 CI 执行此范围及前端检查，不代表运行过游戏。回调检查绕过 Unity 构造过程，不等于原生 Cohtml 或真实存档验收。
