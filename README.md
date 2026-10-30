# Kiiro Equipment Recovery Fix 1.0.1

A standalone XML repair for **Kiiro Equipment Expansion** on RimWorld 1.7.

The tested local installation was missing `1.6/Defs/Apparel/Apparel_Armor.xml`, while its apparel whitelist and CE patch still referred to the missing equipment. This caused failed CE patch operations and unresolved apparel references.

## What it restores

- `Kiiro_ExoSuit`
- `Kiiro_ShadowExoSuit`
- `Kiiro_PowerArmor`
- `Kiiro_ShadowPowerArmor`
- `Kiiro_HeavyPowerArmor`
- Their missing abstract parents and associated heavy-armor shield hediff.

Every definition has its own existence check. If the original mod is repaired or updated, definitions already supplied by it remain untouched. The repair also preserves any existing CE values if applied again.

## Cosmetic label correction

The original `Kiiro_ULWeapon_SniperRifle` label ends with a space: `kiiro ultra sniper rifle `.
Vanilla appends unlocked item labels to techprint descriptions, so that space causes the harmless
`Techprint_Kiiro_UltraWeapon: description has trailing whitespace` error. The existing armor
recovery does not cause it. A separate XML operation corrects only this exact original label typo;
already-fixed labels, custom names, stats, research, and CE values are preserved. No runtime DLL
or additional dependency is needed. The warning itself does not affect gameplay.

## Requirements and load order

1. [Kiiro Equipment Expansion](https://steamcommunity.com/sharedfiles/filedetails/?id=3741356350) and its requirements.
2. **Kiiro Equipment Recovery Fix**.
3. [Kiiro Equipment Expansion CE Patch](https://steamcommunity.com/sharedfiles/filedetails/?id=3770623835), if used with Combat Extended.

CE is optional. This repair supplies the original definitions; it does not replace the existing CE balancing patch. Harmony and other patch mods are not required.

Copy this folder into `RimWorld/Mods`, enable it, and restart the game. The supplied installer backs up the active mod list and inserts only this patch at the required position. Original Workshop files are not changed.

## Source and scope

Recovered definitions come from a local backup of **Kiiro Equipment Expansion by NoFlag**, Workshop item `3741356350`, dated October 28, 2026. Original equipment data is retained; surrounding description whitespace is trimmed. Original textures remain provided by the required mod. This repository does not include those textures or other mods' DLLs.

The repair addresses missing definitions. It does not repair unrelated Kiiro mods, change research progression, or modify Immersive Filter. It cannot restore items already lost from a saved game.

## Verification

`Tests/test.ps1` runs the game's XML patch engine against the installed original and CE patch, checking restored definitions, duplicate prevention, preservation of existing values, CE armor/carrying values, and the label correction's scope and repeatability. The runtime patch contains XML only; the test executable is not needed to play.

Validation on October 30, 2026: **83 native XML, label preservation, and CE value checks passed**. A read-only observer in the current 223-mod load order plus one diagnostic helper confirmed that the original generated techprint description acquired the space when vanilla appended research unlocks. With the XML correction, its label and description passed the actual startup configuration check without trailing whitespace. No error messages were suppressed. Unrelated mod errors remained; this is not a claim that the entire mod list is error-free or that long-term combat balance has been tested.

The external checks require the .NET 10 SDK, `uv`, and local game/Workshop installations. Adjust `-GameDir` and `-WorkshopDir` when invoking `Tests/test.ps1` if your paths differ.
