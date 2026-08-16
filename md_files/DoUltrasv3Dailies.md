# Do Ultras v3 Dailies (Tapsi)

## Overview

`DoUltrasv3Dailies.cs` runs the first four **Ultras v3** daily bosses with army sync. It uses tapsi-local boss wrappers that respect script options for enhancements, potions, and private room.

## Bosses

| Boss | Wrapper | Quest ID | Map |
|------|---------|----------|-----|
| Ultra Ezrajal | `TapsiUltraEzrajalv3` | 8152 | `ultraezrajal` |
| Ultra Warden | `TapsiUltraWardenv3` | 8153 | `ultrawarden` |
| Ultra Engineer | `TapsiUltraEngineerv3` | 8154 | `ultraengineer` |
| Ultra Avatar Tyndarius | `TapsiUltraAvatarTyndariusv3` | 8245 | `ultratyndarius` |

Run order: **Ezrajal → Warden → Engineer → Tyndarius**.

## Script options

| Option | Default | Description |
|--------|---------|-------------|
| `UsePrerequisitesChecker` | `true` | Run prerequisite sync gate before fights. |
| `DoEnh` | `true` | Auto-enhance gear before each boss. |
| `PotionQuantity` | `10` | Stock level for potion purchases. |
| `privateRoomNumber` | `100000` | Private room for army map joins (1000–999999). |

## Features

### Army sync queue
- Sync files: `tapsi_ultrasv3_dailies_bosses.sync`, `tapsi_ultrasv3_dailies_participants.sync`
- Skips bosses already complete for the current account
- Re-runs remaining bosses if some army members still need them

### Potion active check (`TapsiPotions` in `tapsi/Ultras/TapsiPotions.cs`)
- Ported from `UltrasLW/CoreLoneWolf.cs` — lives only in the tapsi script
- Before using a tonic or elixir, checks if its aura is already active
- Logs `{potion} already active.` with tag `UsePotions` and skips re-use
- Combat potions still equip when needed; use is skipped if aura was already active
- Does **not** modify `Ultrasv3/DependenciesUltras/UltraPotions.cs`

### Tapsi boss wrappers (`tapsi/Ultras/`)
- Thin copies of the Ultras v3 individual scripts
- Call `TapsiPotions` instead of `UltraPotions` for ensure/use
- Honor `DoEnh` from parent script options
- Potions are always enabled (no toggle)
- Original Ultras v3 scripts are unchanged

## Files

```
tapsi/
├── DoUltrasv3Dailies.cs          # Main runner
├── DoUltrasv3Dailies.md
└── Ultras/
    ├── TapsiPotions.cs           # Potion helper (aura skip)
    ├── 1UltraEzrajalv3.cs        # TapsiUltraEzrajalv3
    ├── 2UltraWardenv3.cs         # TapsiUltraWardenv3
    ├── 3UltraEngineerv3.cs       # TapsiUltraEngineerv3
    └── 4UltraAvatarTyndariusv3.cs # TapsiUltraAvatarTyndariusv3
```

## Usage

1. Set up a 4-player army with the classes expected by each v3 script.
2. Configure private room number, potion quantity, and enhancement toggle.
3. Run **Do Ultras v3 Dailies (Tapsi)** from the tapsi folder.
4. The script stops when all four daily quests are complete for every synced account.

## Testing recommendations

1. Run with one character to confirm quest skip after completion.
2. Run Ezrajal then Warden — verify tonics log `already active.` on the second fight.
3. Set `DoEnh` to false and confirm gear is not re-enhanced between bosses.
4. Confirm whitemap and ultra map joins use your configured private room.
