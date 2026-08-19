/*
name: Bone Dust
description: This script will farm Bone Dust. Set how many to farm in options.
tags: bone dust, battleunderb, tapsi
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class BoneDustTapsi
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;

    private static CoreFarms Farm
    {
        get => _Farm ??= new CoreFarms();
        set => _Farm = value;
    }
    private static CoreFarms _Farm;

    public bool DontPreconfigure = true;
    public string OptionsStorage = "BoneDustTapsi";
    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Bone Dust Quantity", "Number of Bone Dust to farm.", 1000),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quant = Bot.Config!.Get<int>("Quantity");
        Core.AddDrop("Bone Dust", "Undead Essence", "Undead Energy");
        Farm.BattleUnderB("Bone Dust", quant);

        Core.SetOptions(false);
    }
}
