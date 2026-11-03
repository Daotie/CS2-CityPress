param(
 [ValidateSet('NewVersion','Update')][string]$Command='NewVersion',
 [string]$ContentFolder,
 [string]$PublisherPath='D:\SteamLibrary\steamapps\common\Cities Skylines II\Cities2_Data\Content\Game\.ModdingToolchain\ModPublisher\ModPublisher.exe'
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
Push-Location $root
$previousRollForward=$env:DOTNET_ROLL_FORWARD
try {
 & "$root/tools/verify-version.ps1"
 $version=([IO.File]::ReadAllText("$root/VERSION")).Trim()
 $config=[xml][IO.File]::ReadAllText("$root/Properties/PublishConfiguration.xml")
 if($config.Publish.ModId.Value -ne '166126'){throw 'Unexpected CityPress ModId; do not create a duplicate listing.'}
 if($config.Publish.ModVersion.Value -ne $version){throw 'Publishing metadata does not match VERSION.'}
 if(!$ContentFolder){$ContentFolder="dist/staging/CityPress-$version"}
 if(!(Test-Path -LiteralPath "$ContentFolder/CityPress.dll")){throw 'Build and package this version first.'}
 if(!(Test-Path -LiteralPath $PublisherPath)){throw 'Set -PublisherPath to the official installed ModPublisher.exe.'}
 $env:DOTNET_ROLL_FORWARD='Major'
 $log="$root/artifacts/pmod-$($Command.ToLowerInvariant())-$version.log"
 & $PublisherPath $Command Properties/PublishConfiguration.xml -c $ContentFolder *> $log
 if($LASTEXITCODE -ne 0){throw "Official publisher failed. Inspect local log: $log"}
 Select-String -LiteralPath $log -Pattern 'finished successfully|metadata Updated|Id=' | ForEach-Object {$_.Line}
}finally{
 $env:DOTNET_ROLL_FORWARD=$previousRollForward
 Pop-Location
}
