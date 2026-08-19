# Farmer Joe DoAll Progression

This is the execution order of `0FarmerJoeDoAll.cs` through `CoreFarmerJoe.DoAll()`.

## Entry Point

1. `ScriptMain` applies the shared Core options with `Core.SetOptions()`.
2. `FJDoAll()` calls `CFJ.DoAll()`.
3. When `CFJ.DoAll()` finishes, `ScriptMain` restores the shared options with `Core.SetOptions(false)`.

## Complete Progression Order

### 1. Initial Gear Storage

1. Run `Adv.GearStore(EnhAfter: true)` to store/manage gear and enhance afterward.

### 2. Level 1 to 30

1. Run `BeginnerItems()`.
2. `BeginnerItems()` first checks whether setup can be skipped:
   - It skips beginner setup when a suitable high-tier solo class and farm class already exist.
   - It also skips when the account is level 30 or higher and it has a rank 10 beginner solo class (`Assassin`, `Ninja Warrior`, or `Ninja`) and a rank 10 beginner farm class (`Mage (Rare)` or `Mage`).
3. If setup is not skipped, complete the Tutorial badges.
4. For each equipment category, equip missing beginner gear:
   - Buy and equip `Battle Oracle Hood` if no helm is equipped.
   - Buy and equip `Battle Oracle Wings` if no cape is equipped.
   - Replace a default weapon with `Battle Oracle Battlestaff`, then sell the default weapon.
5. Level to 10 with `Farm.Experience(10)`.
6. Buy `Rogue` if neither `Rogue (Rare)` nor `Rogue` is owned.
7. If no starter dodge class is owned (`Assassin`, `Ninja Warrior`, or `Ninja`):
   - Rank up `Rogue`.
   - Level to 25.
   - Select an available class.
   - Complete the Mazumi quests.
   - Buy `Ninja`.
8. Select an available class again.
9. Buy `Mage` if neither `Mage (Rare)` nor `Mage` is owned.
10. Select an available class again.
11. Log the warning that class points can desync at Rank 9 and that a relog may be required if progression gets stuck.

### 3. Level 30 to 75 Brackets

1. Run `SetClass()` to read the configured class preferences, resolve available classes, rank selected classes if needed, and equip the solo class.
2. If the player is level 30 or higher and the `Elders' Blood` daily is available, complete the daily.
3. Log the bracket progression message.
4. If the `GetBoosts` option is enabled:
   - Ensure 10 REP boosts.
   - Ensure 10 XP boosts unless already level 100.
   - Request the selected free boosts with `Boosts.GetBoostsSelect(10, 10, 0)`.
5. Process these level targets in order: `30`, `50`, `55`, `60`, `65`, `70`, `75`, and `80`.
6. At the start of each target:
   - Apply Smart Enhancement when the current bracket has not been enhanced or an inventory item is below the player level.
   - Level with `Farm.Experience(target)`.
7. Run the target-specific handler.
8. Bank all inventory classes that are not the configured solo, farm, or dodge classes.
9. Reset the bracket enhancement flag.

#### Level 30 Handler

1. Acquire and rank 10 `Master Ranger` if it is missing or not rank 10.
2. Sell `Venom Head` if it is in the inventory.
3. Acquire and rank 10 `Dragonslayer` if it is missing or not rank 10.
4. Farm Blade of Awe reputation.
5. Buy `Awethur's Accoutrements` from the Museum.
6. Unlock the forge helm enhancement.

#### Level 50 Handler

1. Acquire and rank 10 `Scarlet Sorceress` if needed.
2. Acquire and rank 10 `Dragonslayer General` if needed.
3. Acquire `Burning Blade`.

#### Level 55 Handler

1. Acquire and rank 10 `Blaze Binder` if needed.
2. Acquire and rank 10 `Cryomancer` if needed.

#### Level 60 Handler

1. Acquire and rank 10 `DragonSoul Shinobi` if needed.
2. Unlock the `Lacerate` forge enhancement.

#### Level 65 Handler

1. Acquire and rank 10 `Glacial Berserker` if needed.

#### Level 70 Handler

1. Buy `Horc Evader` from Bloodtusk if it is missing or not rank 10.
2. Select an available class.
3. Unlock the easy level 70 forge enhancements:
   - `Smite`
   - `Vim`
   - `Pneuma`
   - `Examen`
   - `Anima`
4. Acquire and rank 10 `ArchPaladin` if needed.

#### Level 75 Handler

1. Acquire `Archfiend DoomLord` when it is missing, the Archfiend class is not rank 10, or the account lacks at least a 30% all-damage boost.
2. Acquire and rank 10 `Archfiend` if needed.

#### Level 80 Handler

1. Check for `Void Highlord` or `Void Highlord (IoDA)`.
2. If one is found, log that the class was found but not rank 10, select an available class, and return from the handler.
3. Otherwise, start `VHL.GetVHL()` to acquire Void Highlord. The script notes that Elders' Blood is a daily and the process can take 17 days minus existing resources.

### 4. Level 75 to 100 Progression

#### Phase 1: Healing Class Preparation

