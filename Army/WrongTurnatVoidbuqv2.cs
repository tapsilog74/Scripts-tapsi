/*
name: Wrong Turn at Voidbuquerque v2
description: Completes Wrong Turn at Voidbuquerque with Ultrasv3 army, class-sync, enhancements, and aura-aware potion support.
tags: nation, voidbuquerque, xyfrag, flibbitiestgibbet, nightbane, army, ultra, v2
*/

//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/Army/CoreArmyLite.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreEnginev3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreUltrav3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraGeneral.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraEnhancements.cs
//cs_include Scripts/Ultrasv3/ArmyDependencies/ArmyGeneral.cs
//cs_include Scripts/tapsi/Ultras/TapsiPotions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class WrongTurnatVoidbuqv2
{
    private static IScriptInterface Bot => IScriptInterface.Instance;
    private static CoreBots Core => CoreBots.Instance;
    private static CoreEnginev3 Engine => _Engine ??= new CoreEnginev3();
    private static CoreEnginev3 _Engine;
    private static CoreArmyLite Army => _Army ??= new CoreArmyLite();
    private static CoreArmyLite _Army;
    private static UltraEnhancements Enh => _Enh ??= new UltraEnhancements();
    private static UltraEnhancements _Enh;

    public CoreUltrav3 Ultra = new();
    public bool DontPreconfigure = true;
    public string OptionsStorage = "WrongTurnatVoidbuqv2";

    public List<IOption> Options = new()
    {
        new Option<int>("ArmySize", "Army Size", "Number of army accounts, including this account. Set to 1 for solo.", 4),
        new Option<bool>("EnableClassSync", "Class Sync", "Assign and equip the configured class presets across the army before farming.", true),
        new Option<bool>("UseArmySync", "Army Sync", "Synchronize readiness and quest-item completion across army accounts.", true),
        new Option<bool>("DoEnh", "Do Enh", "Apply UltraEnhancements for the equipped class before farming.", true),
        new Option<bool>("DoPotions", "Do Potions", "Stock and use recommended potions. Active potion auras are not reapplied.", true),
        new Option<int>("PotionQuantity", "Potion Quantity", "Minimum quantity of each recommended potion to stock.", 10),
        new Option<int>("PrivateRoomNumber", "Private Room Number", "Shared private room number for all army accounts (1000-99999).", 12345),
        new Option<string>("TaunterAccount", "Taunter Account", "Exact account name assigned to use Scroll of Enrage against Xyfrag.", ""),
        new Option<Rewards>("Reward", "Reward Item", "Quest reward to select upon completion.", Rewards.Tainted_Gem),
        new Option<string>("Class1", "Class 1", "Class preset 1. Format: ClassName,Username (username optional).", "ArchPaladin"),
        new Option<string>("Class2", "Class 2", "Class preset 2. Format: ClassName,Username (username optional).", "Lord of Order"),
        new Option<string>("Class3", "Class 3", "Class preset 3. Format: ClassName,Username (username optional).", "StoneCrusher"),
        new Option<string>("Class4", "Class 4", "Class preset 4. Format: ClassName,Username (username optional).", "Legion Revenant"),
        new Option<string>("Class5", "Class 5", "Optional class preset 5. Format: ClassName,Username.", ""),
        new Option<string>("Class6", "Class 6", "Optional class preset 6. Format: ClassName,Username.", ""),
        new Option<string>("Class7", "Class 7", "Optional class preset 7. Format: ClassName,Username.", ""),
        CoreBots.Instance.SkipOptions,
    };

    private string TaunterAccount => (Bot.Config!.Get<string>("TaunterAccount") ?? string.Empty).Trim();
    private bool IsTaunter => !string.IsNullOrWhiteSpace(TaunterAccount)
        && string.Equals(Bot.Player.Username, TaunterAccount, StringComparison.OrdinalIgnoreCase);

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();
        AddDrops();

        int armySize = Math.Max(1, Bot.Config!.Get<int>("ArmySize"));
        bool useArmySync = Bot.Config.Get<bool>("UseArmySync");

        if (useArmySync && armySize > 1)
            SetupArmy(armySize, Bot.Config.Get<bool>("EnableClassSync"));
        else if (Bot.Config.Get<bool>("EnableClassSync"))
            Core.Logger("Class Sync requires Army Sync with an Army Size greater than 1. Keeping the current class.");

        if (Bot.Config.Get<bool>("DoEnh"))
            Enh.Apply();

        if (Bot.Config.Get<bool>("DoPotions"))
            TapsiPotions.EnsureRecommendedPotions();

        DoDaily(useArmySync);
        Core.SetOptions(false);
    }

    private void DoDaily(bool useArmySync)
    {
        Core.EnsureAcceptmultiple(9091, 9418);
        UsePotions();
        Xyfrag(useArmySync);

        UsePotions();
        FightBoss("voidflibbi", "Flibbitiestgibbet", 73861, "Flibbitiestgibbet's ??? Essence", "WrongTurnatVoidbuqv2_Flib.sync", useArmySync);

        UsePotions();
        FightBoss("voidnightbane", "Nightbane", 73862, "Nightbane's ??? Essence", "WrongTurnatVoidbuqv2_Nightbane.sync", useArmySync);

        string rewardName = Bot.Config!.Get<string>("Reward") ?? nameof(Rewards.Tainted_Gem);
        Rewards reward = Enum.TryParse(rewardName.Replace(" ", "_"), out Rewards selectedReward)
            ? selectedReward
            : Rewards.Tainted_Gem;
        Core.EnsureComplete(9091, (int)reward);
        Core.Logger("Wrong Turn at Voidbuquerque completed.");
    }

    private void Xyfrag(bool useArmySync)
    {
        const string boss = "Xyfrag";
        const string syncFile = "WrongTurnatVoidbuqv2_Xyfrag.sync";
        PrepareFight("voidxyfrag", boss, syncFile, useArmySync);

        if (IsTaunter)
        {
            Bot.Events.ExtensionPacketReceived += Ultra.GenericChargeListener;
            Ultra.GetScrollOfEnrage();
        }

        try
        {
            while (!Bot.ShouldExit)
            {
                if (ItemFinished(73863, syncFile, useArmySync))
                    break;

                if (!Bot.Player.Alive)
                {
                    Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
                    continue;
                }

                if (IsTaunter)
                    UseEnrageWhenFocused();

                if (!Bot.Player.HasTarget)
                    Bot.Combat.Attack(boss);
                Bot.Sleep(500);
            }
        }
        finally
        {
            Bot.Events.ExtensionPacketReceived -= Ultra.GenericChargeListener;
        }
    }

    private void FightBoss(string map, string boss, int itemId, string itemName, string syncFile, bool useArmySync)
    {
        PrepareFight(map, boss, syncFile, useArmySync);
        while (!Bot.ShouldExit)
        {
            if (ItemFinished(itemId, syncFile, useArmySync))
            {
                Core.Logger($"All army members finished farming {itemName}.");
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

    private void PrepareFight(string map, string boss, string syncFile, bool useArmySync)
    {
        Ultra.ClearSyncFile(Ultra.ResolveSyncPath(syncFile));
        Core.Join(map);
        if (useArmySync && Bot.Config!.Get<int>("ArmySize") > 1)
            Ultra.WaitForArmy(Bot.Config.Get<int>("ArmySize") - 1, $"{syncFile}.ready");

        Engine.ChooseBestCell(boss);
        Bot.Player.SetSpawnPoint();
        Bot.Options.AggroMonsters = true;
        Engine.EnableSkills();
    }

    private bool ItemFinished(int itemId, string syncFile, bool useArmySync)
    {
        if (!useArmySync)
            return Core.CheckInventory(itemId);

        return Ultra.CheckArmyProgressBool(() => Core.CheckInventory(itemId), Ultra.ResolveSyncPath(syncFile));
    }

    private void UseEnrageWhenFocused()
    {
        if (!Bot.Player.HasTarget || Bot.Target.Auras.Any(a => a != null && a.Name == "Focus"))
            return;

        Engine.DisableSkills();
        while (!Bot.ShouldExit && Bot.Player.HasTarget && !Bot.Target.Auras.Any(a => a != null && a.Name == "Focus"))
        {
            if (Bot.Skills.CanUseSkill(5))
                Bot.Skills.UseSkill(5);
            Bot.Sleep(250);
        }
        Engine.EnableSkills();
    }

    private void SetupArmy(int armySize, bool enableClassSync)
    {
        Core.PrivateRooms = true;
        int room = Bot.Config!.Get<int>("PrivateRoomNumber");
        Core.PrivateRoomNumber = room is >= 1000 and <= 99999 ? room : Army.getRoomNr();

        const string readyFile = "WrongTurnatVoidbuqv2.ready.sync";
        Ultra.ClearSyncFile(Ultra.ResolveSyncPath(readyFile));
        Core.Logger($"Waiting for {armySize - 1} army members in room #{Core.PrivateRoomNumber}...");
        Ultra.WaitForArmy(armySize - 1, readyFile);

        if (enableClassSync)
            ArmyGeneral.PrepareClassSync(Ultra, Bot, armySize, "WrongTurnatVoidbuqv2.class.sync");
    }

    private void UsePotions()
    {
        if (Bot.Config!.Get<bool>("DoPotions"))
            TapsiPotions.UseRecommendedPotions();
    }

    private static void AddDrops()
    {
        Core.BankingBlackList.AddRange(new[] { "Flibbitiestgibbet's ??? Essence", "Nightbane's ??? Essence", "Xyfrag's ??? Essence" });
        Core.AddDrop(73861, 73862, 73863, 4769, 4770, 4771, 5357, 6136, 22332);
    }

    public enum Rewards
    {
        Tainted_Gem = 4769,
        Dark_Crystal_Shard = 4770,
        Diamond_of_Nulgath = 4771,
        Totem_of_Nulgath = 5357,
        Gem_of_Nulgath = 6136,
        Blood_Gem_of_the_Archfiend = 22332,
    }
}
