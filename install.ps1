param([string]$GameDir='D:/SteamLibrary/steamapps/common/RimWorld',[string]$ConfigDir="$env:USERPROFILE/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Config")
$ErrorActionPreference='Stop'
if(Get-Process RimWorld -ErrorAction SilentlyContinue){throw 'Close RimWorld before installing.'}
$workspace=Split-Path $PSScriptRoot -Parent
$backup=Join-Path $workspace ('output/backups/kiiro-recovery-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$mods=[IO.Path]::GetFullPath((Join-Path $GameDir 'Mods'))
if([IO.Path]::GetFullPath($workspace) -eq $mods){
 $backup=Join-Path $env:LOCALAPPDATA ('Kiiro-Equipment-Recovery/backups/'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
}
New-Item -ItemType Directory -Path $backup -Force | Out-Null
$target=[IO.Path]::GetFullPath((Join-Path $mods 'Kiiro-Equipment-Recovery'))
if([IO.Path]::GetDirectoryName($target) -ne $mods){throw 'Invalid installation path'}
$config=Join-Path $ConfigDir 'ModsConfig.xml'
[xml]$doc=Get-Content -LiteralPath $config -Raw
$original=@($doc.SelectNodes('/ModsConfigData/activeMods/li') | Where-Object {$_.InnerText -eq 'NoFlag.Kiiroexpansion'})
if(!$original.Count){throw 'Enable Kiiro Equipment Expansion first.'}
if([IO.Path]::GetFullPath($PSScriptRoot) -ne $target){
 if(Test-Path -LiteralPath $target){Move-Item -LiteralPath $target -Destination "$backup/Kiiro-Equipment-Recovery"}
 New-Item -ItemType Directory -Path $target | Out-Null
 foreach($part in @('About','Patches','LoadFolders.xml','README.md')){Copy-Item -LiteralPath "$PSScriptRoot/$part" -Destination $target -Recurse -Force}
}
if(!@($doc.SelectNodes('/ModsConfigData/activeMods/li') | Where-Object {$_.InnerText -eq 'w0bderful.kiiroequipmentrecovery'}).Count){
 Copy-Item -LiteralPath $config -Destination "$backup/ModsConfig.xml"
 $node=$doc.CreateElement('li');$node.InnerText='w0bderful.kiiroequipmentrecovery'
 $ce=@($doc.SelectNodes('/ModsConfigData/activeMods/li') | Where-Object {$_.InnerText -eq 'Aruminon.KiiroEquipmentExpansion.CEPatch'})
 if($ce.Count){[void]$ce[0].ParentNode.InsertBefore($node,$ce[0])}
 else{[void]$original[0].ParentNode.InsertAfter($node,$original[0])}
 $doc.Save($config)
}
Write-Output "Installed and enabled Kiiro Equipment Recovery Fix. Backup: $backup"