1. If `Dragon of Time` exists but is below rank 10, log that it needs ranking and select an available class.
2. Otherwise, check for `Healer (Rare)` or `Healer`.
3. If neither healer exists, buy `Healer` from Class Hall and rank it up.

#### Phase 2: Cape of Awe and 13 Lords of Chaos

1. Acquire `Enchanted Cape of Awe` with `COA.GetCoA()`.
2. Equip `Cape of Awe`.
3. Select an available class.
4. Complete the 13 Lords of Chaos progression with `LOC.Complete13LOC()`.

#### Phase 3: Lord of Order, Classes, and Enhancements

1. Acquire `Lord of Order` with `LOO.GetLoO()`.
2. Select an available class.
3. Bank the rewards from quest `7156`.
4. Process these additional classes one at a time:
   - `Frost Spirit Reaver`
   - `Northlands Monk`
   - `Shaman`
5. For each class, attempt acquisition once if it is not owned.
6. If acquisition still fails, log that the class is unavailable and skip it.
7. If the class exists but is not rank 10, run its getter again to rank it.
8. Log the helmet-enhancement phase. The current code does not call a helmet unlock method at this point.
9. Unlock weapon enhancements with `ForgeWeaponEnhancement()` and `Praxis()`.

#### Phase 4: Leveling and Endgame Items

1. Level to 80.
2. Complete the Celestial Arena quest line.
3. Acquire `Blinding Bright of Awe` (`BBoA`).
4. Unlock cape enhancements:
   - `ForgeCapeEnhancement()`
   - `Absolution`
   - `Vainglory`
   - `Avarice`
   - `Lament`
5. Acquire Yami No Ronin with `YNR.GetYnR()`.
6. Select an available class.
7. Acquire Dragon of Time with `DoT.GetDoT()`.
8. Select an available class.
9. Level to 100 with `Farm.Experience()`.
10. Select an available class.
11. Acquire King's Echo with `KE.GetKE()`.
12. Buy all merge requirements for `Hollowborn Reaper's Scythe` through `ShadowrealmMerge`.
13. Log that the level 75 to 100 progression is complete.

### 5. Endgame Preparation

1. If the `OutFit` option is enabled, run `Outfit()` once here.
2. Select an available class.
3. Prepare remaining forge enhancements:
   - Always run `HerosValiance()`.
   - Run `Elysium()` only when `The Divine Will` is owned.
   - `ArcanasConcerto()` and `DauntLess()` are currently commented out and do not run.
   - Run `Ravenous()` only when `Void Highlord` and at least 10 `Roentgenium of Nulgath` are owned.
4. Run the Exalted Apotheosis prerequisites with `ExaltedApotheosisPreReqs.PreReqs()`.
5. Max Nation materials through `Nation.Supplies()`.

### 6. Outfit Setup

`DoAll()` then unconditionally runs `Outfit()` after `EndGame()`.

1. Select an available class.
2. Acquire and prepare the basic outfit items:
   - Merge `NO BOTS Armor` through Synderes Merge.
   - Buy `Scarecrow Hat` from Yulgar.
3. Acquire and equip `The Server is Down` by hunting `Rabid Server Hamster` in `undergroundlabb` if it is missing.
4. Run Smart Enhancement on the current class.
5. Run `Pets()`.
6. If `EquipOutfit` is enabled, equip:
   - `NO BOTS Armor`
   - `Scarecrow Hat`
   - `The Server is Down`
   - `Hollowborn Reaper's Scythe`
7. Equip the configured pet choice.
8. Log the Farmer Joe outfit message.

### 7. Pet Calls at the End

1. Call `Pets(PetChoice.HotMama)`.
2. Call `Pets(PetChoice.Akriloth)`.
3. In the current implementation, the `petChoice` argument is not used. `Pets()` reads the `Pets` configuration key instead. Therefore these two calls only acquire a pet when that configuration value is set to the corresponding pet choice:
   - For `HotMama`, hunt `Hot Mama` in `battleundere` if missing, wait for pickup, and equip it.
   - For `Akriloth`, hunt `Ultra Akriloth` in `gravestrike` for `Akriloth Pet` if missing, wait for pickup, and equip it.
   - If the configuration value is `None`, the method returns without doing anything.

### 8. Final Gear Storage

1. Run `Adv.GearStore(true, EnhAfter: true)` one final time to store/manage gear and enhance afterward.
2. Return to `ScriptMain`.
3. Restore the shared Core options with `Core.SetOptions(false)`.

## Options That Affect the Run

- `OutFit`: runs `Outfit()` inside `EndGame()`; `DoAll()` also runs `Outfit()` unconditionally afterward.
- `EquipOutfit`: equips the complete Farmer Joe outfit during `Outfit()`.
- `SellStarterClasses`: is declared as an option but is not read by the shown progression code.
- `GetBoosts`: enables the REP, XP, and free-boost farming in the level 30 to 75 phase.
- `PetChoice`: is declared as an option, but `Pets()` currently reads `Pets` rather than `PetChoice`.
- `SkipOptions`: is passed through the Core option collection.
