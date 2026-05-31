using System.Collections.Generic;
using System.Linq;
using Ascendex.Game.Content;

namespace Ascendex.Game;

public static class BattleBonusRules
{
    public static double GetCatchSpeedBonusFromTrainer(RunState state, string trainerId)
    {
        var trainerIndex = GetTrainerIndex(trainerId);
        if (trainerIndex < 0)
        {
            return 0;
        }

        if (!state.TrainersById.TryGetValue(trainerId, out var progress))
        {
            return 0;
        }

        var clears = System.Math.Max(0, progress.Level);
        var weight = GetTrainerClearWeight(trainerIndex);
        return clears * weight * GameBalance.Battles.RouteCatchFractionOfTrainingGymBonus;
    }

    public static double GetCatchSpeedBonusPerClear(string trainerId)
    {
        var trainerIndex = GetTrainerIndex(trainerId);
        if (trainerIndex < 0)
        {
            return 0;
        }

        return GetTrainerClearWeight(trainerIndex) * GameBalance.Battles.RouteCatchFractionOfTrainingGymBonus;
    }

    private static int GetTrainerIndex(string trainerId)
    {
        for (var i = 0; i < KantoTrainerCatalog.All.Length; i++)
        {
            if (KantoTrainerCatalog.All[i].Id == trainerId)
            {
                return i;
            }
        }

        return -1;
    }

    private static double GetTrainerClearWeight(int trainerIndexZeroBased)
    {
        var weights = GameBalance.Battles.RouteTrainingBonusPerClearByTrainerIndex;
        if (weights.Length == 0 || trainerIndexZeroBased < 0)
        {
            return 0;
        }

        return trainerIndexZeroBased >= weights.Length ? weights[^1] : weights[trainerIndexZeroBased];
    }
}

public static class CollectionsTooltipFormatter
{
    public static string FormatPokedexCell(string dexSpeciesName, RunState state)
    {
        if (!KantoSpeciesCatalog.TryGetRootForDexName(dexSpeciesName, out var speciesRoot))
        {
            return dexSpeciesName;
        }

        if (!state.SpeciesByRoot.TryGetValue(speciesRoot, out var progress))
        {
            return $"{dexSpeciesName}\nNot caught";
        }

        if (progress.Level < 1)
        {
            return $"{dexSpeciesName}\nNot caught";
        }

        var chain = KantoSpeciesCatalog.TryGetEvolutionChain(speciesRoot);
        var contributions = TrainingSimulator.ComputeLifetimeTypeContributions(
            speciesRoot,
            chain,
            progress.Level,
            progress.IsVisible);
        var statsLine = FormatTypeContributions(contributions);
        return $"{dexSpeciesName}\nLv. {progress.Level}\n{statsLine}";
    }

    public static string FormatBadge(BadgeDefinition badge, RunState state)
    {
        var bonus = BattleBonusRules.GetCatchSpeedBonusFromTrainer(state, badge.TrainerId);
        var perClear = BattleBonusRules.GetCatchSpeedBonusPerClear(badge.TrainerId);
        var earned = state.TrainersById.TryGetValue(badge.TrainerId, out var progress)
            && progress.Level >= GameBalance.Battles.MinTrainerLevelToRevealNextBattle;

        var header = badge.Tier switch
        {
            BadgeTier.Gym => $"{badge.DisplayName} Badge",
            BadgeTier.Champion => "Champion",
            _ => badge.DisplayName,
        };

        if (!earned)
        {
            return $"{header}\nCatch speed +{FormatMultiplierBonus(perClear)} when earned";
        }

        if (bonus <= 0)
        {
            return $"{header}\nCatch speed +{FormatMultiplierBonus(perClear)} when earned";
        }

        var clears = progress!.Level;
        var clearsLine = clears == 1 ? "1 clear" : $"{clears} clears";
        return $"{header}\nCatch speed +{FormatMultiplierBonus(bonus)} ({clearsLine})";
    }

    private static string FormatTypeContributions(IReadOnlyDictionary<string, int> contributions)
    {
        if (contributions.Count == 0)
        {
            return "Type stats: none";
        }

        var parts = contributions
            .Where(pair => pair.Value > 0)
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key)
            .Select(pair => $"{FormatTypeLabel(pair.Key)} +{pair.Value}");

        return $"Type stats: {string.Join(", ", parts)}";
    }

    private static string FormatTypeLabel(string typeKey) =>
        typeKey.Length == 0 ? typeKey : char.ToUpper(typeKey[0]) + typeKey[1..];

    private static string FormatMultiplierBonus(double bonus) =>
        bonus.ToString(bonus >= 1 ? "0.##" : "0.###") + "×";
}
