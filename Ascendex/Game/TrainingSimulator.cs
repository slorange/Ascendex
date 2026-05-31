using System;
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

    public static void TickSpecies(GameSession session, SpeciesProgress progress, SpeciesBarConfig config)
    {
        if (!progress.IsTraining && !progress.IsCatching)
        {
            return;
        }

        var progressRequired = GetSpeciesProgressRequired(config, progress.Level);
        if (progressRequired <= 0)
        {
            return;
        }

        progress.Progress += GetSpeciesProgressPerTick(session, progress, config);
        if (progress.Progress < progressRequired)
        {
            return;
        }

        progress.Progress = 0;
        ApplySpeciesLevelUp(session, progress, config);
    }

    public static void TickTrainer(GameSession session, TrainerProgress progress, TrainerBarConfig config)
    {
        if (!progress.IsTraining)
        {
            return;
        }

        var progressRequired = GetTrainerProgressRequired(config, progress.Level);
        if (progressRequired <= 0)
        {
            return;
        }

        progress.Progress += GetTrainerProgressPerTick(session, progress, config);
        if (progress.Progress < progressRequired)
        {
            return;
        }

        progress.Progress = 0;
        progress.Level++;
        session.NotifyTrainerLevelChanged();
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

        var perTick = ViewModels.GameBalance.Training.ProgressPerTick
            * GetCatchActivityMultiplier(session, progress.IsCatching)
            * ClampMultiplier(speedMultiplier);

        if (progress.IsCatching && config.CatchDifficultyMultiplier > 1.0)
        {
            perTick /= config.CatchDifficultyMultiplier;
        }

        return perTick;
    }

    public static double GetTrainerProgressPerTick(GameSession session, TrainerProgress progress, TrainerBarConfig config) =>
        ViewModels.GameBalance.Training.ProgressPerTick * ClampMultiplier(session.GetBattleSpeedFromTypeLevels());

    private static void ApplySpeciesLevelUp(GameSession session, SpeciesProgress progress, SpeciesBarConfig config)
    {
        var chain = config.EvolutionChain;
        if (chain is { Length: > 0 })
        {
            var previousStageIndex = GetActiveStageIndexZeroBased(chain, progress.Level);
            progress.Level++;
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
            progress.Level++;
        }

        RecordTypeContributionsForCurrentStage(session, progress, config);
        session.NotifySpeciesLevelChanged(progress.SpeciesRootName);
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

        var chain = config.EvolutionChain;
        var chainLength = chain?.Length ?? 1;
        var stageIndex = chain is { Length: > 0 }
            ? GetActiveStageIndexZeroBased(chain, progress.Level)
            : 0;
        var totalPoints = TypeLevelUpLookup.PointsForChainStage(chainLength, stageIndex);

        string primary;
        string? secondary;
        if (TryGetResolvedEvolutionStage(chain, progress.Level, out var resolved))
        {
            primary = resolved.TypeKey;
            secondary = resolved.SecondaryTypeKey;
        }
        else
        {
            primary = KantoSpeciesCatalog.PrimaryTypeKey(progress.SpeciesRootName);
            secondary = null;
        }

        session.RecordTypeLevelContributions(TypeLevelContributionRules.SplitTotal(primary, secondary, totalPoints));
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

        var multiplier = ViewModels.GameBalance.Routes.CatchSpeedMultiplier;
        if (session.QualifiesForFirstCatchSpeedBonus())
        {
            multiplier *= ViewModels.GameBalance.Routes.FirstCatchSpeedMultiplier;
        }

        return multiplier;
    }

    private static double ClampMultiplier(double raw)
    {
        if (double.IsNaN(raw) || double.IsInfinity(raw))
        {
            return ViewModels.GameBalance.Training.NeutralSpeedMultiplier;
        }

        return Math.Clamp(
            raw,
            ViewModels.GameBalance.Training.MinExternalSpeedMultiplier,
            ViewModels.GameBalance.Training.MaxExternalSpeedMultiplier);
    }
}
