/*
name: Do Ultras v3 Dailies (Tapsi)
description: Runs the first four Ultras v3 dailies with army sync — Ultra Ezrajal, Ultra Warden, Ultra Engineer, and Ultra Avatar Tyndarius.
tags: ultra, dailies, ezrajal, warden, engineer, tyndarius, tapsi, ultrasv3
*/
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreEnginev3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreUltrav3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraGeneral.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraQueue.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraWaitForArmy.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/PrerequisitesChecker.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraPotions.cs
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/tapsi/Ultras/TapsiPotions.cs
//cs_include Scripts/tapsi/Ultras/1UltraEzrajalv3.cs
//cs_include Scripts/tapsi/Ultras/2UltraWardenv3.cs
//cs_include Scripts/tapsi/Ultras/3UltraEngineerv3.cs
//cs_include Scripts/tapsi/Ultras/4UltraAvatarTyndariusv3.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class DoUltrasv3Dailies
{
    private static CoreEnginev3 Core => CoreEnginev3.Instance;
    private static CoreUltrav3 Ultra => _Ultra ??= new CoreUltrav3();
    private static CoreUltrav3 _Ultra;
    private CoreBots C => CoreBots.Instance;
    public IScriptInterface Bot => IScriptInterface.Instance;

    public bool DontPreconfigure = true;
    public string OptionsStorage = "DoUltrasv3Dailies";
    public List<IOption> Options = new()
    {
        new Option<bool>(
            "UsePrerequisitesChecker",
            "Use Prerequisites Checker",
            "Enable to run the prerequisites checker before starting ultras. Disable to skip.",
            true
        ),
        new Option<bool>("DoEnh", "Do Enhancements", "Auto-enhance gear for the fight.", true),
        new Option<int>("PotionQuantity", "Potion Quantity", "How many potions to keep stocked.", 10),
        new Option<int>(
            "privateRoomNumber",
            "Private Room Number",
            "Private room number for army map joins (1000–999999).",
            100000
        ),
        CoreBots.Instance.SkipOptions,
    };

    private const string BossParticipantSyncFile = "tapsi_ultrasv3_dailies_participants.sync";
    private const string BossSyncFile = "tapsi_ultrasv3_dailies_bosses.sync";

    private static readonly string[] DailyBosses =
    {
        "UltraEzrajal",
        "UltraWarden",
        "UltraEngineer",
        "UltraAvatarTyndarius",
    };

    public void ScriptMain(IScriptInterface bot)
    {
        C.SetOptions(true);
        ApplyPrivateRoom();
        Core.Boot();

        RunAll();

        Core.DisableSkills();
        C.SetOptions(false);
        Bot.StopSync();
    }

    private void ApplyPrivateRoom()
    {
        C.PrivateRooms = true;
        int room = Bot.Config!.Get<int>("privateRoomNumber");

        if (room >= 1000 && room <= 999999)
        {
            C.PrivateRoomNumber = room;
            C.Logger($"[DoUltrasv3Dailies] Private room set to #{room}.");
        }
        else
        {
            C.Logger(
                $"[DoUltrasv3Dailies] Invalid privateRoomNumber ({room}); using default #{C.PrivateRoomNumber}.",
                "Warning"
            );
        }
    }

    public void RunAll()
    {
        if (Bot.Config!.Get<bool>("UsePrerequisitesChecker"))
        {
            if (!new PrerequisitesChecker().PrerequisiteSyncGate(4))
                return;
        }

        int pass = 1;
        while (true)
        {
            var pending = GetSharedBossQueue(DailyBosses).ToList();
            if (!pending.Any())
                break;

            if (pass > 1)
                C.Logger(
                    $"[DoUltrasv3Dailies] Re-run pass #{pass}: {pending.Count} boss(es) still pending for some accounts.",
                    "Info"
                );

            RunBossQueue(pending);
            pass++;
        }

        C.Logger("[DoUltrasv3Dailies] All daily ultra bosses complete.");
    }

    private void RunBossQueue(IEnumerable<string> bosses)
    {
        foreach (string boss in bosses)
        {
            switch (boss)
            {
                case "UltraEzrajal":
                    new TapsiUltraEzrajalv3().RunBoss();
                    break;
                case "UltraWarden":
                    new TapsiUltraWardenv3().RunBoss();
                    break;
                case "UltraEngineer":
                    new TapsiUltraEngineerv3().RunBoss();
                    break;
                case "UltraAvatarTyndarius":
                    new TapsiUltraAvatarTyndariusv3().RunBoss();
                    break;
                default:
                    C.Logger($"Unknown Ultra boss in queue: {boss}", "Error", true, true);
                    break;
            }
        }
    }

    private IEnumerable<string> GetSharedBossQueue(IEnumerable<string> bosses)
        => UltraQueue.GetSharedBossQueue(
            Ultra,
            Bot,
            C,
            bosses,
            BossSyncFile,
            BossParticipantSyncFile,
            IsBossComplete
        );

    private bool IsBossComplete(string boss)
    {
        (int id, string name) = boss switch
        {
            "UltraEzrajal" => (8152, "Ultra Ezrajal"),
            "UltraWarden" => (8153, "Ultra Warden"),
            "UltraEngineer" => (8154, "Ultra Engineer"),
            "UltraAvatarTyndarius" => (8245, "Ultra Avatar Tyndarius"),
            _ => (0, string.Empty),
        };

        if (id == 0)
            return false;

        bool complete = UltraGeneral.IsQuestComplete(Bot, id);
        C.Logger($"{name} [{id}] complete={complete}", "Info");
        return complete;
    }
}
