using System;
using System.Collections.Generic;
using Ascendex.Game.Content;

namespace Ascendex.Game;

public static class TrainingSimulator
{
    public static double GetProgressRequired(double baseProgressRequired, double perLevelExponent, int level) =>
        baseProgressRequired * Math.Pow(perLevelExponent, Math.Max(0, level));

    public static double GetSpeciesProgressRequired(SpeciesBarConfig config, int level) =>
        GetProgressRequired(config.BaseProgressRequired, config.ProgressRequiredPerLevelExponent, level);

    public static double GetTrainerProgressRequired(TrainerBarConfig config, int level) =>
        GetProgressRequired(config.BaseProgressRequired, config.ProgressRequiredPerLevelExponent, level);

    public static void TickSpecies(GameSession session, SpeciesProgress progress, SpeciesBarConfig config) =>
        AdvanceSpecies(session, progress, config, 1);

    public static void AdvanceSpecies(
        GameSession session,
        SpeciesProgress progress,
        SpeciesBarConfig config,
        long tickCount)
    {
        var progressChanged = false;
        var levelChanged = false;
        while (tickCount-- > 0 && (progress.IsTraining || progress.IsCatching))
        {
            var change = TickSpeciesDeferred(session, progress, config);
            progressChanged |= change.ProgressChanged;
            levelChanged |= change.LevelChanged;
        }

        progress.PublishSimulationChanges(levelChanged, progressChanged);
    }

    internal static (bool ProgressChanged, bool LevelChanged) TickSpeciesDeferred(
        GameSession session,
        SpeciesProgress progress,
        SpeciesBarConfig config)
    {
        if (!progress.IsTraining && !progress.IsCatching)
        {
            return default;
        }

        var progressRequired = session.GetSpeciesProgressRequired(config, progress.Level);
        var progressPerTick = GetSpeciesProgressPerTick(session, progress, config);
        if (progressRequired <= 0 || progressPerTick <= 0)
        {
            return default;
        }

        var nextProgress = progress.Progress + progressPerTick;
        if (nextProgress < progressRequired)
        {
            return (progress.SetSimulationProgress(nextProgress), false);
        }

        var progressChanged = progress.SetSimulationProgress(0);
        ApplySpeciesLevelUp(session, progress, config, deferProgressNotifications: true);
        return (progressChanged, true);
    }

    public static void TickTrainer(GameSession session, TrainerProgress progress, TrainerBarConfig config) =>
        AdvanceTrainer(session, progress, config, 1);

    public static void AdvanceTrainer(
        GameSession session,
        TrainerProgress progress,
        TrainerBarConfig config,
        long tickCount)
    {
        var progressChanged = false;
        var levelChanged = false;
        while (tickCount-- > 0 && progress.IsTraining)
        {
            var change = TickTrainerDeferred(session, progress, config);
            progressChanged |= change.ProgressChanged;
            levelChanged |= change.LevelChanged;
        }

        progress.PublishSimulationChanges(levelChanged, progressChanged);
    }

    internal static (bool ProgressChanged, bool LevelChanged) TickTrainerDeferred(
        GameSession session,
        TrainerProgress progress,
        TrainerBarConfig config)
    {
        if (!progress.IsTraining)
        {
            return default;
        }

        var progressRequired = session.GetTrainerProgressRequired(config, progress.Level);
        var progressPerTick = GetTrainerProgressPerTick(session, progress, config);
        if (progressRequired <= 0 || progressPerTick <= 0)
        {
            return default;
        }

        var nextProgress = progress.Progress + progressPerTick;
        if (nextProgress < progressRequired)
        {
            return (progress.SetSimulationProgress(nextProgress), false);
        }

        var progressChanged = progress.SetSimulationProgress(0);
        progress.IncrementSimulationLevel();
        session.NotifyTrainerLevelChanged(progress.TrainerId);
        return (progressChanged, true);
    }

