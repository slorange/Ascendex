using System;
using Ascendex.Game.Content;

namespace Ascendex.Game;

public static class ShopRules
{
    private static readonly (string ItemId, double Multiplier)[] BallTiers =
    [
        (ShopItemIds.GreatBall, GameBalance.Shop.GreatBallCatchMultiplier),
        (ShopItemIds.UltraBall, GameBalance.Shop.UltraBallCatchMultiplier),
        (ShopItemIds.DuskBall, GameBalance.Shop.DuskBallCatchMultiplier),
        (ShopItemIds.QuickBall, GameBalance.Shop.QuickBallCatchMultiplier),
        (ShopItemIds.TimerBall, GameBalance.Shop.TimerBallCatchMultiplier),
    ];

    private static readonly string[] XItemIds =
    [
        ShopItemIds.XAttack,
        ShopItemIds.XDefense,
        ShopItemIds.XSpecial,
        ShopItemIds.XSpeed,
    ];

    /// <summary>Shop tab unlocks when Cerulean / Misty becomes available.</summary>
    public static bool IsShopTabUnlocked(RunState state) =>
        ProgressionRules.IsTrainerVisible(state, TrainerIds.Misty);

    public static bool IsShopVisible(RunState state, string shopId)
    {
        foreach (var shop in KantoShopCatalog.All)
        {
            if (shop.Id == shopId)
            {
                return ProgressionRules.IsTrainerVisible(state, shop.UnlockTrainerId);
            }
        }

        return false;
    }

    public static bool IsItemUnlocked(RunState state, string itemId)
    {
        var shop = KantoShopCatalog.FindShopForItem(itemId);
        return shop is not null && IsShopVisible(state, shop.Value.Id);
    }

    public static int DollarsForTrainerClear(int trainerIndex, int clearCount)
    {
        if (clearCount <= 0 || trainerIndex < 0)
        {
            return 0;
        }

        var payout = GameBalance.Shop.DollarsPerClearBase
            * Math.Pow(GameBalance.Shop.DollarsPerClearIndexScale, trainerIndex)
            * clearCount;
        return (int)Math.Floor(payout);
    }

    public static double BestOwnedBallCatchMultiplier(RunState state)
    {
        var best = 1.0;
        foreach (var (itemId, multiplier) in BallTiers)
        {
            if (state.OwnedShopItemIds.Contains(itemId) && multiplier > best)
            {
                best = multiplier;
            }
        }

        return best;
    }

    public static double XItemBattleMultiplier(RunState state)
    {
        var multiplier = 1.0;
        foreach (var itemId in XItemIds)
        {
            if (state.OwnedShopItemIds.Contains(itemId))
            {
                multiplier *= GameBalance.Shop.XItemBattleMultiplier;
            }
        }

        return multiplier;
    }

    public static int MaxVitaminDoses(string speciesRootName) =>
        KantoShopCatalog.IsBossSpeciesRoot(speciesRootName)
            ? GameBalance.Shop.VitaminMaxDosesPerBossFamily
            : GameBalance.Shop.VitaminMaxDosesPerFamily;

    public static double VitaminTrainingMultiplier(RunState state, string speciesRootName)
    {
        if (!state.VitaminDosesBySpeciesRoot.TryGetValue(speciesRootName, out var doses) || doses <= 0)
        {
            return 1.0;
        }

        doses = Math.Min(doses, MaxVitaminDoses(speciesRootName));
        return 1.0 + doses * GameBalance.Shop.VitaminTrainingBonusPerDose;
    }

    public static bool OwnsOneTimeItem(RunState state, string itemId) =>
        state.OwnedShopItemIds.Contains(itemId);

    public static bool CanPurchase(RunState state, ShopItemDefinition item)
    {
        if (!IsItemUnlocked(state, item.Id))
        {
            return false;
        }

        if (state.Pokedollars < item.Price)
        {
            return false;
        }

        return item.Kind switch
        {
            ShopItemKind.Vitamin => true,
            ShopItemKind.Ball or ShopItemKind.XItem or ShopItemKind.EvolutionItem =>
                !OwnsOneTimeItem(state, item.Id),
            _ => false,
        };
    }

    public static bool TryPurchase(RunState state, ShopItemDefinition item)
    {
        if (!CanPurchase(state, item))
        {
            return false;
        }

        state.Pokedollars -= item.Price;

        if (item.Kind == ShopItemKind.Vitamin)
        {
            state.UnassignedVitaminCount++;
            state.VitaminApplySectionUnlocked = true;
            return true;
        }

        state.OwnedShopItemIds.Add(item.Id);
        return true;
    }

    /// <summary>Spends as many vitamins as current Pokédollars allow.</summary>
    public static int TryBuyAllVitamins(RunState state)
    {
        var item = KantoShopCatalog.FindItem(ShopItemIds.Vitamin);
        if (item is null || !IsItemUnlocked(state, item.Value.Id) || item.Value.Price <= 0)
        {
            return 0;
        }

        var count = (int)(state.Pokedollars / item.Value.Price);
        if (count <= 0)
        {
            return 0;
        }

        state.Pokedollars -= (long)count * item.Value.Price;
        state.UnassignedVitaminCount += count;
        state.VitaminApplySectionUnlocked = true;
        return count;
    }

    public static bool TryApplyVitamin(RunState state, string speciesRootName)
    {
        if (state.UnassignedVitaminCount <= 0)
        {
            return false;
        }

        if (!state.SpeciesByRoot.ContainsKey(speciesRootName))
        {
            return false;
        }

        state.VitaminDosesBySpeciesRoot.TryGetValue(speciesRootName, out var doses);
        if (doses >= MaxVitaminDoses(speciesRootName))
        {
            return false;
        }

        state.UnassignedVitaminCount--;
        state.VitaminDosesBySpeciesRoot[speciesRootName] = doses + 1;
        return true;
    }

    public static int TryApplyMaxVitamins(RunState state, string speciesRootName)
    {
        if (state.UnassignedVitaminCount <= 0 || !state.SpeciesByRoot.ContainsKey(speciesRootName))
        {
            return 0;
        }

        state.VitaminDosesBySpeciesRoot.TryGetValue(speciesRootName, out var doses);
        var count = Math.Min(state.UnassignedVitaminCount, MaxVitaminDoses(speciesRootName) - doses);
        if (count <= 0)
        {
            return 0;
        }

        state.UnassignedVitaminCount -= count;
        state.VitaminDosesBySpeciesRoot[speciesRootName] = doses + count;
        return count;
    }
}
