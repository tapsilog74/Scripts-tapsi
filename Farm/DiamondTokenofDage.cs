/*
name: Diamond Token of Dage
description: This script will farm Diamond Tokens of Dage. Set how many to farm in options.
tags: diamond token, dage, diamond, token, lf3, legion fealty 3, lr, legion loyalty rewarded, shadowblast arena, tapsi
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Legion/CoreLegion.cs
//cs_include Scripts/CoreAdvanced.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class DiamondTokenofDageTapsi
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    private static CoreLegion Legion
    {
        get => _Legion ??= new CoreLegion();
        set => _Legion = value;
    }
    private static CoreLegion _Legion;
    private static CoreAdvanced Adv
    {
        get => _Adv ??= new CoreAdvanced();
        set => _Adv = value;
    }
    private static CoreAdvanced _Adv;

    public string OptionsStorage = "DiamondTokenofDageTapsi";
    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Diamond Token Quantity", "Number of Diamond Tokens of Dage to farm.", 30),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quant = Bot.Config!.Get<int>("Quantity");
        Legion.DiamondTokenofDage(quant);

        Core.SetOptions(false);
    }
}
