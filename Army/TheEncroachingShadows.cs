/*
name: TheEncroachingShadows
description: `The Encroaching Shadows` quest with an army.
tags: The Encroaching Shadows, TheEncroachingShadows, Encroaching, Shadows, army, void aura
*/

//cs_include Scripts/Ultrasv3/Entwined Eclipse/CoreEngine.cs
//cs_include Scripts/Ultrasv3/Entwined Eclipse/CoreUltra.cs
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Army/CoreArmyLite.cs
using System;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class TheEncroachingShadows
{
    private static CoreAdvanced Adv
    {
        get => _Adv ??= new CoreAdvanced();
        set => _Adv = value;
    }
    private CoreBots C => CoreBots.Instance;
    private static CoreAdvanced _Adv;
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreEngine Core = new();
    public CoreUltra Ultra = new();
    private static CoreBots sCore
    {
        get => _sCore ??= new CoreBots();
        set => _sCore = value;
    }

    private static CoreBots _sCore;

    private static CoreArmyLite sArmy
    {
        get => _sArmy ??= new CoreArmyLite();
        set => _sArmy = value;
    }

    private static CoreArmyLite _sArmy;
    private static CoreArmyLite Army
    {
        get => _Army ??= new CoreArmyLite();
        set => _Army = value;
    }
    private static CoreArmyLite _Army;
    private static CoreStory Story
    {
        get => _Story ??= new CoreStory();
        set => _Story = value;
    }
    private static CoreStory _Story;

    string taunter;
    public string OptionsStorage = "TheEncroachingShadows";
    public bool DontPreconfigure = true;
    public List<IOption> Options = new()
    {
        new Option<string>(
            "taunter",
            "Taunter Class",
            "Insert the name of the class that will taunt",
            "Verus DoomKnight"
        ),
        sArmy.player1,
        sArmy.player2,
        sArmy.player3,
        sArmy.player4,
        sArmy.player5,
        sArmy.player6,
        sArmy.packetDelay,
        CoreBots.Instance.SkipOptions,
    };

    bool IsTaunter() => Core.HasClassEquipped(taunter);
    public void ScriptMain(IScriptInterface Bot)
    {
        C.BankingBlackList.AddRange(new[] { $"Flibbitiestgibbet's ??? Essence", $"Nightbane's ??? Essence", $"Xyfrag's ??? Essence" });

        if (
            Bot.Config != null
            && Bot.Config.Options.Contains(C.SkipOptions)
            && !Bot.Config.Get<bool>(C.SkipOptions)
        )
            Bot.Config.Configure();

        if (sArmy.Players().Length < 4)
        {
            C.Logger(
                "Players empty or less then 4 (6 reccomended), please add players to the options ( scripts botton > edit scripts option > insert account names exactly as is)"
            );
            return;
        }

        taunter = (Bot.Config!.Get<string>("taunter") ?? "").Trim();
        Core.Boot();

        DoEnchroach();

        C.SetOptions(false);
    }

    void DoEnchroach()
    {
        C.EnsureAcceptmultiple(8653);

        
        if (IsTaunter())
        {
            Bot.Events.ExtensionPacketReceived += Ultra.GenericChargeListener;
            Ultra.GetScrollOfEnrage();
        }
        Ultra.UseAlchemyPotions(Ultra.GetBestTonicPotion(), Ultra.GetBestElixirPotion());
        Bot.Drops.Add($"Flibbitiestgibbet's ??? Essence", "Glacial Pinion", "Hydra Eyeball", "Flibbitigiblets", "Void Essentia", "Void Energy", "Chest Plate");
        C.AddDrop(70052, 70053, 70054, 73862);

        FarmBoss("voidflibbi", "Flibbitiestgibbet", 70054, 1, "VoidFlib.sync", "Flibbitigiblets");
        FarmBoss("icewing", "Warlord Icewing", 70052, 1, "WarlordIcewing.sync", "Glacial Pinion");
        FarmBoss("hydrachallenge", "Hydra Head 90", 70053, 3, "HydraHead90.sync", "Hydra Eyeball");

        C.AbandonQuest(9091);
        C.EnsureComplete(8653);
        C.Logger("All players finished farm.");
    }

    void FarmBoss(string map, string boss, int itemId, int quantity, string waitSyncFile, string itemLabel)
    {
        string syncPath = Ultra.ResolveSyncPath($"EncroachingShadows_{itemId}.sync");

        Ultra.ClearSyncFile(syncPath);
        Bot.Sleep(2500);
        C.Logger($"Players in Current Army: {sArmy.Players().Length}");

        if (sArmy.Players().Length <= 1 && C.CheckInventory(itemId, quantity))
        {
            C.Logger($"Already have \"{itemLabel}\", skipping.");
            return;
        }

        bool skillsEnabled = false;
        bool combatReady = false;

        while (!Bot.ShouldExit)
        {
            if (Ultra.CheckArmyProgressBool(() => C.CheckInventory(itemId, quantity), syncPath))
            {
                C.JumpWait();
                C.Logger($"All players finished farming \"{itemLabel}\".");
                break;
            }

            bool isHelper = C.CheckInventory(itemId, quantity);

            if (isHelper)
                C.Logger($"Army helper on {itemLabel}: helping others finish.");

            RunBossCombat(map, boss, waitSyncFile, isHelper, ref skillsEnabled, ref combatReady);
            Bot.Sleep(isHelper ? 500 : 100);
        }

        if (skillsEnabled)
            Core.DisableSkills();

        ResetCombatOptions();
    }

    void RunBossCombat(
        string map,
        string boss,
        string waitSyncFile,
        bool isHelper,
        ref bool skillsEnabled,
        ref bool combatReady
    )
    {
        if (!skillsEnabled)
        {
            Core.EnableSkills();
            skillsEnabled = true;
        }

        JoinBossMap(map);

        if (!combatReady)
        {
            if (sArmy.Players().Length > 1)
                Ultra.WaitForArmy(sArmy.Players().Length - 1, waitSyncFile);

            Core.ChooseBestCell(boss);
            Bot.Player.SetSpawnPoint();
            Bot.Sleep(1500);
            combatReady = true;
        }

        Bot.Options.AggroMonsters = true;
        if (isHelper)
            Bot.Options.HidePlayers = true;

        if (!Bot.Player.Alive)
        {
            Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
            return;
        }

        if (
            Bot.Player.Cell == null
            || Bot.Player.Cell.Equals("Enter", StringComparison.OrdinalIgnoreCase)
        )
        {
            Core.ChooseBestCell(boss);
            Bot.Player.SetSpawnPoint();
            Bot.Sleep(500);
            combatReady = true;
        }

        // Always attack so support classes (e.g. Lord of Order) acquire and keep a target for skills.
        Bot.Combat.Attack(boss);
    }

    void JoinBossMap(string map)
    {
        if (string.Equals(Bot.Map.Name, map, StringComparison.OrdinalIgnoreCase))
            return;

        Core.Join(map);
        Bot.Wait.ForMapLoad(map);
    }

    void ResetCombatOptions()
    {
        Bot.Options.AttackWithoutTarget = false;
        Bot.Options.AggroAllMonsters = false;
        Bot.Options.AggroMonsters = false;
        Bot.Options.HidePlayers = false;
    }
}
