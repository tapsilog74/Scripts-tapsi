/*
name: Unidentified 36
description: This script will farm Unidentified 36. Set how many to farm in options.
tags: unidentified 36, fresh soul, citadel, hollowborn, tapsi
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Hollowborn/CoreHollowborn.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class Unidentified36Tapsi
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    public CoreHollowborn Hollowborn => _Hollowborn ??= new CoreHollowborn();
    private CoreHollowborn _Hollowborn;

    public bool DontPreconfigure = true;
    public string OptionsStorage = "Unidentified36Tapsi";
    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Unidentified 36 Quantity", "Number of Unidentified 36 to farm.", 100),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quantity = Bot.Config!.Get<int>("Quantity");
        Hollowborn.FreshSouls(quantity, 0);

        Core.SetOptions(false);
    }
}