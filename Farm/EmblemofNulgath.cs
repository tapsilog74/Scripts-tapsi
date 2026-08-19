/*
name: Emblem of Nulgath
description: Farms Emblems of Nulgath. Set the desired quantity in options.
tags: emblem, nulgath, nation, shadowblast, tapsi
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/Nation/CoreNation.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class EmblemofNulgathTapsi
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    private static CoreNation Nation
    {
        get => _Nation ??= new CoreNation();
        set => _Nation = value;
    }
    private static CoreNation _Nation;

    public string OptionsStorage = "EmblemofNulgathTapsi";
    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Emblem Quantity", "Number of Emblems of Nulgath to farm.", 500),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        Nation.EmblemofNulgath(Bot.Config!.Get<int>("Quantity"));

        Core.SetOptions(false);
    }
}
