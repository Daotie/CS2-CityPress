param([string]$ManagedPath=$env:CSII_MANAGED_PATH)
$ErrorActionPreference='Stop'
Push-Location $PSScriptRoot
try {
 if([string]::IsNullOrWhiteSpace($ManagedPath)){
  $ManagedPath=Join-Path ${env:ProgramFiles(x86)} 'Steam/steamapps/common/Cities Skylines II/Cities2_Data/Managed'
 }
 if(!(Test-Path -LiteralPath (Join-Path $ManagedPath 'Game.dll'))){throw 'Set -ManagedPath or CSII_MANAGED_PATH to your own Cities2_Data/Managed directory.'}
 $version=([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'VERSION'))).Trim()
 if($version -notmatch '^\d+\.\d+\.\d+(?:-beta\.\d+)?$'){throw 'Invalid VERSION'}
 $archive=Join-Path $PSScriptRoot "dist/CityPress-$version.zip"
 if(Test-Path -LiteralPath $archive){throw "Archive already exists: $archive. Use a new version for a new release."}
 dotnet build CityPress.csproj -c Release "-p:ManagedPath=$ManagedPath"
 if($LASTEXITCODE -ne 0){throw 'C# build failed'}
 dotnet run --project Tests/CityPress.Tests.csproj -c Release
 if($LASTEXITCODE -ne 0){throw 'Core/export and game-assembly integration checks failed'}
 dotnet build tools/BindingsSmoke.csproj -c Release "-p:ManagedPath=$ManagedPath"
 if($LASTEXITCODE -ne 0){throw 'Binding harness build failed'}
 dotnet tools/bin/Release/net10.0/BindingsSmoke.dll $ManagedPath "$PSScriptRoot/bin/Release/net48/CityPress.dll"
 if($LASTEXITCODE -ne 0){throw 'Gameplay callback harness failed'}
 npm ci --prefix UI --no-audit --no-fund
 if($LASTEXITCODE -ne 0){throw 'UI install failed'}
 npm run typecheck --prefix UI
 if($LASTEXITCODE -ne 0){throw 'UI type check failed'}
 npm run build --prefix UI
 if($LASTEXITCODE -ne 0){throw 'UI build failed'}
 npm audit --prefix UI --omit=dev
 if($LASTEXITCODE -ne 0){throw 'Runtime dependency audit failed'}
 $out=Join-Path $PSScriptRoot "dist/staging/CityPress-$version"
 if(Test-Path -LiteralPath $out){throw "Staging already exists: $out. Inspect it before building again."}
 New-Item -ItemType Directory -Force "$out/Templates" | Out-Null
 Copy-Item bin/Release/net48/CityPress.dll $out
 Copy-Item bin/Release/net48/0Harmony.dll $out
 Copy-Item UI/build/* $out -Recurse
 Copy-Item Templates/newspaper.docx "$out/Templates"
 foreach($file in @('README.md','README.zh-CN.md','CHANGELOG.md','RELEASE_NOTES.md','LICENSE','THIRD_PARTY_NOTICES.md','SECURITY.md')){Copy-Item -LiteralPath $file -Destination $out}
 foreach($directory in @('licenses','docs','assets')){Copy-Item -LiteralPath $directory -Destination $out -Recurse}
 $commit=(git rev-parse HEAD).Trim()
 if($LASTEXITCODE -ne 0){throw 'Commit source before packaging'}
 $metadata=[ordered]@{
  version=$version;sourceCommit=$commit;sourceUrl="https://github.com/Daotie/CS2-CityPress/tree/$commit"
  gameAssemblyVersion=[Reflection.AssemblyName]::GetAssemblyName((Join-Path $ManagedPath 'Game.dll')).Version.ToString()
  gameProductVersion=[Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $ManagedPath 'Game.dll')).ProductVersion
  builtAtUtc=[DateTime]::UtcNow.ToString('o')
 }
 $metadata | ConvertTo-Json | Set-Content -LiteralPath "$out/build-info.json" -Encoding utf8
 Compress-Archive -Path "$out/*" -DestinationPath $archive
 $hash=(Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
 "$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath "$archive.sha256" -Encoding ascii
 Write-Host "Built $archive"
 Write-Host "SHA256 $hash"
}finally{Pop-Location}
