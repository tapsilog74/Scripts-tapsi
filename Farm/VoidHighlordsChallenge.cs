/*
name: (VHL) Challenge Quest (Quantity)
description: This will farm all the requirements for Roentgenium of Nulgath Token.
tags: farm, quest, nation, VHL, void, highlord, roentgenium, void crystal a, void crystal b, challenge, quantity
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/Nation/CoreNation.cs
//cs_include Scripts/Nation/AssistingCragAndBamboozle[Mem].cs
//cs_include Scripts/Nation/VHL/CoreVHL.cs
using System.Collections.Generic;
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class VoidHighlordsChallengeTapsi
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    private static CoreVHL VHL
    {
        get => _VHL ??= new CoreVHL();
        set => _VHL = value;
    }
    private static CoreVHL _VHL;
    public static CoreVHL sVHL
    {
        get => _sVHL ??= new CoreVHL();
        set => _sVHL = value;
    }
    public static CoreVHL _sVHL;

    private static CoreNation Nation
    {
        get => _Nation ??= new CoreNation();
        set => _Nation = value;
    }
    private static CoreNation _Nation;

    public string OptionsStorage = "VoidHighlordsChallengeTapsi";
    public bool DontPreconfigure = true;
    public List<IOption> Options = new()
    {
        new Option<int>("Quantity", "VHL Challenge Quantity", "Number of Roentgenium of Nulgath Tokens to farm.", 25),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.AddRange(Nation.bagDrops);
        Core.SetOptions();

        int quantity = Bot.Config!.Get<int>("Quantity");
        VHL.VHLChallenge(quantity);

        Core.SetOptions(false);
    }
}
