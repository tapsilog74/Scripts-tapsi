/*
name: The Encroaching Shadows v2
description: The Encroaching Shadows quest with shared private room support, helper mode, enhancement support, and account-based taunt control.
tags: The Encroaching Shadows, TheEncroachingShadows, Encroaching, Shadows, army, void aura, v2
*/

//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Army/CoreArmyLite.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreEnginev3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreUltrav3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraEnhancements.cs

using System;
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class TheEncroachingShadowsv2
{
    private static CoreAdvanced Adv
    {
        get => _Adv ??= new CoreAdvanced();
        set => _Adv = value;
    }

    private static CoreBots Core => CoreBots.Instance;
    private CoreBots C => CoreBots.Instance;
    private static CoreAdvanced _Adv;
    public IScriptInterface Bot => IScriptInterface.Instance;
    private static CoreEnginev3 Engine => _Engine ??= new CoreEnginev3();
    private static CoreEnginev3 _Engine;
    public CoreUltrav3 Ultra = new();

    private static CoreArmyLite Army
    {
        get => _Army ??= new CoreArmyLite();
        set => _Army = value;
    }

    private static CoreArmyLite _Army;

    private static UltraEnhancements Enh
    {
        get => _Enh ??= new UltraEnhancements();
        set => _Enh = value;
    }

    private static UltraEnhancements _Enh;

    private static CoreStory Story
    {
        get => _Story ??= new CoreStory();
        set => _Story = value;
    }

    private static CoreStory _Story;

    string taunterAccount;
    public string OptionsStorage = "TheEncroachingShadowsv2";
    public bool DontPreconfigure = true;

    public List<IOption> Options = new()
    {
        new Option<int>("ArmySize", "Army Size", "How many players are in your army (including yourself). Set to 1 for solo.", 4),
        new Option<int>(
            "privateRoomNumber",
            "Private Room Number",
            "Private room number used by all army accounts. Set the same number on every account (1000-99999).",
            12345
        ),
        new Option<bool>("UseArmySync", "Enable Army Sync", "Use army sync to coordinate boss progress and helper behavior.", true),
        new Option<bool>(
            "HelpOthersWhenDone",
            "Help Others When Done",
            "If this account already has the current item, keep attacking to help other army members finish faster.",
            true
        ),
        new Option<bool>("DoEnh", "Do Enhancements", "Apply UltraEnhancements for the equipped class before farming.", true),
        new Option<bool>("UsePotions", "Use Potions", "Enable potion usage before and during the fight.", true),
        new Option<int>("PotionQuantity", "Potion Quantity", "How many potions to keep stocked / use for the run.", 10),
        new Option<string>(
            "TaunterAccount",
            "Taunter Account",
            "Exact username of the account that will taunt. Leave empty to use your current account username.",
            ""
        ),
        new Option<string>("Account1", "Account 1", "Account name only for army preset 1.", ""),
        new Option<string>("Account2", "Account 2", "Account name only for army preset 2.", ""),
        new Option<string>("Account3", "Account 3", "Account name only for army preset 3.", ""),
        new Option<string>("Account4", "Account 4", "Account name only for army preset 4.", ""),
        new Option<string>("Account5", "Account 5", "Account name only for army preset 5.", ""),
        new Option<string>("Account6", "Account 6", "Account name only for army preset 6.", ""),
        CoreBots.Instance.SkipOptions,
    };

    bool IsTaunter() => string.Equals(Bot.Player.Username, taunterAccount, StringComparison.OrdinalIgnoreCase);

    public void ScriptMain(IScriptInterface bot)
    {
        C.BankingBlackList.AddRange(new[]
        {
            $"Flibbitiestgibbet's ??? Essence",
            $"Nightbane's ??? Essence",
            $"Xyfrag's ??? Essence",
        });

        if (
            Bot.Config != null
            && Bot.Config.Options.Contains(C.SkipOptions)
            && !Bot.Config.Get<bool>(C.SkipOptions)
        )
            Bot.Config.Configure();

        int armySize = Bot.Config!.Get<int>("ArmySize");
        bool useArmySync = Bot.Config.Get<bool>("UseArmySync");

        if (useArmySync && armySize > 1)
        {
            SetupPrivateRoom();
            SetupArmy(armySize);
        }
        else if (useArmySync)
        {
            C.Logger("Army sync is enabled but Army Size is 1. Set Army Size to your real army count on every account.");
        }

        taunterAccount = (Bot.Config!.Get<string>("TaunterAccount") ?? "").Trim();
        if (string.IsNullOrWhiteSpace(taunterAccount))
            taunterAccount = Bot.Player.Username ?? string.Empty;

        if (Bot.Config.Get<bool>("DoEnh"))
            DoEnhs();

        if (Bot.Config.Get<bool>("UsePotions"))
            EnsurePotions(Bot.Config.Get<int>("PotionQuantity"));

        DoEncroaching();

        C.SetOptions(false);
    }

    void DoEncroaching()
    {
        bool useArmySync = Bot.Config!.Get<bool>("UseArmySync");
        bool helpOthers = Bot.Config.Get<bool>("HelpOthersWhenDone");

        C.EnsureAcceptmultiple(8653, 9091);
        C.Logger("Also accepting [9091] to get Flib's essence while we do this in case you want to do the other army daily.");

        if (IsTaunter())
        {
            Bot.Events.ExtensionPacketReceived += Ultra.GenericChargeListener;
            Ultra.GetScrollOfEnrage();
        }

        Bot.Drops.Add("Flibbitiestgibbet's ??? Essence", "Glacial Pinion", "Hydra Eyeball", "Flibbitigiblets", "Void Essentia", "Void Energy", "Chest Plate");
        C.AddDrop(70052, 70053, 70054, 73862);

        Flib(useArmySync && helpOthers);
        IceWing(useArmySync && helpOthers);
        Hydra90(useArmySync && helpOthers);

        C.AbandonQuest(9091);
        C.EnsureComplete(8653);
        C.Logger("All players finished the farm.");
    }

    void Flib(bool helpOthers)
    {
        string map = "voidflibbi";
        string boss = "Flibbitiestgibbet";
        string syncPath = Ultra.ResolveSyncPath("TheEncroachingShadowsv2_Flib.sync");
        bool useArmySync = Bot.Config!.Get<bool>("UseArmySync");
        int armySize = Bot.Config.Get<int>("ArmySize");

        Ultra.ClearSyncFile(syncPath);
        Bot.Sleep(2500);
        C.Logger($"Players in current army: {Army.Players().Length}");

        Core.Join(map);
        if (useArmySync && armySize > 1)
            Ultra.WaitForArmy(armySize - 1, "VoidFlib.sync");

        Engine.ChooseBestCell(boss);
        Bot.Player.SetSpawnPoint();
        Bot.Sleep(1500);
        Bot.Options.AggroMonsters = true;

        while (!Bot.ShouldExit)
        {
            Bot.Drops.Add("Glacial Pinion", "Hydra Eyeball", "Flibbitigiblets", "Void Energy");
            C.AddDrop(70052, 70053, 70054);

            if (helpOthers && useArmySync && Ultra.CheckArmyProgressBool(() => C.CheckInventory(70054), syncPath))
            {
                C.JumpWait();
                C.Logger("All players finished farming \"Flibbitigiblets\".");
                break;
            }

            if (!Bot.Player.Alive)
            {
                Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
                continue;
            }

            if (!Bot.Player.HasTarget)
                Bot.Combat.Attack(boss);

            Bot.Sleep(500);
        }
    }

    void IceWing(bool helpOthers)
    {
        string map = "icewing";
        string boss = "Warlord Icewing";
        string syncPath = Ultra.ResolveSyncPath("TheEncroachingShadowsv2_IceWing.sync");
        bool useArmySync = Bot.Config!.Get<bool>("UseArmySync");
        int armySize = Bot.Config.Get<int>("ArmySize");

        Ultra.ClearSyncFile(syncPath);
        Bot.Sleep(2500);
        C.Logger($"Players in current army: {Army.Players().Length}");

        Core.Join(map);
        if (useArmySync && armySize > 1)
            Ultra.WaitForArmy(armySize - 1, "WarlordIcewing.sync");

        Engine.ChooseBestCell(boss);
        Bot.Player.SetSpawnPoint();
        Bot.Sleep(1500);
        Bot.Options.AggroMonsters = true;

        while (!Bot.ShouldExit)
        {
            if (helpOthers && useArmySync && Ultra.CheckArmyProgressBool(() => C.CheckInventory(70052), syncPath))
            {
                C.JumpWait();
                C.Logger("All players finished farming \"Glacial Pinion\".");
                break;
            }

            if (!Bot.Player.Alive)
            {
                Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
                continue;
            }

            if (!Bot.Player.HasTarget)
                Bot.Combat.Attack(boss);

            Bot.Sleep(500);
        }
    }

    void Hydra90(bool helpOthers)
    {
        const string map = "hydrachallenge";
        const string boss = "Hydra Head 90";
        string syncPath = Ultra.ResolveSyncPath("TheEncroachingShadowsv2_Hydra.sync");
        bool useArmySync = Bot.Config!.Get<bool>("UseArmySync");
        int armySize = Bot.Config.Get<int>("ArmySize");

        Bot.Drops.Add("Hydra Eyeball");
        C.AddDrop(70053);
        Ultra.ClearSyncFile(syncPath);
        Bot.Sleep(2500);

        Core.Join(map);
        if (useArmySync && armySize > 1)
            Ultra.WaitForArmy(armySize - 1, "HydraHead90.sync");

        Engine.ChooseBestCell(boss);
        Bot.Player.SetSpawnPoint();
        Engine.EnableSkills();

        while (!Bot.ShouldExit)
        {
            if (helpOthers && useArmySync && Ultra.CheckArmyProgressBool(() => C.CheckInventory(70053, 3), syncPath))
            {
                C.JumpWait();
                C.Logger("All players finished farming \"Hydra Eyeball\".");
                break;
            }

            if (!Bot.Player.Alive)
            {
                Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
                continue;
            }

            if (!Bot.Player.HasTarget)
            {
                Bot.Combat.Attack(boss);
                Bot.Wait.ForPickup("Hydra Eyeball");
            }

            Bot.Sleep(500);
        }
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
            C.Logger($"Invalid private room number '{configuredRoom}'. Generating a fallback room number.");
            Core.PrivateRoomNumber = Army.getRoomNr();
        }

        C.Logger($"Army private room set to #{Core.PrivateRoomNumber}. Use this same number on every account.");
    }

    private void SetupArmy(int armySize)
    {
        string readySyncFile = "TheEncroachingShadowsv2.ready.sync";
        Ultra.ClearSyncFile(Ultra.ResolveSyncPath(readySyncFile));
        Bot.Sleep(2500);
        C.Logger($"Waiting for army ready ({armySize - 1} other players) in room #{Core.PrivateRoomNumber}...");
        Ultra.WaitForArmy(armySize - 1, readySyncFile);
    }

    private void EnsurePotions(int potionQuantity)
    {
        if (potionQuantity <= 0)
            return;

        C.Logger($"Ensuring recommended potion stock ({potionQuantity}) before the run.");
        Ultra.UseAlchemyPotions(Ultra.GetBestTonicPotion(), Ultra.GetBestElixirPotion());
    }

    private void DoEnhs()
    {
        C.Logger("Applying UltraEnhancements...");
        Enh.Apply();
    }
}
