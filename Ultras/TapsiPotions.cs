/*
name: null
description: Tapsi potion helper — skips re-use when aura is already active.
tags: null
*/
//cs_include Scripts/Ultrasv3/DependenciesUltras/CoreEnginev3.cs
//cs_include Scripts/Ultrasv3/DependenciesUltras/UltraPotions.cs
//cs_include Scripts/CoreBots.cs

using System;
using System.Linq;
using Skua.Core.Interfaces;

/// <summary>
/// Potion helpers for tapsi daily ultras — skips re-use when aura is already active (CoreLoneWolf behavior).
/// </summary>
public static class TapsiPotions
{
    private static IScriptInterface Bot => IScriptInterface.Instance;
    private static CoreBots Core => CoreBots.Instance;
    private static CoreEnginev3 C => CoreEnginev3.Instance;
    private static UltraPotions Pots => _Pots ??= new UltraPotions();
    private static UltraPotions _Pots;

    public static void EnsureRecommendedPotions(bool skipThird = false, string context = "")
    {
        int qty = Bot.Config?.Get<int>("PotionQuantity") ?? 10;
        Pots.EnsureRecommendedPotions(qty, skipThird, context);
    }

    public static void UseRecommendedPotions(bool skipThird = false, string context = "")
    {
        string[] potions = Pots.GetRecommendedPotions(context);

        if (potions.Length == 0)
            return;

        if (skipThird && potions.Length >= 3)
            potions = potions[..2];

        foreach (string potion in potions)
        {
            bool isCombat = IsCombatPotion(potion);
            bool auraActive = HasActivePotionAura(potion);

            if (auraActive && !isCombat)
            {
                Core.Logger($"{potion} already active.", "UsePotions");
                continue;
            }

            Core.Logger($"Equipping {potion}...");

            for (int attempt = 0; attempt < 2 && !Bot.Inventory.IsEquipped(potion); attempt++)
            {
                C.EquipConsumable(potion);
                Bot.Sleep(500);
            }

            if (HasActivePotionAura(potion))
            {
                Core.Logger($"{potion} successfully applied.");
                continue;
            }

            if (!Bot.Inventory.IsEquipped(potion))
            {
                Core.Logger($"Failed to equip {potion}.");
                Bot.Sleep(200);
                continue;
            }

            if (auraActive)
            {
                Core.Logger($"{potion} already active and equipped.", "UsePotions");
                continue;
            }

            Core.Logger($"Using {potion}...");
            Core.UsePotion();
            Bot.Sleep(500);
        }
    }

    private static string GetPotionAuraName(string itemName) =>
        itemName switch
        {
            "Sage Tonic" => "Sage",
            "Might Tonic" => "Might",
            "Fate Tonic" => "Fate",
            "Body Tonic" => "Body",
            "Wise Tonic" => "Wise",
            "Potent Honor Potion" => "Potent Honor Malice",
            "Potent Malice Potion" => "Potent Honor Malice",
            "Felicitous Philtre" => "Felicitous Philtre",
            _ => itemName,
        };

    private static bool IsCombatPotion(string itemName) =>
        itemName
            is "Potent Honor Potion"
            or "Potent Malice Potion"
            or "Potent Life Potion"
            or "Felicitous Philtre"
            or "Endurance Draught";

    private static bool HasActivePotionAura(string itemName)
    {
        string auraName = GetPotionAuraName(itemName);
        if (string.IsNullOrWhiteSpace(auraName))
            return false;

        try
        {
            return Bot.Self.HasActiveAura(auraName);
        }
        catch
        {
            return Bot?.Self?.Auras?.Any(
                a => a?.Name != null && auraName.Equals(a.Name, StringComparison.OrdinalIgnoreCase)
            ) == true;
        }
    }
}
