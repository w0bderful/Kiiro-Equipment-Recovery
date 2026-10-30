param([string]$GameDir='D:/SteamLibrary/steamapps/common/RimWorld',[string]$WorkshopDir='D:/SteamLibrary/steamapps/workshop/content/294100')
$ErrorActionPreference='Stop'
$workspace=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
dotnet build "$PSScriptRoot/Validate.csproj" -v q "-p:GameDir=$GameDir" "-p:WorkshopDir=$WorkshopDir"
if($LASTEXITCODE -ne 0){throw 'Test build failed'}
uv run --no-project python "$PSScriptRoot/run-game-runtime.py" $workspace $GameDir $WorkshopDir
if($LASTEXITCODE -ne 0){throw 'Native XML validation failed'}
