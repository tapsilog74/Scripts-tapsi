/*
name: VoidPaladinArmor
description: Farms only the Void Paladin item from DeeperandDeeperintoDarkness quest
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Nation/CoreNation.cs
//cs_include Scripts/Evil/NSoD/CoreNSOD.cs
using Skua.Core.Interfaces;

public class VoidPaladinArmor
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
    private static CoreNSOD NSoD
    {
        get => _NSoD ??= new CoreNSOD();
        set => _NSoD = value;
    }
    private static CoreNSOD _NSoD;
    private static CoreAdvanced Adv
    {
        get => _Adv ??= new CoreAdvanced();
        set => _Adv = value;
    }
    private static CoreAdvanced _Adv;

    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.AddRange(Nation.bagDrops);
        Core.BankingBlackList.Add("Void Paladin");
        Core.SetOptions();

        DeeperandDeeperintoDarkness();

        Core.SetOptions(false);
    }

    public void DeeperandDeeperintoDarkness()
    {
        if (Core.CheckInventory("Void Paladin"))
            return;

        Core.AddDrop("Void Paladin");
        Core.AddDrop(Nation.bagDrops);
        Core.Logger("Farming Void Paladin.");

        int i = 1;
        while (!Bot.ShouldExit && !Core.CheckInventory("Void Paladin"))
        {
            Core.EnsureAccept(5827);

            Nation.FarmDiamondofNulgath(25);
            Nation.FarmTaintedGem(40);
            Nation.FarmVoucher(false);
            Nation.FarmGemofNulgath(25);
            Nation.FarmDarkCrystalShard(40);
            Nation.FarmUni13(2);

            if (!Core.CheckInventory("Nulgath Shaped Chocolate"))
                //Nulgath Shaped Chocolate
                Adv.BuyItem("citadel", 44, 38316, shopItemID: 22367);
            NSoD.VoidAuras(2);

            Core.EnsureComplete(5827);
            Bot.Drops.Pickup("Void Paladin");
            Bot.Wait.ForPickup("Void Paladin");
            Core.Logger($"Completed x{i++}");
        }
    }
}