    public static void GrantSpeciesLevelsWithTypePoints(
        GameSession session,
        SpeciesProgress progress,
        SpeciesBarConfig config,
        int targetLevel)
    {
        if (progress.Level >= targetLevel)
        {
            return;
        }

        progress.IsCatching = false;
        var levelUps = targetLevel - progress.Level;
        progress.Level = targetLevel;

        if (progress.IsVisible && levelUps > 0)
        {
            for (var i = 0; i < levelUps; i++)
            {
                RecordTypeContributionsForCurrentStage(session, progress, config);
            }
        }

        session.NotifySpeciesLevelChanged(progress.SpeciesRootName);
    }

    public static double GetSpeciesProgressPerTick(GameSession session, SpeciesProgress progress, SpeciesBarConfig config)
    {
        var speedMultiplier = progress.IsCatching
            ? session.GetPokemonCatchSpeedFromBattleClears()
            : session.GetPokemonTrainingSpeedFromBattleClears();

        var perTick = GameBalance.Training.ProgressPerTick
            * GetCatchActivityMultiplier(session, progress.IsCatching)
            * ClampMultiplier(speedMultiplier);

        if (progress.IsCatching)
        {
            perTick *= session.GetBestOwnedBallCatchMultiplier();
        }
        else
        {
            perTick *= session.GetVitaminTrainingMultiplier(progress.SpeciesRootName);
        }

        if (progress.IsCatching && config.CatchDifficultyMultiplier > 1.0)
        {
            perTick /= config.CatchDifficultyMultiplier;
        }

        return perTick;
    }

    public static double GetTrainerProgressPerTick(GameSession session, TrainerProgress progress, TrainerBarConfig config) =>
        GameBalance.Training.ProgressPerTick * ClampMultiplier(session.GetBattleSpeedFromTypeLevels());

