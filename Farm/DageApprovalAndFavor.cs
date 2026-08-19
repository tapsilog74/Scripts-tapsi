/*
name: DageApprovalAndFavor (Quantity)
description: farms DageApprovalAndFavor with custom quantity
tags: legion, dage, DageApprovalAndFavor, quantity, farm
*/

//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Legion/CoreLegion.cs

using Skua.Core.Interfaces;
using Skua.Core.Options;
using System.Collections.Generic;

public class DageApprovalAndFavor
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    private static CoreLegion CL
    {
        get => _CL ??= new CoreLegion();
        set => _CL = value;
    }
    private static CoreLegion _CL;

    public string OptionsStorage = "DageApprovalAndFavorTapsi";
    public List<IOption> Options = new()
    {
        new Option<int>("QuantApproval", "Approval Quantity", "Number of Approval to farm.", 5000),
        new Option<int>("QuantFavor", "Favor Quantity", "Number of Favor to farm.", 5000),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        int quantApproval = Bot.Config!.Get<int>("QuantApproval");
        int quantFavor = Bot.Config!.Get<int>("QuantFavor");

        GetDageApprovalAndFavor(quantApproval, quantFavor);

        Core.SetOptions(false);
    }

    public void GetDageApprovalAndFavor(int quantApproval = 5000, int quantFavor = 5000)
    {
        CL.ApprovalAndFavor(quantApproval, quantFavor);
    }
}
