using System;
using System.Collections.Generic;
using System.Linq;
using Ascendex.Game.Content;
using Ascendex.ViewModels;

namespace Ascendex.Game;

public sealed class GameSession
{
    private readonly Dictionary<string, SpeciesBarConfig> _speciesBarConfigs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TrainerBarConfig> _trainerBarConfigs = new(StringComparer.Ordinal);

    public RunState State { get; } = new();

    public event Action<string>? SpeciesLevelChanged;

    public event Action? TrainerLevelChanged;

    public event Action? TypeCountersChanged;

    public event Action? ProgressionChanged;

    public event Action? CeladonAlternatesUnlocked;

    public event Action? ActiveBarsChanged;

    public static GameSession CreateNew()
    {
        var session = new GameSession();
        session.InitializeState();
        return session;
    }

    public SpeciesBarConfig GetSpeciesBarConfig(string speciesRootName) => _speciesBarConfigs[speciesRootName];

    public TrainerBarConfig GetTrainerBarConfig(string trainerId) => _trainerBarConfigs[trainerId];

    public SpeciesProgress GetSpecies(string speciesRootName) => State.SpeciesByRoot[speciesRootName];

    public TrainerProgress GetTrainer(string trainerId) => State.TrainersById[trainerId];

    public int GetTypeCounterCount(string typeKey) =>
        State.TypeCounterCounts.TryGetValue(typeKey, out var count) ? count : 0;

    public void SelectRoute(string routeId) => State.SelectedRouteId = routeId;

    public void Tick()
    {
        foreach (var config in _speciesBarConfigs.Values)
        {
            var progress = GetSpecies(config.SpeciesRootName);
            if (progress.IsTraining || progress.IsCatching)
            {
                TrainingSimulator.TickSpecies(this, progress, config);
            }
        }

        foreach (var config in _trainerBarConfigs.Values)
        {
            var progress = GetTrainer(config.TrainerId);
            if (progress.IsTraining)
            {
                TrainingSimulator.TickTrainer(this, progress, config);
            }
        }
    }

    public bool HasActiveBars() =>
        State.SpeciesByRoot.Values.Any(progress => progress.IsTraining || progress.IsCatching)
        || State.TrainersById.Values.Any(progress => progress.IsTraining);

    public void ToggleSpeciesActivity(string speciesRootName, bool catchMode)
    {
        var selected = GetSpecies(speciesRootName);
        if (catchMode)
        {
            if (!selected.IsCatching && selected.Level == 0)
            {
                ClearSpeciesCatchingExcept(speciesRootName);
                selected.IsCatching = true;
            }
            else
            {
                selected.IsCatching = false;
            }

            NotifyActiveBarsChanged();
            return;
        }

        if (selected.IsTraining)
        {
            selected.IsTraining = false;
            NotifyActiveBarsChanged();
            return;
        }

        ClearSpeciesTrainingExcept(speciesRootName);
        selected.IsTraining = true;
        NotifyActiveBarsChanged();
    }

    public void ToggleTrainerActivity(string trainerId)
    {
        var selected = GetTrainer(trainerId);
        if (selected.IsTraining)
        {
            selected.IsTraining = false;
            NotifyActiveBarsChanged();
            return;
        }

        foreach (var trainer in State.TrainersById.Values)
        {
            trainer.IsTraining = trainer.TrainerId == trainerId;
        }

        NotifyActiveBarsChanged();
    }

    internal void NotifySpeciesLevelChanged(string speciesRootName)
    {
        var progress = GetSpecies(speciesRootName);
        if (progress.Level >= 1 && progress.IsCatching)
        {
            progress.IsCatching = false;
            NotifyActiveBarsChanged();
        }

        TryUnlockCeladonAlternateEeveelutions(speciesRootName);
        SpeciesLevelChanged?.Invoke(speciesRootName);
        ProgressionChanged?.Invoke();
    }

    internal void NotifyTrainerLevelChanged()
    {
        TrainerLevelChanged?.Invoke();
        ProgressionChanged?.Invoke();
    }

    public void RecordTypeLevelContributions(TypeLevelContribution[] contributions)
    {
        var any = false;
        foreach (var contribution in contributions)
        {
            if (contribution.Points == 0)
            {
                continue;
            }

            any = true;
            if (!State.TypeCounterCounts.TryGetValue(contribution.TypeKey, out var count))
            {
                count = 0;
            }

            State.TypeCounterCounts[contribution.TypeKey] = count + contribution.Points;
        }

        if (any)
        {
            TypeCountersChanged?.Invoke();
        }
    }

    public void GrantSpeciesLevelsWithTypePoints(string speciesRootName, int targetLevel)
    {
        var progress = GetSpecies(speciesRootName);
        var config = GetSpeciesBarConfig(speciesRootName);
        TrainingSimulator.GrantSpeciesLevelsWithTypePoints(this, progress, config, targetLevel);
    }

    public bool QualifiesForFirstCatchSpeedBonus() =>
        !State.SpeciesByRoot.Values.Any(progress =>
            progress.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute);

    public double GetBattleSpeedFromTypeLevels()
    {
        var sum = State.TypeCounterCounts.Values.Sum();
        var multiplier = GameBalance.Battles.BattleSpeedMultiplierBaseline
            + sum * GameBalance.Battles.BattleSpeedBonusPerTotalTypeLevel;
        return Math.Min(multiplier, GameBalance.Battles.BattleSpeedMultiplierCap);
    }

    public double GetPokemonTrainingSpeedFromBattleClears() => RouteGymTrainingSpeedMultiplier;

