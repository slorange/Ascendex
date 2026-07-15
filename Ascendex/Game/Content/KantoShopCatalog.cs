using System;
using System.Collections.Generic;

namespace Ascendex.Game.Content;

public enum ShopItemKind
{
    Ball,
    XItem,
    EvolutionItem,
    Vitamin,
}

public readonly record struct ShopItemDefinition(
    string Id,
    string DisplayName,
    ShopItemKind Kind,
    int Price,
    string Description);

public readonly record struct ShopLocationDefinition(
    string Id,
    string DisplayName,
    string UnlockTrainerId,
    ShopItemDefinition[] Items);

/// <summary>Kanto city / league shops. Unlock trainer matches gym / Lorelei gate for Indigo.</summary>
public static class KantoShopCatalog
{
    public static readonly ShopLocationDefinition[] All =
    [
        new(ShopIds.Pewter, "Pewter", TrainerIds.Brock, []),
        new(ShopIds.Cerulean, "Cerulean", TrainerIds.Misty,
        [
            Ball(ShopItemIds.GreatBall, "Great Balls", GameBalance.Shop.PriceGreatBall, "Catch speed ×1.5."),
        ]),
        new(ShopIds.Vermilion, "Vermilion", TrainerIds.LtSurge,
        [
            XItem(ShopItemIds.XAttack, "X Attack", GameBalance.Shop.PriceXAttack),
        ]),
        new(ShopIds.Celadon, "Celadon", TrainerIds.Erika,
        [
            Ball(ShopItemIds.UltraBall, "Ultra Balls", GameBalance.Shop.PriceUltraBall, "Catch speed ×2."),
            XItem(ShopItemIds.XDefense, "X Defense", GameBalance.Shop.PriceXDefense),
            Evo(ShopItemIds.EvolutionStones, "Evolution Stones", GameBalance.Shop.PriceEvolutionStones, "Effect coming later."),
        ]),
        new(ShopIds.Fuchsia, "Fuchsia", TrainerIds.Koga,
        [
            Ball(ShopItemIds.DuskBall, "Dusk Balls", GameBalance.Shop.PriceDuskBall, "Catch speed ×3."),
        ]),
        new(ShopIds.Saffron, "Saffron", TrainerIds.Sabrina,
        [
            XItem(ShopItemIds.XSpecial, "X Special", GameBalance.Shop.PriceXSpecial),
            Evo(ShopItemIds.LinkCable, "Link Cable", GameBalance.Shop.PriceLinkCable, "Effect coming later."),
        ]),
        new(ShopIds.Cinnabar, "Cinnabar", TrainerIds.Blaine,
        [
            Ball(ShopItemIds.QuickBall, "Quick Balls", GameBalance.Shop.PriceQuickBall, "Catch speed ×4."),
            XItem(ShopItemIds.XSpeed, "X Speed", GameBalance.Shop.PriceXSpeed),
        ]),
        new(ShopIds.Viridian, "Viridian", TrainerIds.Giovanni,
        [
            Ball(ShopItemIds.TimerBall, "Timer Balls", GameBalance.Shop.PriceTimerBall, "Catch speed ×5."),
        ]),
        new(ShopIds.IndigoPlateau, "Indigo Plateau", TrainerIds.Lorelei,
        [
            new(ShopItemIds.Vitamin, "Vitamin", ShopItemKind.Vitamin, GameBalance.Shop.PriceVitamin,
                "Consumable. +5% train speed per dose on one species family (persists across prestige)."),
        ]),
    ];

    /// <summary>All sellable items in unlock / catalog order (flat shop UI).</summary>
    public static IEnumerable<(ShopLocationDefinition Shop, ShopItemDefinition Item)> EnumerateItems()
    {
        foreach (var shop in All)
        {
            foreach (var item in shop.Items)
            {
                yield return (shop, item);
            }
        }
    }

    public static ShopItemDefinition? FindItem(string itemId)
    {
        foreach (var (_, item) in EnumerateItems())
        {
            if (item.Id == itemId)
            {
                return item;
            }
        }

        return null;
    }

    public static ShopLocationDefinition? FindShopForItem(string itemId)
    {
        foreach (var (shop, item) in EnumerateItems())
        {
            if (item.Id == itemId)
            {
                return shop;
            }
        }

        return null;
    }

    public static bool IsBossSpeciesRoot(string speciesRootName)
    {
        foreach (var route in KantoRouteCatalog.All)
        {
            foreach (var spawn in route.Spawns)
            {
                if (spawn.SpeciesRootName == speciesRootName)
                {
                    return spawn.IsBoss;
                }
            }
        }

        return false;
    }

    /// <summary>Species roots in route order; non-bosses first, then bosses (also route order).</summary>
    public static IEnumerable<string> SpeciesRootsInVitaminApplyOrder()
    {
        var normals = new List<string>();
        var bosses = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var route in KantoRouteCatalog.All)
        {
            foreach (var spawn in route.Spawns)
            {
                if (!seen.Add(spawn.SpeciesRootName))
                {
                    continue;
                }

                if (spawn.IsBoss)
                {
                    bosses.Add(spawn.SpeciesRootName);
                }
                else
                {
                    normals.Add(spawn.SpeciesRootName);
                }
            }
        }

        foreach (var root in normals)
        {
            yield return root;
        }

        foreach (var root in bosses)
        {
            yield return root;
        }
    }

    private static ShopItemDefinition Ball(string id, string name, int price, string description) =>
        new(id, name, ShopItemKind.Ball, price, description);

    private static ShopItemDefinition XItem(string id, string name, int price) =>
        new(id, name, ShopItemKind.XItem, price, "Battle speed ×1.5.");

    private static ShopItemDefinition Evo(string id, string name, int price, string description) =>
        new(id, name, ShopItemKind.EvolutionItem, price, description);
}
