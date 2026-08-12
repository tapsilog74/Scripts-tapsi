/*
name: Fishing REP v2
description: Improved version - This script will farm Fishing reputation to rank 10. Fixed quest 1682 turn-in issue.
tags: fish, fishing, rep, rank, reputation
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
using Skua.Core.Interfaces;

public class FishingREPv2
{
    public CoreBots Core => CoreBots.Instance;
    private static CoreFarms Farm
    {
        get => _Farm ??= new CoreFarms();
        set => _Farm = value;
    }
    private static CoreFarms _Farm;
    private static CoreAdvanced Adv
    {
        get => _Adv ??= new CoreAdvanced();
        set => _Adv = value;
    }
    private static CoreAdvanced _Adv;

    private const int FaithQuestId = 1682;

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        FishingREPv2Improved();

        Core.SetOptions(false);
    }

    public void FishingREPv2Improved()
    {
        int rank = 10;
        int waitTimer = 3500;
        int successful = 1;
        int failed = 1;
        int startingRep = Farm.FactionRep("Fishing");
        int currentRep = Farm.FactionRep("Fishing");
        IScriptInterface Bot = IScriptInterface.Instance;

        // Check if already at rank
        if (Farm.FactionRank("Fishing") >= rank)
        {
            Core.TrashCan("Fishing Bait", "Fishing Dynamite");
            return;
        }

        // Initialize faction if not exists
        if (!Bot.Reputation.FactionList.Exists(f => f.Name == "Fishing"))
        {
            Core.TrashCan(new[] { "Fishing Bait", "Fishing Dynamite" });
            GetBaitandDynamiteFixed(0, 1);
        }

        Core.AddDrop("Fishing Bait", "Fishing Dynamite", "Faith's Fi'shtick");
        Core.EquipClass(ClassType.Farm);
        Core.Logger($"Farming rank {rank}");

        Bot.Events.ExtensionPacketReceived += FishingWaiter;

        while (!Bot.ShouldExit && Farm.FactionRank("Fishing") < rank)
        {
            if (Core.CheckSaveState())
                Core.ExecuteSaveState();

            GetBaitandDynamiteFixed(0, 50);

            Core.Join("fishing");
            Bot.Wait.ForCellChange("Enter");
            Bot.Wait.ForTrue(() => Bot.Player.Loaded, 20);

            Core.Logger("Fishing With: Dynamite");

            while (
                !Bot.ShouldExit
                && Core.CheckInventory("Fishing Dynamite")
                && Farm.FactionRank("Fishing") < rank
            )
            {
                Core.Sleep(1000);
                Bot.Send.Packet("%xt%zm%FishCast%1%Dynamite%30%");
                Core.Logger($"CatchTimer™ Delay: {waitTimer}ms");
                Core.Sleep(waitTimer);
                Bot.Send.Packet("%xt%zm%getFish%1%false%");
                Core.Sleep(1000);

                currentRep = Farm.FactionRep("Fishing");
                Core.Logger(
                    currentRep > startingRep
                        ? $"Successful! [Dynamite Cast x{successful++}]"
                        : $"Failed! [Dynamite Cast x{failed++}]"
                );
            }
        }

        Bot.Events.ExtensionPacketReceived -= FishingWaiter;
        waitTimer = 0;
        Core.TrashCan(new[] { "Fishing Bait", "Fishing Dynamite" });
        Core.Logger("Fishing REP farming complete!");

        void CompleteFaithQuestIfReady()
        {
            if (!Bot.Quests.IsInProgress(FaithQuestId))
                return;

            if (
                !Bot.Quests.CanCompleteFullCheck(FaithQuestId)
                && !Core.CheckInventory("Faith's Fi'shtick", 1)
            )
                return;

            Core.Logger("Faith's Fi'shtick ready - turning in quest 1682 (Favor for Faith).");
            if (Core.EnsureComplete(FaithQuestId))
                Bot.Wait.ForQuestComplete(FaithQuestId);

            if (!Bot.Quests.IsInProgress(FaithQuestId))
                Core.EnsureAccept(FaithQuestId);
        }

        void GetBaitandDynamiteFixed(int fishingBaitQuant, int fishingDynamiteQuant)
        {
            if (
                Core.CheckInventory("Fishing Bait", fishingBaitQuant)
                && Core.CheckInventory("Fishing Dynamite", fishingDynamiteQuant)
            )
                return;

            Core.AddDrop("Fishing Bait", "Fishing Dynamite", "Faith's Fi'shtick");

            // Clear any stuck 1/1 state left from CoreFarms.GetBaitandDynamite abandoning the quest
            CompleteFaithQuestIfReady();

            if (!Core.isCompletedBefore(FaithQuestId))
            {
                Core.Logger("Pre-Fishing XP (required)");
                Core.EnsureAccept(FaithQuestId);
                Core.KillMonster(
                    "greenguardwest",
                    "West4",
                    "Right",
                    "Slime",
                    "Faith's Fi'shtick",
                    1,
                    log: false
                );
                CompleteFaithQuestIfReady();
            }

            Core.RegisterQuests(FaithQuestId);

            if (fishingBaitQuant > 0)
            {
                Core.FarmingLogger("Fishing Bait", fishingBaitQuant);
                while (!Bot.ShouldExit && !Core.CheckInventory("Fishing Bait", fishingBaitQuant))
                {
                    CompleteFaithQuestIfReady();
                    Core.KillMonster(
                        "greenguardwest",
                        "West3",
                        "Right",
                        "Frogzard",
                        "Fishing Bait",
                        fishingBaitQuant,
                        log: false
                    );
                }
            }

            if (fishingDynamiteQuant > 0)
            {
                Core.FarmingLogger("Fishing Dynamite", fishingDynamiteQuant);
                while (
                    !Bot.ShouldExit
                    && !Core.CheckInventory("Fishing Dynamite", fishingDynamiteQuant)
                )
                {
                    CompleteFaithQuestIfReady();
                    Core.KillMonster(
                        "greenguardwest",
                        "West4",
                        "Right",
                        "Slime",
                        "Faith's Fi'shtick",
                        1,
                        log: false
                    );
                    Bot.Wait.ForPickup("Fishing Dynamite");
                }
            }

            Core.CancelRegisteredQuests();
            Core.Logger("Returning to Fishing Map");
        }

        void FishingWaiter(dynamic packet)
        {
            var type = packet["params"].type;
            var data = packet["params"].dataObj;

            if (type is not null && type == "json")
            {
                var cmd = data.cmd.ToString();

                switch (cmd)
                {
                    case "castWait":
                        if (data.wait is not null)
                        {
                            waitTimer = data.wait;
                            Core.Logger(
                                $"Derp Moosefish: {data.derp}, Set CatchTimer™: {waitTimer}ms"
                            );
                        }
                        break;

                    case "CatchResult":
                        foreach (var c in data.catchResult)
                        {
                            if (c is null || (string)c["act"] == null || (int)c["myRep"] == 0)
                                continue;

                            switch ((string)c["act"])
                            {
                                case "Miss":
                                case "CatchPole":
                                    Core.Logger($"{(string)c["act"]}");
                                    break;
                            }

                            if ((int)c["myRep"] != 0)
                            {
                                Core.Logger($"{(int)c["myRep"]}");
                            }
                        }
                        break;
                }
            }
        }
    }
}
