/*
name: VoidSpartanArmor
description: Farms only the Void Spartan item
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Nation/CoreNation.cs
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;

public class VoidSpartanArmor
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
        Core.BankingBlackList.Add("Void Spartan");
        Core.SetOptions();

        GetSpartan("Void Spartan");

        Core.SetOptions(false);
    }

    public void GetSpartan(string item)
    {
        Core.AddDrop(
            Nation
                .bagDrops.Concat(new[] { "Void Spartan", "Zee's Red Jasper", "Fiend Cloak of Nulgath" })
                .ToArray()
        );

        Quest? QuestData = Core.InitializeWithRetries(() => Core.EnsureLoad(5982));
        if (QuestData is null)
        {
            Core.Logger("Quest 5982 not found, please report this to the devs.");
            return;
        }
        ItemBase? Item = Core.EnsureLoad(5982).Rewards.Find(x => x.Name == item);

        Core.Logger($"Farming {item}.");

        int i = 1;
        while (!Bot.ShouldExit && !Core.CheckInventory(item))
        {
            Core.EnsureAccept(5982);

            Nation.FarmUni13(1);
            Nation.FarmBloodGem(5);
            Nation.FarmGemofNulgath(10);
            Bot.Quests.UpdateQuest(4055);
            Core.HuntMonster("pyrewatch", "Flame Soldier", "Zee's Red Jasper", 1, false);
            Core.JumpWait();
            Farm.Gold(500000);
            Core.BuyItem("tercessuinotlim", 68, "Fiend Cloak of Nulgath");

            Core.EnsureComplete(5982, Item.ID);
            Bot.Wait.ForPickup(Item.Name);
            Core.Logger($"Completed {QuestData.Name}[{QuestData.ID}] x{i++}");
        }
    }
}