    public double GetPokemonCatchSpeedFromBattleClears()
    {
        var training = RouteGymTrainingSpeedMultiplier;
        var baseline = GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline;
        var gymBonusAboveBaseline = Math.Max(0, training - baseline);
        return baseline + gymBonusAboveBaseline * GameBalance.Battles.RouteCatchFractionOfTrainingGymBonus;
    }

    public bool IsRouteVisible(string routeId) => ProgressionRules.IsRouteVisible(State, routeId);

    public bool IsTrainerVisible(string trainerId) => ProgressionRules.IsTrainerVisible(State, trainerId);

    public bool AnySpeciesCatching() => State.SpeciesByRoot.Values.Any(progress => progress.IsCatching);

    private double RouteGymTrainingSpeedMultiplier =>
        SpeedMultiplierFromBattleClears(
            GameBalance.Battles.RouteTrainingBonusPerClearByTrainerIndex,
            GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline,
            GameBalance.Battles.RouteTrainingSpeedMultiplierCap);

    private double SpeedMultiplierFromBattleClears(double[] weightsPerTrainer, double baseline, double cap)
    {
        var bonus = 0.0;
        var trainerIndex = 0;
        foreach (var trainer in KantoTrainerCatalog.All)
        {
            var clears = Math.Max(0, GetTrainer(trainer.Id).Level);
            bonus += clears * BonusWeightForTrainer(weightsPerTrainer, trainerIndex);
            trainerIndex++;
        }

        return Math.Min(baseline + bonus, cap);
    }

    private static double BonusWeightForTrainer(double[] weights, int trainerIndexZeroBased)
    {
        if (weights.Length == 0 || trainerIndexZeroBased < 0)
        {
            return 0;
        }

        if (trainerIndexZeroBased >= weights.Length)
        {
            return weights[^1];
        }

        return weights[trainerIndexZeroBased];
    }

    private void InitializeState()
    {
        foreach (var route in KantoRouteCatalog.All)
        {
            foreach (var spawn in route.Spawns)
            {
                if (!State.SpeciesByRoot.ContainsKey(spawn.SpeciesRootName))
                {
                    State.SpeciesByRoot[spawn.SpeciesRootName] = new SpeciesProgress
                    {
                        SpeciesRootName = spawn.SpeciesRootName,
                        IsVisible = !spawn.StartsHidden,
                    };
                }

                if (_speciesBarConfigs.ContainsKey(spawn.SpeciesRootName))
                {
                    continue;
                }

                var catchMultiplier = spawn.IsBoss ? GameBalance.Routes.BossCatchDifficultyMultiplier : 1.0;
                _speciesBarConfigs[spawn.SpeciesRootName] = new SpeciesBarConfig
                {
                    SpeciesRootName = spawn.SpeciesRootName,
                    BaseProgressRequired = GameBalance.Training.DefaultBaseProgressRequired,
                    ProgressRequiredPerLevelExponent = GameBalance.Training.RoutePokemonProgressRequiredPerLevelExponent,
                    CatchDifficultyMultiplier = catchMultiplier,
                    AllowsCatching = spawn.AllowsCatching,
                    EvolutionChain = KantoSpeciesCatalog.TryGetEvolutionChain(spawn.SpeciesRootName),
                };
            }
        }

        for (var i = 0; i < KantoTrainerCatalog.All.Length; i++)
        {
            var trainer = KantoTrainerCatalog.All[i];
            State.TrainersById[trainer.Id] = new TrainerProgress
            {
                TrainerId = trainer.Id,
            };

            var baseRequired = GameBalance.Battles.FirstTrainerBaseProgress
                * Math.Pow(GameBalance.Battles.PerTrainerDifficultyStep, i);
            _trainerBarConfigs[trainer.Id] = new TrainerBarConfig
            {
                TrainerId = trainer.Id,
                BaseProgressRequired = baseRequired,
                ProgressRequiredPerLevelExponent = GameBalance.Battles.BattleProgressRequiredPerLevelExponent,
            };
        }

        foreach (var typeKey in TypeCatalog.CounterTypeKeys)
        {
            State.TypeCounterCounts[typeKey] = 0;
        }

        State.SelectedRouteId = RouteIds.PalletTown;
    }

    private void ClearSpeciesCatchingExcept(string speciesRootName)
    {
        foreach (var progress in State.SpeciesByRoot.Values)
        {
            if (progress.SpeciesRootName != speciesRootName)
            {
                progress.IsCatching = false;
            }
        }
    }

    private void ClearSpeciesTrainingExcept(string speciesRootName)
    {
        foreach (var progress in State.SpeciesByRoot.Values)
        {
            if (progress.SpeciesRootName != speciesRootName)
            {
                progress.IsTraining = false;
            }
        }
    }

    private void NotifyActiveBarsChanged() => ActiveBarsChanged?.Invoke();

    /// <summary>Celadon: Flareon and Jolteon stay hidden until Eevee's bar reaches Vaporeon (level 25).</summary>
    private void TryUnlockCeladonAlternateEeveelutions(string speciesRootName)
    {
        if (State.CeladonAlternateEeveelutionsUnlocked || speciesRootName != "Eevee")
        {
            return;
        }

        var chain = KantoSpeciesCatalog.TryGetEvolutionChain("Eevee");
        if (chain is not { Length: >= 2 })
        {
            return;
        }

        if (GetSpecies("Eevee").Level < chain[1].MinLevel)
        {
            return;
        }

        State.CeladonAlternateEeveelutionsUnlocked = true;

        if (State.SpeciesByRoot.TryGetValue("Flareon", out var flareon))
        {
            flareon.IsVisible = true;
        }

        if (State.SpeciesByRoot.TryGetValue("Jolteon", out var jolteon))
        {
            jolteon.IsVisible = true;
        }

        CeladonAlternatesUnlocked?.Invoke();
    }
}
