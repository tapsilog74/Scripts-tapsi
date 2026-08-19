/*
name: Sparkling Deception
description: This script farms the Sparkling Deception quest loop for the requested number of Doom Extract.
tags: sparkling deception, templedelve, doomed extract, tapsi
*/
//cs_include Scripts/CoreBots.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class SparklingDeceptionTapsi
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;

    public bool DontPreconfigure = true;
    public string OptionsStorage = "SparklingDeceptionTapsi";
    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "Doomed Extract Quantity", "Number of Doomed Extract to farm.", 100),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quant = Bot.Config!.Get<int>("Quantity");
        Core.AddDrop("Doomed Extract");
        Core.RegisterQuests(9090);

        while (!Bot.ShouldExit && !Core.CheckInventory("Doomed Extract", quant))
        {
            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("templedelve", "Delirious Elemental", "Elemental Study", 6);
            Core.HuntMonster("templedelve", "Infested Nation", "Infestation Study", 6);
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("templedelve", "Doomed Fiend", "Fiend Worm");
            Bot.Wait.ForPickup("Doomed Extract");
        }

        Core.CancelRegisteredQuests();
        Core.SetOptions(false);
    }
}
