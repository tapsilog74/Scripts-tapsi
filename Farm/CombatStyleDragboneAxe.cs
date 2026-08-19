/*
name: Combat Style: Dragonbone Axe
description: Farms the Dual Dragonbone Axe of Nulgath from Combat Style: Dragonbone Axe quest (ID 629)
tags: dragonbone axe, nation, nulgath, combat style
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Nation/CoreNation.cs
using Skua.Core.Interfaces;

public class CombatStyleDragonboneAxe
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    private static CoreFarms Farm
    {
        get => _Farm ??= new CoreFarms();
        set => _Farm = value;
    }
    private static CoreFarms _Farm;
    private static CoreNation Nation
    {
        get => _Nation ??= new CoreNation();
        set => _Nation = value;
    }
    private static CoreNation _Nation;

    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.AddRange(Nation.bagDrops);
        Core.BankingBlackList.Add("Dual Dragonbone Axe of Nulgath");
        Core.SetOptions();

        FarmDragonboneAxe();

        Core.SetOptions(false);
    }

    public void FarmDragonboneAxe()
    {
        if (Core.CheckInventory("Dual Dragonbone Axe of Nulgath"))
            return;

        Core.AddDrop("Dual Dragonbone Axe of Nulgath");
        Core.AddDrop(Nation.bagDrops);
        Core.Logger("Farming Dual Dragonbone Axe of Nulgath.");

        int i = 1;
        while (!Bot.ShouldExit && !Core.CheckInventory("Dual Dragonbone Axe of Nulgath"))
        {
            Core.EnsureAccept(629);

            Nation.FarmUni13(1);
            Nation.FarmTaintedGem(13);
            Nation.FarmDarkCrystalShard(13);
            Nation.FarmDiamondofNulgath(7);
            Nation.Supplies(Nation.Uni(21));
            Core.HuntMonster("evilmarsh", "Tainted Elemental", "Tainted Rune of Evil", log: false);

            Core.EnsureComplete(629);
            Bot.Drops.Pickup("Dual Dragonbone Axe of Nulgath");
            Bot.Wait.ForPickup("Dual Dragonbone Axe of Nulgath");
            Core.Logger($"Completed x{i++}");
        }
    }
}
