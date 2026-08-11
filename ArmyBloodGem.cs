/*
name: Army Blood Gem
description: Army-farms Blood Gem of the Archfiend via Bloody Chaos with shared private room support and helper mode so finished accounts keep attacking to speed up the rest of the army.
tags: blood gem, bloodgem, blood, gem, nation, materials, army, sync
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Nation/CoreNation.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreEnginev3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreUltrav3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraGeneral.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraEnhancements.cs
//cs_include Scripts/Ultrasv3/ArmyDependencies/ArmyGeneral.cs
//cs_include Scripts/Army/CoreArmyLite.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Skua.Core.Interfaces;
using Skua.Core.Models.Monsters;
using Skua.Core.Options;

public class ArmyBloodGem
{
    private static IScriptInterface Bot => IScriptInterface.Instance;
    private static CoreBots Core => CoreBots.Instance;
    private static CoreEnginev3 Engine => _Engine ??= new CoreEnginev3();
    private static CoreEnginev3 _Engine;
    private static CoreFarms Farm => _Farm ??= new CoreFarms();
    private static CoreFarms _Farm;
    private static UltraEnhancements Enh => _Enh ??= new UltraEnhancements();
    private static UltraEnhancements _Enh;
    private static CoreArmyLite Army => _Army ??= new CoreArmyLite();
    private static CoreArmyLite _Army;
    private static CoreNation Nation => _Nation ??= new CoreNation();
    private static CoreNation _Nation;

    public CoreUltrav3 Ultra = new();
    public bool DontPreconfigure = true;
    public string OptionsStorage = "ArmyBloodGem";

    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Blood Gem Quantity", "Number of Blood Gems of the Archfiend to farm.", 100),
        new Option<bool>("BloodyChaos", "Do Bloody Chaos", "Army-synced Bloody Chaos farming. Requires an army for Hydra.", true),
        new Option<HydraLevel>("HydraLevel", "Hydra Lvl to kill", "", HydraLevel.Head_85),
        new Option<int>("ArmySize", "Army Size", "How many players are in your army (including yourself). Set to 1 for solo.", 4),
        new Option<int>(
            "privateRoomNumber",
            "Private Room Number",
            "Private room number used by all army accounts. Set the same number on every account (1000-99999).",
            12345
        ),
        new Option<bool>("UseArmySync", "Enable Army Sync", "Use army sync to coordinate Bloody Chaos item progress.", true),
        new Option<bool>(
            "HelpOthersWhenDone",
            "Help Others When Done",
            "If this account already has the current stage item, keep attacking to help other army members finish faster.",
            true
        ),
        new Option<bool>("DoEnh", "Do Enhancements", "Apply UltraEnhancements for the equipped class before farming.", true),
        new Option<string>("Account1", "Account 1", "Account name only for preset 1.", ""),
        new Option<string>("Account2", "Account 2", "Account name only for preset 2.", ""),
        new Option<string>("Account3", "Account 3", "Account name only for preset 3.", ""),
        new Option<string>("Account4", "Account 4", "Account name only for preset 4.", ""),
        new Option<string>("Account5", "Account 5", "Account name only for preset 5.", ""),
        new Option<string>("Account6", "Account 6", "Account name only for preset 6.", ""),
        new Option<string>("Account7", "Account 7", "Account name only for preset 7.", ""),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quant = Bot.Config!.Get<int>("Quantity");
        int armySize = Bot.Config.Get<int>("ArmySize");
        bool useArmySync = Bot.Config.Get<bool>("UseArmySync");
        int hydraLevel = (int)Bot.Config.Get<HydraLevel>("HydraLevel");

        if (useArmySync && armySize > 1)
        {
            SetupPrivateRoom();
            SetupArmy(armySize, hydraLevel);
        }
        else if (useArmySync)
        {
            Core.Logger("Army sync is enabled but Army Size is 1. Set Army Size to your real army count on every account.");
        }

        if (Bot.Config.Get<bool>("DoEnh"))
            DoEnhs();

        if (Bot.Config.Get<bool>("BloodyChaos"))
            BloodyChaosArmy(quant, useArmySync, hydraLevel);
        else
            Nation.FarmBloodGem(quant, hydraLevel);

        Core.SetOptions(false);
    }

    public void BloodyChaosArmy(int quant = 100, bool useArmySync = false, int hydraLevel = 85)
    {
        if (Core.CheckInventory("Blood Gem of the Archfiend", quant) || Bot.Player.Level < 80)
            return;

        Core.AddDrop("Blood Gem of the Archfiend", "Hydra Scale Piece", "Relic of Chaos");
        Core.FarmingLogger("Blood Gem of the Archfiend", quant);

        if (!useArmySync)
        {
            Nation.BloodyChaos(quant, false, hydraLevel);
            return;
        }

        RetrieveBloodGems(quant, useArmySync, hydraLevel);
    }

    public void RetrieveBloodGems(int quant = 100, bool useArmySync = false, int hydraLevel = 85)
    {
        if (Core.CheckInventory("Blood Gem of the Archfiend", quant) || Bot.Player.Level < 80)
            return;

        bool helpOthers = Bot.Config!.Get<bool>("HelpOthersWhenDone");
        string bloodGemSyncFile = "ArmyBloodGem_BloodGem.sync";
        string stageSyncFile = "ArmyBloodGem_stage.sync";
        int armySize = Bot.Config.Get<int>("ArmySize");
        var stages = GetBloodyChaosStages(hydraLevel);

        Core.AddDrop(stages.Select(s => s.item).Append("Blood Gem of the Archfiend").Append("Relic of Chaos").ToArray());
        Core.EquipClass(ClassType.Solo);
        string cycleSyncFile = "ArmyBloodGem_Cycle.sync";

        while (!Bot.ShouldExit)
        {
            if (!HasAllBloodyChaosItems(stages) || !Bot.Quests.CanComplete(7816))
                Core.EnsureAccept(7816);
            if (Core.CheckInventory("Blood Gem of the Archfiend", quant))
            {
                if (
                    !useArmySync
                    || !helpOthers
                    || CheckArmyAllMembersComplete(
                        () => Core.CheckInventory("Blood Gem of the Archfiend", quant),
                        bloodGemSyncFile,
                        armySize
                    )
                )
                    break;

                Core.Logger("Army members still farming Blood Gems. Staying in the fight to help others.");
            }

            int ownStage = GetOwnStage(stages);
            int stageIndex = ownStage;

            if (useArmySync)
            {
                // Publish our current stage so other accounts can see it.
                ArmyGeneral.PublishAndGetArmyStage(Ultra, Bot, ownStage, stageSyncFile, armySize);

                // Wait briefly for all army members to register their stage entries.
                int ticks = 0;
                const int maxTicks = 6; // ~3s
                while (!ArmyGeneral.IsFullArmyActive(Ultra, stageSyncFile, armySize) && !Bot.ShouldExit && ticks++ < maxTicks)
                    Bot.Sleep(500);

                // Re-read army stage (stable after full registration)
                stageIndex = ArmyGeneral.PublishAndGetArmyStage(Ultra, Bot, ownStage, stageSyncFile, armySize);

                Core.Logger($"Own stage {ownStage}, army stage {stageIndex}");
            }

            if (stageIndex >= stages.Length)
            {
                if (
                    !useArmySync
                    || CheckArmyAllMembersComplete(() => HasAllBloodyChaosItems(stages), cycleSyncFile, armySize)
                )
                {
                    EnsureExitedHydraChallenge();
                    Bot.Sleep(500);
                    TurnInBloodyChaosQuest();
                }
                else
                {
                    Core.Logger("Waiting for all army members to finish Bloody Chaos items before turning in quest.");
                }

                Core.FarmingLogger("Blood Gem of the Archfiend", quant);
                Bot.Sleep(500);
                continue;
            }

            var stage = stages[stageIndex];
            string syncFile = ArmyGeneral.GetSyncFileForItem("ArmyBloodGem_Item", stage.item);
            bool stageSkillsEnabled = false;
            bool combatSynced = false;

            while (!Bot.ShouldExit)
            {
                ownStage = GetOwnStage(stages);
                RefreshStageSync(stageSyncFile, ownStage);

                if (useArmySync)
                {
                    int armyStageNow = ArmyGeneral.PublishAndGetArmyStage(Ultra, Bot, ownStage, stageSyncFile, armySize);
                    if (armyStageNow != stageIndex)
                    {
                        Core.Logger($"Army stage changed {stageIndex} -> {armyStageNow}. Re-syncing.");
                        break;
                    }
                }

                if (
                    CheckArmyAllMembersComplete(
                        () => Core.CheckInventory(stage.item, stage.quant),
                        syncFile,
                        useArmySync ? armySize : 1
                    )
                )
                    break;

                bool stageComplete = Core.CheckInventory(stage.item, stage.quant);
                bool isHelper = useArmySync && helpOthers && (stageComplete || IsPostCycleHydraHelper(stages, stage));

                if (isHelper)
                    Core.Logger($"Army helper on {stage.item}: {GetItemQuantity(stage.item)}/{stage.quant}");
                else
                    Core.Logger($"Army stage {stageIndex + 1}/{stages.Length}: {stage.item}");

                Core.FarmingLogger(stage.item, stage.quant);

                if (useArmySync && !combatSynced)
                {
                    WaitArmyBeforeCombat(stageIndex, armySize);
                    combatSynced = true;
                }

                RunStageCombat(stage, isHelper, useArmySync, ref stageSkillsEnabled);
                Bot.Sleep(isHelper ? 500 : 100);
            }

            if (stageSkillsEnabled)
            {
                Engine.DisableSkills();
                stageSkillsEnabled = false;
            }

            ResetCombatOptions();
        }

        Core.CancelRegisteredQuests();
    }

    private (string map, int? monsterID, string? monsterName, string item, int quant)[] GetBloodyChaosStages(int hydraLevel) =>
    [
        ("escherion", 3, null, "Escherion's Helm", 1),
        ("stalagbite", 7, null, "Shattered Legendary Sword of Dragon Control", 1),
        ("hydrachallenge", null, $"Hydra Head {hydraLevel}", "Hydra Scale Piece", 200),
    ];

    private void RunStageCombat(
        (string map, int? monsterID, string? monsterName, string item, int quant) stage,
        bool isHelper,
        bool useArmySync,
        ref bool stageSkillsEnabled
    )
    {
        EnsureStageSkills(ref stageSkillsEnabled);

        if (isHelper)
        {
            JoinStageMap(stage.map);
            PrepareStageMap(stage);

            if (!Bot.Player.Alive)
            {
                Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
                return;
            }

            if (Bot.Player.Cell == null || Bot.Player.Cell.Equals("Enter", StringComparison.OrdinalIgnoreCase))
                PrepareHelperPosition(stage);
            else
                EnsureHelperOnTargetCell(stage);

            RunStageAttack(stage);
            return;
        }

        if (!useArmySync)
        {
            FarmStageSolo(stage);
            return;
        }

        FarmStageArmy(stage);
    }

    private void FarmStageSolo((string map, int? monsterID, string? monsterName, string item, int quant) stage)
    {
        if (Core.CheckInventory(stage.item, stage.quant))
            return;

        if (stage.map == "escherion")
        {
            Core.KillEscherion(stage.item, stage.quant, false);
            return;
        }

        if (stage.map == "stalagbite")
        {
            Core.KillVath(stage.item, stage.quant, false);
            return;
        }

        if (!string.IsNullOrEmpty(stage.monsterName))
        {
            Core.HuntMonster(stage.map, stage.monsterName!, stage.item, stage.quant, false);
            return;
        }

        if (stage.monsterID.HasValue)
            Core.HuntMonsterMapID(stage.map, stage.monsterID.Value, stage.item, stage.quant, false, false, false);
    }

    private void FarmStageArmy((string map, int? monsterID, string? monsterName, string item, int quant) stage)
    {
        if (Core.CheckInventory(stage.item, stage.quant))
            return;

        JoinStageMap(stage.map);
        PrepareStageMap(stage);

        if (!Bot.Player.Alive)
        {
            Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
            return;
        }

        RunStageAttack(stage);
    }

    private void RunStageAttack((string map, int? monsterID, string? monsterName, string item, int quant) stage)
    {
        if (stage.map == "escherion")
        {
            AttackEscherion();
            return;
        }

        if (stage.map == "stalagbite")
        {
            AttackVath();
            return;
        }

        if (!string.IsNullOrEmpty(stage.monsterName))
        {
            Engine.ChooseBestCell(stage.monsterName!);
            if (!Bot.Player.HasTarget)
                Bot.Combat.Attack(stage.monsterName!);
            return;
        }

        if (!stage.monsterID.HasValue)
            return;

        var target = Bot.Monsters.MapMonsters
            .FirstOrDefault(m => m != null && m.MapID == stage.monsterID.Value && m.Alive);

        if (target != null && !string.Equals(Bot.Player.Cell, target.Cell, StringComparison.OrdinalIgnoreCase))
        {
            Bot.Map.Jump(target.Cell, "Left", autoCorrect: false);
            Bot.Wait.ForCellChange(target.Cell);
        }

        if (!Bot.Player.HasTarget || Bot.Player.Target?.MapID != stage.monsterID.Value)
            Bot.Combat.Attack(stage.monsterID.Value);
    }

    private void AttackEscherion()
    {
        if (!Bot.Player.HasTarget)
            Bot.Combat.Attack(3);
        else if (
            Bot.Player.Target?.MapID == 3
            && Bot.Player.Target?.State == 2
            && Bot.Monsters.MapMonsters.FirstOrDefault(x => x != null && x.MapID == 2)?.Alive == true
        )
            Bot.Combat.Attack(2);
        else if (Bot.Player.Target?.MapID == 2 && Bot.Player.Target?.HP > 0)
            Bot.Combat.Attack(2);
        else
            Bot.Combat.Attack(3);
    }

    private void AttackVath()
    {
        Monster? vath = Bot.Monsters.MapMonsters.FirstOrDefault(x => x?.MapID == 7);
        Monster? stalagbite = Bot.Monsters.MapMonsters.FirstOrDefault(x => x?.MapID == 8);

        if (stalagbite != null)
        {
            Bot.Wait.ForMonsterSpawn(stalagbite.Name);
            if (vath != null)
                Bot.Combat.Attack(stalagbite.State is 1 or 2 ? stalagbite : vath);
        }
    }

    private void PrepareStageMap((string map, int? monsterID, string? monsterName, string item, int quant) stage)
    {
        if (stage.map == "escherion")
            EnsureEscherionPosition();
        else if (stage.map == "stalagbite")
            EnsureVathPosition();
    }

    private void EnsureEscherionPosition()
    {
        if (Bot.Map.Name == "escherion" && Bot.Player.Cell != "Cut1" && Bot.Player.Cell == "Boss")
            return;

        if (Bot.Player.Cell != "Boss")
        {
            Bot.Map.Jump("Boss", "Left", autoCorrect: false);
            Bot.Wait.ForCellChange("Boss");
            Bot.Player.SetSpawnPoint();
        }

        if (Bot.Player.Cell == "Cut1")
        {
            Bot.Map.Jump("Boss", "Left", autoCorrect: false);
            Bot.Wait.ForCellChange("Boss");
            Bot.Player.SetSpawnPoint();
        }
    }

    private void EnsureVathPosition()
    {
        if (!string.Equals(Bot.Player.Cell, "r2", StringComparison.OrdinalIgnoreCase))
        {
            Bot.Map.Jump("r2", "Left", autoCorrect: false);
            Bot.Wait.ForCellChange("r2");
        }
    }

    private void EnsureStageSkills(ref bool stageSkillsEnabled)
    {
        if (stageSkillsEnabled)
            return;

        Engine.EnableSkills();
        stageSkillsEnabled = true;
    }

    private void RefreshStageSync(string stageSyncFile, int ownStage)
    {
        string? username = Bot.Player.Username;
        if (string.IsNullOrWhiteSpace(username))
            return;

        string key = username.Replace(":", "-");
        Ultra.UpdateEntry(Ultra.ResolveSyncPath(stageSyncFile), key, ownStage.ToString());
    }

    private void PrepareHelperPosition((string map, int? monsterID, string? monsterName, string item, int quant) stage)
    {
        EnsureHelperOnTargetCell(stage);
        Bot.Player.SetSpawnPoint();
        Bot.Sleep(500);
    }

    private void EnsureHelperOnTargetCell((string map, int? monsterID, string? monsterName, string item, int quant) stage)
    {
        if (stage.map == "escherion")
        {
            EnsureEscherionPosition();
            return;
        }

        if (stage.map == "stalagbite")
        {
            EnsureVathPosition();
            return;
        }

        if (!string.IsNullOrEmpty(stage.monsterName))
        {
            Engine.ChooseBestCell(stage.monsterName!);
            return;
        }

        if (!stage.monsterID.HasValue)
            return;

        var target = Bot.Monsters.MapMonsters
            .FirstOrDefault(m => m != null && m.MapID == stage.monsterID.Value && m.Alive);

        if (target == null || string.IsNullOrWhiteSpace(target.Cell))
            return;

        if (string.Equals(Bot.Player.Cell, target.Cell, StringComparison.OrdinalIgnoreCase))
            return;

        Bot.Map.Jump(target.Cell, "Left", autoCorrect: false);
        Bot.Wait.ForCellChange(target.Cell);
    }

    private void JoinStageMap(string map)
    {
        if (string.Equals(Bot.Map.Name, map, StringComparison.OrdinalIgnoreCase))
            return;

        Core.Join(map);
        Bot.Wait.ForMapLoad(map);
    }

    private void ResetCombatOptions()
    {
        Bot.Options.AttackWithoutTarget = false;
        Bot.Options.AggroAllMonsters = false;
    }

    private int GetOwnStage((string map, int? monsterID, string? monsterName, string item, int quant)[] stages)
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (!Core.CheckInventory(stages[i].item, stages[i].quant))
                return i;
        }

        return stages.Length;
    }

    private void TurnInBloodyChaosQuest()
    {
        Core.EnsureAccept(7816);
        Bot.Sleep(200);
        Core.EnsureComplete(7816);
        Bot.Wait.ForQuestComplete(7816);
        Bot.Sleep(200);
        Bot.Wait.ForPickup("Blood Gem of the Archfiend");
        Bot.Sleep(200);
        Core.EnsureAccept(7816);
    }

    private void EnsureExitedHydraChallenge()
    {
        if (!string.Equals(Bot.Map.Name, "hydrachallenge", StringComparison.OrdinalIgnoreCase))
            return;

        if (string.Equals(Bot.Player.Cell, "Enter", StringComparison.OrdinalIgnoreCase))
            return;

        Core.Logger("Exiting hydrachallenge to Enter, Spawn before quest turn-in.");
        Bot.Map.Jump("Enter", "Spawn", autoCorrect: false);
        Bot.Wait.ForCellChange("Enter");
        Bot.Sleep(500);
    }

    private bool HasAllBloodyChaosItems((string map, int? monsterID, string? monsterName, string item, int quant)[] stages) =>
        stages.All(s => Core.CheckInventory(s.item, s.quant));

    private bool CheckArmyAllMembersComplete(Func<bool> condition, string syncFilePath, int armySize)
    {
        if (armySize <= 1)
            return condition();

        if (!Bot.Player.Alive)
            Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);

        if (string.IsNullOrWhiteSpace(Bot.Player.Username))
            Bot.Wait.ForTrue(() => !string.IsNullOrWhiteSpace(Bot.Player.Username), 20);

        string? username = Bot.Player.Username;
        string? className = Bot.Player.CurrentClass?.Name;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(className))
            return false;

        string syncFile = Ultra.ResolveSyncPath(syncFilePath);
        string myKey = $"{username}|{className}".Replace(":", "-");
        bool myCondition = condition();
        Ultra.UpdateEntry(syncFile, myKey, myCondition ? "1" : "0");

        string[] lines = Ultra.ReadLines(syncFile);
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        const int staleThreshold = 600;

        int activeMembers = 0;
        int completedMembers = 0;

        foreach (string line in lines)
        {
            string[] parts = line.Split(':');
            if (parts.Length < 3)
                continue;

            string[] keyParts = parts[0].Split('|');
            if (keyParts.Length < 1 || string.IsNullOrWhiteSpace(keyParts[0]))
                continue;

            if (!int.TryParse(parts[1], out int status))
                continue;

            if (!long.TryParse(parts[2], out long ts))
                continue;

            if (now - ts > staleThreshold)
                continue;

            activeMembers++;
            if (status == 1)
                completedMembers++;
        }

        return activeMembers >= armySize && completedMembers == activeMembers;
    }

    private bool IsPostCycleHydraHelper(
        (string map, int? monsterID, string? monsterName, string item, int quant)[] stages,
        (string map, int? monsterID, string? monsterName, string item, int quant) stage
    )
    {
        if (stage.item != stages[^1].item)
            return false;

        return GetOwnStage(stages) == 0
            && !Core.CheckInventory(stages[0].item, stages[0].quant)
            && !Core.CheckInventory(stage.item, stage.quant);
    }

    private int GetItemQuantity(string item) =>
        Math.Max(Bot.TempInv.GetQuantity(item), Bot.Inventory.GetQuantity(item));

    private void WaitArmyBeforeCombat(int stageIndex, int armySize)
    {
        string syncFile = $"ArmyBloodGem_combat_{stageIndex}.sync";
        Core.Logger($"Waiting for army before stage {stageIndex + 1} combat...");
        Ultra.WaitForArmy(armySize - 1, syncFile);
        Bot.Sleep(500);
    }

    private void SetupPrivateRoom()
    {
        Core.PrivateRooms = true;

        int configuredRoom = Bot.Config!.Get<int>("privateRoomNumber");
        if (configuredRoom >= 1000 && configuredRoom <= 99999)
        {
            Core.PrivateRoomNumber = configuredRoom;
        }
        else
        {
            Core.Logger($"Invalid private room number '{configuredRoom}'. Generating a fallback room number.");
            Core.PrivateRoomNumber = Army.getRoomNr();
        }

        Core.Logger($"Army private room set to #{Core.PrivateRoomNumber}. Use this same number on every account.");
    }

    private void SetupArmy(int armySize, int hydraLevel)
    {
        string readySyncFile = "ArmyBloodGem.ready.sync";
        var stages = GetBloodyChaosStages(hydraLevel);

        Ultra.ClearSyncFile(Ultra.ResolveSyncPath(readySyncFile));
        Bot.Sleep(2500);
        Core.Logger($"Waiting for army ready ({armySize - 1} other players) in room #{Core.PrivateRoomNumber}...");
        Ultra.WaitForArmy(armySize - 1, readySyncFile);

        ArmyGeneral.PrepareSyncFiles(
            Ultra,
            "ArmyBloodGem_BloodGem.sync",
            stages.Select(s => s.item),
            "ArmyBloodGem_Item"
        );
        ArmyGeneral.PrepareStageSync(Ultra, "ArmyBloodGem_stage.sync");
        Ultra.ClearSyncFile(Ultra.ResolveSyncPath("ArmyBloodGem_Cycle.sync"));
    }

    private void DoEnhs()
    {
        Core.Logger("Applying UltraEnhancements...");
        Enh.Apply();
    }

    private enum HydraLevel
    {
        Head_85 = 85,
        Head_90 = 90,
    }
}
