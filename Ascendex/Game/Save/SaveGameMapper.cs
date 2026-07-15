using System;
using System.Linq;

namespace Ascendex.Game.Save;

public static class SaveGameMapper
{
    public static SaveGameData ToSaveData(RunState state, int selectedMainTab)
    {
        return new SaveGameData
        {
            Version = SaveGameVersions.Current,
            SavedAtUtc = System.DateTimeOffset.UtcNow,
            SelectedMainTab = selectedMainTab,
            SelectedRouteId = state.SelectedRouteId,
            CeladonAlternateEeveelutionsUnlocked = state.CeladonAlternateEeveelutionsUnlocked,
            BankTimeSeconds = state.BankTimeSeconds,
            ChampionResetUnlocked = state.ChampionResetUnlocked,
            ChampionResetCount = state.ChampionResetCount,
            ExpShareCount = state.ExpShareCount,
            SpeciesTrainingOrder = state.SpeciesTrainingOrder.ToList(),
            PokedexResetCount = state.PokedexResetCount,
            ShinyCharmCount = state.ShinyCharmCount,
            LifetimeShinySpeciesRoots = state.LifetimeShinySpeciesRoots.ToList(),
            PendingGuaranteedShinies = state.PendingGuaranteedShinies,
            Pokedollars = state.Pokedollars,
            OwnedShopItemIds = state.OwnedShopItemIds.ToList(),
            UnassignedVitaminCount = state.UnassignedVitaminCount,
            VitaminApplySectionUnlocked = state.VitaminApplySectionUnlocked,
            VitaminDosesBySpeciesRoot = state.VitaminDosesBySpeciesRoot.ToDictionary(pair => pair.Key, pair => pair.Value),
            Species = state.SpeciesByRoot.ToDictionary(
                pair => pair.Key,
                pair => new SpeciesProgressData
                {
                    Level = pair.Value.Level,
                    Progress = pair.Value.Progress,
                    IsTraining = pair.Value.IsTraining,
                    IsCatching = pair.Value.IsCatching,
                    IsVisible = pair.Value.IsVisible,
                    IsShiny = pair.Value.IsShiny,
                }),
            Trainers = state.TrainersById.ToDictionary(
                pair => pair.Key,
                pair => new TrainerProgressData
                {
                    Level = pair.Value.Level,
                    Progress = pair.Value.Progress,
                    IsTraining = pair.Value.IsTraining,
                }),
            TypeCounterCounts = state.TypeCounterCounts.ToDictionary(pair => pair.Key, pair => pair.Value),
        };
    }

    public static void ApplyToRunState(RunState state, SaveGameData data)
    {
        state.SelectedRouteId = data.SelectedRouteId;
        state.CeladonAlternateEeveelutionsUnlocked = data.CeladonAlternateEeveelutionsUnlocked;
        state.BankTimeSeconds = data.BankTimeSeconds;
        state.ChampionResetUnlocked = data.ChampionResetUnlocked;
        state.ChampionResetCount = data.ChampionResetCount;
        state.ExpShareCount = data.ExpShareCount;
        state.PokedexResetCount = data.PokedexResetCount;
        state.ShinyCharmCount = data.ShinyCharmCount;
        state.PendingGuaranteedShinies = data.PendingGuaranteedShinies;
        state.Pokedollars = data.Pokedollars;
        state.UnassignedVitaminCount = data.UnassignedVitaminCount;
        state.VitaminApplySectionUnlocked = data.VitaminApplySectionUnlocked
            || data.UnassignedVitaminCount > 0
            || data.VitaminDosesBySpeciesRoot.Values.Any(doses => doses > 0);

        state.OwnedShopItemIds.Clear();
        if (data.OwnedShopItemIds is { Count: > 0 })
        {
            foreach (var itemId in data.OwnedShopItemIds)
            {
                state.OwnedShopItemIds.Add(itemId);
            }
        }

        state.VitaminDosesBySpeciesRoot.Clear();
        if (data.VitaminDosesBySpeciesRoot is { Count: > 0 })
        {
            foreach (var (speciesRoot, doses) in data.VitaminDosesBySpeciesRoot)
            {
                state.VitaminDosesBySpeciesRoot[speciesRoot] = doses;
            }
        }

        state.LifetimeShinySpeciesRoots.Clear();
        if (data.LifetimeShinySpeciesRoots is { Count: > 0 })
        {
            foreach (var speciesRoot in data.LifetimeShinySpeciesRoots)
            {
                state.LifetimeShinySpeciesRoots.Add(speciesRoot);
            }
        }

        foreach (var (speciesRoot, saved) in data.Species)
        {
            if (!state.SpeciesByRoot.TryGetValue(speciesRoot, out var progress))
            {
                continue;
            }

            progress.Level = saved.Level;
            progress.Progress = saved.Progress;
            progress.IsTraining = saved.IsTraining;
            progress.IsCatching = saved.IsCatching;
            progress.IsVisible = saved.IsVisible;
            progress.IsShiny = saved.IsShiny;
        }

        foreach (var (trainerId, saved) in data.Trainers)
        {
            if (!state.TrainersById.TryGetValue(trainerId, out var progress))
            {
                continue;
            }

            progress.Level = saved.Level;
            progress.Progress = saved.Progress;
            progress.IsTraining = saved.IsTraining;
        }

        foreach (var (typeKey, count) in data.TypeCounterCounts)
        {
            if (state.TypeCounterCounts.ContainsKey(typeKey))
            {
                state.TypeCounterCounts[typeKey] = count;
            }
        }

        state.SpeciesTrainingOrder.Clear();
        if (data.SpeciesTrainingOrder is { Count: > 0 })
        {
            foreach (var speciesRoot in data.SpeciesTrainingOrder)
            {
                if (!state.SpeciesTrainingOrder.Contains(speciesRoot))
                {
                    state.SpeciesTrainingOrder.Add(speciesRoot);
                }
            }
        }

        SyncSpeciesTrainingOrder(state);
    }

    private static void SyncSpeciesTrainingOrder(RunState state)
    {
        for (var i = state.SpeciesTrainingOrder.Count - 1; i >= 0; i--)
        {
            var speciesRoot = state.SpeciesTrainingOrder[i];
            if (!state.SpeciesByRoot.TryGetValue(speciesRoot, out var progress) || !progress.IsTraining)
            {
                state.SpeciesTrainingOrder.RemoveAt(i);
            }
        }

        foreach (var pair in state.SpeciesByRoot.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (pair.Value.IsTraining && !state.SpeciesTrainingOrder.Contains(pair.Key))
            {
                state.SpeciesTrainingOrder.Add(pair.Key);
            }
        }

        var maxConcurrent = 1 + state.ExpShareCount;
        while (state.SpeciesTrainingOrder.Count > maxConcurrent)
        {
            var oldest = state.SpeciesTrainingOrder[0];
            state.SpeciesTrainingOrder.RemoveAt(0);
            state.SpeciesByRoot[oldest].IsTraining = false;
        }
    }
}
