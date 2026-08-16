/*
name: Dark Token
description: This script will farm Dark Tokens. Set how many to farm in options.
tags: darktoken, lf3, legion fealty 3, lr, seraphic medals, seraphic, dark token, tapsi
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Legion/CoreLegion.cs
//cs_include Scripts/Story/Legion/WorldSoul.cs
//cs_include Scripts/CoreAdvanced.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class DarkTokenTapsi
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    private static CoreLegion Legion
    {
        get => _Legion ??= new CoreLegion();
        set => _Legion = value;
    }
    private static CoreLegion _Legion;
    private static WorldSoul WS
    {
        get => _WS ??= new WorldSoul();
        set => _WS = value;
    }
    private static WorldSoul _WS;
    private static CoreAdvanced Adv
    {
        get => _Adv ??= new CoreAdvanced();
        set => _Adv = value;
    }
    private static CoreAdvanced _Adv;

    public string OptionsStorage = "DarkTokenTapsi";
    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Dark Token Quantity", "Number of Dark Tokens to farm.", 100),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quant = Bot.Config!.Get<int>("Quantity");
        FarmDarkToken(quant);

        Core.SetOptions(false);
    }

    public void FarmDarkToken(int quant)
    {
        WS.WorldSoulQuests();
        Legion.DarkToken(quant);
    }
}
