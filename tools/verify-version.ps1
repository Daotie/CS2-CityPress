$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$version=([IO.File]::ReadAllText((Join-Path $root 'VERSION'))).Trim()
if($version -notmatch '^\d+\.\d+\.\d+(?:-beta\.\d+)?$'){throw 'Invalid VERSION'}
$project=[xml][IO.File]::ReadAllText((Join-Path $root 'CityPress.csproj'))
$package=Get-Content -LiteralPath (Join-Path $root 'UI/package.json') -Raw | ConvertFrom-Json
$lock=Get-Content -LiteralPath (Join-Path $root 'UI/package-lock.json') -Raw | ConvertFrom-Json -AsHashtable
$mod=Get-Content -LiteralPath (Join-Path $root 'UI/mod.json') -Raw | ConvertFrom-Json
$versions=@($project.SelectSingleNode('/Project/PropertyGroup/Version').InnerText,$package.version,$lock.version,$lock['packages']['']['version'],$mod.version)
if($versions | Where-Object {$_ -ne $version}){throw 'Backend/UI version metadata does not match VERSION'}
foreach($file in @('Mod.cs','README.md','README.zh-CN.md','CHANGELOG.md','RELEASE_NOTES.md','docs/USER_GUIDE.zh-CN.md')){
 if(![IO.File]::ReadAllText((Join-Path $root $file)).Contains($version)){throw "Missing current version in $file"}
}
if($project.SelectSingleNode('/Project/PropertyGroup/PackageLicenseExpression').InnerText -ne 'GPL-3.0-only'){throw 'Unexpected project license'}
Write-Host "PASS version consistency: $version"
