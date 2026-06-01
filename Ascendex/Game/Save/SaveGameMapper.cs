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
            PokedexResetCount = state.PokedexResetCount,
            ShinyCharmCount = state.ShinyCharmCount,
            Species = state.SpeciesByRoot.ToDictionary(
                pair => pair.Key,
                pair => new SpeciesProgressData
                {
                    Level = pair.Value.Level,
                    Progress = pair.Value.Progress,
                    IsTraining = pair.Value.IsTraining,
                    IsCatching = pair.Value.IsCatching,
                    IsVisible = pair.Value.IsVisible,
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
    }
}
