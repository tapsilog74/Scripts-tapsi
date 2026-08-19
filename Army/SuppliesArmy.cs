/*
name: Supplies to Spin the Wheel of Chance (Army Hydra)
description: Uses an army in /hydrachallenge to farm Relics of Chaos from Hydra Head 90, complete Supplies to Spin the Wheel of Chance, and optionally do Swindle's Return.
tags: nation, supplies, spin the wheel, swindles return, relic of chaos, hydra, hydrachallenge, army
*/

//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Army/CoreArmyLite.cs
//cs_include Scripts/Ultrasv3/Entwined Eclipse/CoreUltra.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Models.Quests;
using Skua.Core.Options;

public class SuppliesToSpinTheWheelofChanceArmy
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private static CoreBots Core => CoreBots.Instance;
    private static CoreArmyLite Army => _Army ??= new CoreArmyLite();
    private static CoreArmyLite _Army;
    private static CoreUltra Ultra => _Ultra ??= new CoreUltra();
    private static CoreUltra _Ultra;

    public string OptionsStorage = "SuppliesToSpinTheWheelofChanceArmy";
    public bool DontPreconfigure = true;
    public List<IOption> Options = new()
    {
        new Option<SuppliesReward>("SuppliesReward", "Supplies Reward", "Select the Supplies to Spin the Wheel of Chance reward to farm.", SuppliesReward.All),
        new Option<bool>("DoSwindlesReturn", "Do Swindle's Return", "Complete Swindle's Return whenever all five required Unidentified items are available.", true),
        new Option<SwindlesReturnItem>("SwindlesReturnItem", "Swindle's Return Reward", "Select the Swindle's Return reward to farm.", SwindlesReturnItem.All),
        new Option<int>("PrivateRoomNumber", "Private Room Number", "Shared room number for every army account (1000-99999).", 12345),
        Army.player1,
        Army.player2,
        Army.player3,
        Army.player4,
        Army.player5,
        Army.player6,
        Army.packetDelay,
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int roomNumber = Bot.Config!.Get<int>("PrivateRoomNumber");
        if (roomNumber is < 1000 or > 99999)
        {
            Core.Logger("Private Room Number must be between 1000 and 99999.");
            Core.SetOptions(false);
            return;
        }

        if (Army.Players().Length < 2)
        {
            Core.Logger("Configure at least two army accounts before starting this script.");
            Core.SetOptions(false);
            return;
        }

        Core.PrivateRooms = true;
        Core.PrivateRoomNumber = roomNumber;
        Core.Logger($"Waiting for {Army.Players().Length - 1} army member(s) in room #{roomNumber}.");
        Ultra.WaitForArmy(Army.Players().Length - 1, "SuppliesToSpinTheWheelofChanceArmy.ready.sync");

        Core.AddDrop(
            "Relic of Chaos",
            "Receipt of Swindle",
            "Essence of Nulgath",
            "Hydra Scale Piece",
            "Enchanted Pearl",
            "Unidentified 1",
            "Unidentified 6",
            "Unidentified 9",
            "Unidentified 10",
            "Unidentified 16",
            "Unidentified 20"
        );
        FarmSupplies();

        Core.SetOptions(false);
    }

    private void FarmSupplies()
    {
        const int questId = 2857;
        ItemBase[] rewards = Core.EnsureLoad(questId).Rewards.Where(r => r != null && IsSuppliesReward(r.Name)).ToArray();
        string selectedReward = Bot.Config!.Get<SuppliesReward>("SuppliesReward").ToString().Replace('_', ' ');
        IEnumerable<ItemBase> targets = selectedReward == "All" ? rewards : rewards.Where(r => r.Name == selectedReward);

        Core.RegisterQuests(questId);

        foreach (ItemBase reward in targets)
        {
            Core.AddDrop(reward.Name);
            Core.FarmingLogger(reward.Name, reward.MaxStack);

            while (!Bot.ShouldExit && !Core.CheckInventory(reward.ID, reward.MaxStack))
            {
                Core.HuntMonster("hydrachallenge", "Hydra Head 90", "Relic of Chaos", 1, isTemp: false, log: false);
                DoSwindlesReturnArea();
            }
        }

        Core.CancelRegisteredQuests();
    }

    private void DoSwindlesReturnArea()
    {
        if (!Bot.Config!.Get<bool>("DoSwindlesReturn") || !Core.CheckInventory(new[] { "Unidentified 1", "Unidentified 6", "Unidentified 9", "Unidentified 16", "Unidentified 20" }))
            return;

        Quest? quest = Core.InitializeWithRetries(() => Bot.Quests.EnsureLoad(7551));
        if (quest?.Rewards == null)
            return;

        string selected = Bot.Config.Get<SwindlesReturnItem>("SwindlesReturnItem").ToString().Replace('_', ' ');
        ItemBase? reward = selected == "All"
            ? quest.Rewards.FirstOrDefault(r => r != null && !Core.CheckInventory(r.ID, r.MaxStack))
            : quest.Rewards.FirstOrDefault(r => r != null && r.Name == selected && !Core.CheckInventory(r.ID, r.MaxStack));

        if (reward == null)
            return;

        Core.AddDrop(reward.Name);
        Core.EnsureAccept(7551);
        Core.ResetQuest(7551);
        Core.DarkMakaiItem("Dark Makai Rune");

        if (Bot.Quests.CanCompleteFullCheck(7551))
        {
            Core.EnsureComplete(7551, reward.ID);
            Bot.Wait.ForQuestComplete(7551);
            Bot.Wait.ForPickup(reward.ID);
        }
    }

    private static bool IsSuppliesReward(string itemName) => itemName is
        "Tainted Gem" or "Dark Crystal Shard" or "Diamond of Nulgath" or "Voucher of Nulgath" or
        "Voucher of Nulgath (non-mem)" or "Gem of Nulgath" or "Unidentified 10" or "Essence of Nulgath";

    public enum SuppliesReward
    {
        All, Tainted_Gem, Dark_Crystal_Shard, Diamond_of_Nulgath, Voucher_of_Nulgath,
        Voucher_of_Nulgath_NonMem, Gem_of_Nulgath, Unidentified_10, Essence_of_Nulgath,
    }

    public enum SwindlesReturnItem
    {
        All, Tainted_Gem, Dark_Crystal_Shard, Diamond_of_Nulgath, Gem_of_Nulgath,
        Blood_Gem_of_the_Archfiend, Receipt_of_Swindle,
    }
}
