/*
name: Army Elemental Core
description: Army-farms the Elemental Core from Shadows of War using the Void Aura army template with shared private room setup, helper mode, and sync support.
tags: elemental core, shadows of war, farm, army, sync
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreEnginev3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreUltrav3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraGeneral.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraEnhancements.cs
//cs_include Scripts/Ultrasv3/ArmyDependencies/ArmyGeneral.cs
//cs_include Scripts/Army/CoreArmyLite.cs
//cs_include Scripts/ShadowsOfWar/CoreSoWMats.cs
//cs_include Scripts/Story/ShadowsOfWar/CoreSoW.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class ArmyElementalCore
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
    private static CoreSoWMats SOWM
    {
        get => _SOWM ??= new CoreSoWMats();
        set => _SOWM = value;
    }
    private static CoreSoWMats _SOWM;

    public CoreUltrav3 Ultra = new();
    public bool DontPreconfigure = true;
    public string OptionsStorage = "ArmyElementalCore";

    private readonly (string map, string monsterName, string item, int quantity)[] Stages =
    {
        ("manacradle", "Dark Tainted Mana", "Elemental Tear", 20),
        ("manacradle", "Malgor", "Weathered Armor Shard", 1),
        ("manacradle", "The Mainyu", "Licorice Scale", 1),
    };

    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Elemental Core Quantity", "Number of Elemental Cores to farm.", 500),
        new Option<int>("ArmySize", "Army Size", "How many players are in your army (including yourself). Set to 1 for solo.", 4),
        new Option<int>(
            "privateRoomNumber",
            "Private Room Number",
            "Private room number used by all army accounts. Set the same number on every account (1000-99999).",
            12345
        ),
        new Option<bool>("UseArmySync", "Enable Army Sync", "Use army sync to coordinate item progress for the Mana Cradle farm.", true),
        new Option<bool>(
            "HelpOthersWhenDone",
            "Help Others When Done",
            "If this account already has the current stage item, keep attacking to help other army members finish faster.",
            true
        ),
        new Option<bool>("EnableClassSync", "Enable Class Sync", "Auto-equip class presets before farming starts.", true),
        new Option<bool>("DoEnh", "Do Enhancements", "Apply UltraEnhancements for the equipped class before farming.", true),
        new Option<string>("Class1", "Class 1", "Preset class 1 to auto-equip before the fight. Use format: ClassName,Username.", "Chaos Avenger"),
        new Option<string>("Class2", "Class 2", "Preset class 2 to auto-equip before the fight. Use format: ClassName,Username.", "Chaos Avenger"),
        new Option<string>("Class3", "Class 3", "Preset class 3 to auto-equip before the fight. Use format: ClassName,Username.", "Chaos Avenger"),
        new Option<string>("Class4", "Class 4", "Preset class 4 to auto-equip before the fight. Use format: ClassName,Username.", "Chaos Avenger"),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quant = Bot.Config!.Get<int>("Quantity");
        int armySize = Bot.Config.Get<int>("ArmySize");
        bool useArmySync = Bot.Config.Get<bool>("UseArmySync");

        if (useArmySync && armySize > 1)
        {
            SetupPrivateRoom();
            SetupArmy(armySize, Bot.Config.Get<bool>("EnableClassSync"));
        }
        else if (useArmySync)
        {
            Core.Logger("Army sync is enabled but Army Size is 1. Set Army Size to your real army count on every account.");
        }

        if (Bot.Config.Get<bool>("DoEnh"))
            DoEnhs();

        ElementalCore(quant, useArmySync);

        Core.SetOptions(false);
    }

    public void ElementalCore(int quant = 500, bool useArmySync = false)
    {
        if (!useArmySync && Core.CheckInventory("Elemental Core", quant))
            return;

        Core.BankingBlackList.Add("Elemental Core");
        Core.AddDrop("Elemental Core");
        Core.AddDrop(Stages.Select(s => s.item).Append("Elemental Core").ToArray());
        Core.FarmingLogger("Elemental Core", quant);

        if (!useArmySync)
        {
            SOWM.ElementalCore(quant);
            return;
        }

        RetrieveElementalCores(quant);
    }

    public void RetrieveElementalCores(int quant = 500)
    {
        if (Core.CheckInventory("Elemental Core", quant))
            return;

        bool helpOthers = Bot.Config!.Get<bool>("HelpOthersWhenDone");
        string syncFile = "ArmyElementalCore.sync";
        string stageSyncFile = "ArmyElementalCore_stage.sync";
        int armySize = Bot.Config.Get<int>("ArmySize");

        
        Core.EnsureAccept(9126);


        while (!Bot.ShouldExit)
        {

            Core.EnsureAccept(9126);

            if (Core.CheckInventory("Elemental Core", quant))
            {
                if (!helpOthers || Ultra.CheckArmyProgressBool(() => Core.CheckInventory("Elemental Core", quant), syncFile))
                    break;

                Core.Logger("Army members still farming Elemental Cores. Staying in the fight to help others.");
            }

            int ownStage = GetOwnStage();
            int stageIndex = ArmyGeneral.PublishAndGetArmyStage(Ultra, Bot, ownStage, stageSyncFile, armySize);
            Core.Logger($"Own stage {ownStage}, army stage {stageIndex}");

            if (stageIndex >= Stages.Length)
            {
                Core.Logger("All stage items complete.");
                Core.EnsureAccept(9126);
                Bot.Sleep(1000);
                Bot.Map.Jump("Enter", "Spawn", autoCorrect: false);
                Bot.Wait.ForCellChange("Enter");
                Core.EnsureComplete(9126);
                Bot.Wait.ForPickup("Elemental Core");
                Ultra.CheckArmyProgress("Elemental Core", quant, false, syncFile);
                Core.FarmingLogger("Elemental Core", quant);
                continue;
            }

            var stage = Stages[stageIndex];
            string stageSyncItem = ArmyGeneral.GetSyncFileForItem("ArmyElementalCore_Item", stage.item);
            bool stageSkillsEnabled = false;

            while (!Bot.ShouldExit)
            {
                RefreshStageSync(stageSyncFile, GetOwnStage());

                int armyStageNow = ArmyGeneral.PublishAndGetArmyStage(Ultra, Bot, GetOwnStage(), stageSyncFile, armySize);
                if (armyStageNow != stageIndex)
                {
                    Core.Logger($"Army stage changed {stageIndex} -> {armyStageNow}. Re-syncing.");
                    break;
                }

                if (Ultra.CheckArmyProgressBool(() => Core.CheckInventory(stage.item, stage.quantity), stageSyncItem))
                    break;

                bool stageComplete = Core.CheckInventory(stage.item, stage.quantity);
                bool isHelper = helpOthers && stageComplete;

                if (isHelper)
                    Core.Logger($"Army helper on {stage.item}: {Bot.Inventory.GetQuantity(stage.item)}/{stage.quantity}");
                else
                    Core.Logger($"Army stage {stageIndex + 1}/{Stages.Length}: {stage.item}");

                Core.FarmingLogger(stage.item, stage.quantity);
                RunStageCombat(stage, isHelper, ref stageSkillsEnabled);
                Bot.Sleep(isHelper ? 500 : 100);
            }

            if (stageSkillsEnabled)
            {
                Engine.DisableSkills();
                stageSkillsEnabled = false;
            }

            ResetCombatOptions();
        }
    }

    private void RunStageCombat(
        (string map, string monsterName, string item, int quantity) stage,
        bool isHelper,
        ref bool stageSkillsEnabled
    )
    {
        EnsureStageSkills(ref stageSkillsEnabled);

        if (isHelper)
        {
            JoinStageMap(stage.map);

            if (!Bot.Player.Alive)
            {
                Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
                return;
            }

            Bot.Options.AggroMonsters = true;
            Bot.Options.HidePlayers = true;

            if (Bot.Player.Cell == null || Bot.Player.Cell.Equals("Enter", StringComparison.OrdinalIgnoreCase))
            {
                Engine.ChooseBestCell(stage.monsterName);
                Bot.Player.SetSpawnPoint();
                Bot.Sleep(500);
            }

            HelperAttack(stage.monsterName);
            return;
        }

        JoinStageMap(stage.map);

        if (!Bot.Player.Alive)
        {
            Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
            return;
        }

        if (Bot.Map.PlayerNames?.Any(x => x != Bot.Player.Username) == true)
        {
            Bot.Options.AggroMonsters = true;
            Bot.Options.HidePlayers = true;
        }

        Engine.ChooseBestCell(stage.monsterName);

        if (!Bot.Player.HasTarget)
            Bot.Combat.Attack(stage.monsterName);
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

    private void HelperAttack(string monsterName)
    {
        Bot.Combat.Attack(monsterName);
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
        Bot.Options.AggroMonsters = false;
        Bot.Options.HidePlayers = false;
    }

    private int GetOwnStage()
    {
        for (int i = 0; i < Stages.Length; i++)
        {
            if (!Core.CheckInventory(Stages[i].item, Stages[i].quantity))
                return i;
        }

        return Stages.Length;
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

    private void SetupArmy(int armySize, bool enableClassSync)
    {
        string readySyncFile = "ArmyElementalCore.ready.sync";
        Ultra.ClearSyncFile(Ultra.ResolveSyncPath(readySyncFile));
        Bot.Sleep(2500);
        Core.Logger($"Waiting for army ready ({armySize - 1} other players) in room #{Core.PrivateRoomNumber}...");
        Ultra.WaitForArmy(armySize - 1, readySyncFile);

        ArmyGeneral.PrepareSyncFiles(
            Ultra,
            "ArmyElementalCore_VoidAura.sync",
            Stages.Select(s => s.item),
            "ArmyElementalCore_Item"
        );
        ArmyGeneral.PrepareStageSync(Ultra, "ArmyElementalCore_stage.sync");

        if (enableClassSync)
        {
            Core.Logger("Starting army class sync...");
            ArmyGeneral.PrepareClassSync(Ultra, Bot, armySize, "ArmyElementalCore.class.sync");
        }
    }

    private void DoEnhs()
    {
        Core.Logger("Applying UltraEnhancements...");
        Enh.Apply();
    }
}