    private static void ApplySpeciesLevelUp(
        GameSession session,
        SpeciesProgress progress,
        SpeciesBarConfig config,
        bool deferProgressNotifications)
    {
        var previousLevel = progress.Level;
        var chain = config.EvolutionChain;
        if (chain is { Length: > 0 })
        {
            var previousStageIndex = GetActiveStageIndexZeroBased(chain, progress.Level);
            if (deferProgressNotifications)
            {
                progress.IncrementSimulationLevel();
            }
            else
            {
                progress.Level++;
            }
            var newStageIndex = GetActiveStageIndexZeroBased(chain, progress.Level);
            if (newStageIndex > previousStageIndex && progress.IsVisible)
            {
                var oldPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, previousStageIndex);
                var newPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, newStageIndex);
                var oldStage = chain[previousStageIndex];
                var newStage = chain[newStageIndex];
                var remove = TypeLevelContributionRules.Negate(
                    TypeLevelContributionRules.SplitTotal(oldStage.TypeKey, oldStage.SecondaryTypeKey, progress.Level * oldPerLevel));
                var add = TypeLevelContributionRules.SplitTotal(newStage.TypeKey, newStage.SecondaryTypeKey, progress.Level * newPerLevel);
                session.RecordTypeLevelContributions([.. remove, .. add]);
            }
        }
        else
        {
            if (deferProgressNotifications)
            {
                progress.IncrementSimulationLevel();
            }
            else
            {
                progress.Level++;
            }
        }

        RecordTypeContributionsForCurrentStage(session, progress, config);

        if (previousLevel == 0 && progress.Level >= 1)
        {
            ShinyRules.ApplyFirstCatchShinyRoll(session.State, progress);
        }

        session.NotifySpeciesLevelChanged(progress.SpeciesRootName);
    }

    public static IReadOnlyDictionary<string, int> ComputeLifetimeTypeContributions(
        string speciesRootName,
        EvolutionStage[]? chain,
        int level,
        bool isVisible)
    {
        if (level < 1 || !isVisible)
        {
            return new Dictionary<string, int>();
        }

        var totals = new Dictionary<string, int>();
        var simulatedLevel = 0;
        for (var step = 0; step < level; step++)
        {
            if (chain is { Length: > 0 })
            {
                var previousStageIndex = GetActiveStageIndexZeroBased(chain, simulatedLevel);
                simulatedLevel++;
                var newStageIndex = GetActiveStageIndexZeroBased(chain, simulatedLevel);
                if (newStageIndex > previousStageIndex)
                {
                    var oldPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, previousStageIndex);
                    var newPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, newStageIndex);
                    var oldStage = chain[previousStageIndex];
                    var newStage = chain[newStageIndex];
                    ApplyContributions(
                        totals,
                        TypeLevelContributionRules.Negate(
                            TypeLevelContributionRules.SplitTotal(
                                oldStage.TypeKey,
                                oldStage.SecondaryTypeKey,
                                simulatedLevel * oldPerLevel)));
                    ApplyContributions(
                        totals,
                        TypeLevelContributionRules.SplitTotal(
                            newStage.TypeKey,
                            newStage.SecondaryTypeKey,
                            simulatedLevel * newPerLevel));
                }
            }
            else
            {
                simulatedLevel++;
            }

            ApplyContributions(totals, GetContributionsForLevel(speciesRootName, chain, simulatedLevel));
        }

        return totals;
    }

    public static void RecordTypeContributionsForCurrentStage(
        GameSession session,
        SpeciesProgress progress,
        SpeciesBarConfig config)
    {
        if (!progress.IsVisible)
        {
            return;
        }

        session.RecordTypeLevelContributions(
            GetContributionsForLevel(progress.SpeciesRootName, config.EvolutionChain, progress.Level));
    }

    private static TypeLevelContribution[] GetContributionsForLevel(
        string speciesRootName,
        EvolutionStage[]? chain,
        int level)
    {
        var chainLength = chain?.Length ?? 1;
        var stageIndex = chain is { Length: > 0 }
            ? GetActiveStageIndexZeroBased(chain, level)
            : 0;
        var totalPoints = TypeLevelUpLookup.PointsForChainStage(chainLength, stageIndex);

        string primary;
        string? secondary;
        if (TryGetResolvedEvolutionStage(chain, level, out var resolved))
        {
            primary = resolved.TypeKey;
            secondary = resolved.SecondaryTypeKey;
        }
        else
        {
            primary = KantoSpeciesCatalog.PrimaryTypeKey(speciesRootName);
            secondary = null;
        }

        return TypeLevelContributionRules.SplitTotal(primary, secondary, totalPoints);
    }

    private static void ApplyContributions(Dictionary<string, int> totals, TypeLevelContribution[] contributions)
    {
        foreach (var contribution in contributions)
        {
            totals[contribution.TypeKey] = totals.GetValueOrDefault(contribution.TypeKey) + contribution.Points;
        }
    }

    public static bool TryGetResolvedEvolutionStage(EvolutionStage[]? chain, int level, out EvolutionStage stage)
    {
        stage = default;
        if (chain is not { Length: > 0 })
        {
            return false;
        }

        stage = chain[0];
        foreach (var evolutionStage in chain)
        {
            if (level >= evolutionStage.MinLevel)
            {
                stage = evolutionStage;
            }
            else
            {
                break;
            }
        }

        return true;
    }

    public static int GetActiveStageIndexZeroBased(EvolutionStage[] chain, int level)
    {
        var idx = 0;
        for (var i = 0; i < chain.Length; i++)
        {
            if (level >= chain[i].MinLevel)
            {
                idx = i;
            }
            else
            {
                break;
            }
        }

        return idx;
    }

    private static double GetCatchActivityMultiplier(GameSession session, bool isCatching)
    {
        if (!isCatching)
        {
            return 1.0;
        }

        var multiplier = GameBalance.Routes.CatchSpeedMultiplier;
        if (session.QualifiesForFirstCatchSpeedBonus())
        {
            multiplier *= GameBalance.Routes.FirstCatchSpeedMultiplier;
        }

        return multiplier;
    }

    private static double ClampMultiplier(double raw)
    {
        if (double.IsNaN(raw) || double.IsInfinity(raw))
        {
            return GameBalance.Training.NeutralSpeedMultiplier;
        }

        return Math.Clamp(
            raw,
            GameBalance.Training.MinExternalSpeedMultiplier,
            GameBalance.Training.MaxExternalSpeedMultiplier);
    }
}
