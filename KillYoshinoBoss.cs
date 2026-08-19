/*
name: KillYoshinoBoss Tapsi
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreFarms.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Options;

public class KillYoshinoBossTapsi
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
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

    public bool DontPreconfigure = true;
    public string OptionsStorage = "KillYoshinoBossTapsi";
    public List<IOption> Options = new()
    {
        new Option<int>(
            "privateRoomNumber",
            "Private Room Number",
            "Private room number to use when joining Yoshino (1000-99999).",
            12345
        ),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.SetOptions();

        int room = Bot.Config!.Get<int>("privateRoomNumber");
        if (room >= 1000 && room <= 99999)
            Core.PrivateRoomNumber = room;

        Yoshino();
        Core.SetOptions(false);
    }

    public void Yoshino()
    {
        if (!Bot.Quests.IsAvailable(5720))
            return;

        Core.EquipClass(ClassType.Solo);

        Core.AddDrop("Limited Event Coin");
        Core.EnsureAccept(5720);
        Core.KillMonster("yoshino", "r2", "Right", "*", "Limited Event Monster Proof");
        Core.JumpWait();
        Farm.ToggleBoost(BoostType.Gold);
        Core.Sleep();
        Core.EnsureComplete(5720);
        Bot.Wait.ForPickup("Limited Event Coin");
        Farm.ToggleBoost(BoostType.Gold, false);
    }
}
