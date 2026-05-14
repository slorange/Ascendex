namespace Ascendex.ViewModels;

/// <summary>
/// All numeric knobs that affect how fast things fill, unlock, and scale. Tune here; logic lives in view models.
/// </summary>
public static class GameBalance
{
    /// <summary>Timer-driven bar fill: tick rate, per-tick progress, per-level scaling, safety clamps.</summary>
    public static class Training
    {
        /// <summary>Dispatcher timer interval. Lower feels smoother; higher saves wakeups.</summary>
        public const int TickIntervalMilliseconds = 16;

        /// <summary>Progress added each tick before any speed multiplier (party levels, battle clears, etc.).</summary>
        public const double ProgressPerTick = 1;

        /// <summary>
        /// Required progress for the next fill = <see cref="PokemonTrainingBarViewModel.BaseProgressRequired"/> × this^(Level−1).
        /// Applies to both route Pokémon and battle trainers.
        /// </summary>
        public const double ProgressRequiredPerLevelExponent = 1.2;

        /// <summary>Bar outline when not actively training.</summary>
        public const double IdleTrainingBorderThickness = 1;

        /// <summary>Bar outline when this row is the one training.</summary>
        public const double ActiveTrainingBorderThickness = 4;

        /// <summary>Floor for external speed multipliers so bad values cannot freeze the bar.</summary>
        public const double MinExternalSpeedMultiplier = 0.05;

        /// <summary>Ceiling for external speed multipliers so absurd values do not skip levels in one tick.</summary>
        public const double MaxExternalSpeedMultiplier = 50.0;

        /// <summary>Used when no speed callback is supplied, or the callback returns NaN/infinity.</summary>
        public const double NeutralSpeedMultiplier = 1.0;

        /// <summary>Time-remaining text uses m:ss at or above this many seconds; below shows "Ns".</summary>
        public const int SecondsBeforeMinuteTimeFormat = 60;
    }

    /// <summary>Route areas and wild Pokémon bar pacing.</summary>
    public static class Routes
    {
        /// <summary>Starting required progress for a new route Pokémon (before per-level exponent scaling).</summary>
        public const double DefaultBaseProgressRequired = 30;

        /// <summary>Reveal the next route when any single Pokémon in the previous area reaches at least this level.</summary>
        public const int MinPokemonLevelToUnlockNextArea = 5;
    }

    /// <summary>Gym / Elite Four battle list pacing and cross-mode bonuses.</summary>
    public static class Battles
    {
        /// <summary>Brock’s starting required progress; each later trainer multiplies by <see cref="PerTrainerDifficultyStep"/>.</summary>
        public const double FirstTrainerBaseProgress = 25000;

        /// <summary>Each trainer after the first: base × step^(order−1). Raise for a steeper difficulty curve.</summary>
        public const double PerTrainerDifficultyStep = 1.5;

        /// <summary>Battle bar speed uses: min(cap, baseline + bonusPerLevel × total party levels).</summary>
        public const double BattleSpeedMultiplierBaseline = 1.0;

        /// <summary>Battle bar speed: add this × (sum of all Pokémon levels everywhere) to the baseline.</summary>
        public const double BattleSpeedBonusPerTotalPartyLevel = 0.1;

        /// <summary>Cap on the party-level battle speed multiplier.</summary>
        public const double BattleSpeedMultiplierCap = int.MaxValue;

        /// <summary>Route training uses: min(cap, baseline + Σ (clears on trainer i × <see cref="RouteTrainingBonusPerClearByTrainerIndex"/>[i])).</summary>
        public const double RouteTrainingSpeedMultiplierBaseline = 1.0;

        /// <summary>
        /// Route training speed bonus per clear for each battle row, same order as the battles lineup (Brock, Misty, … Blue).
        /// Each trainer's clears (level − 1) add this weight to the bonus sum. Extra rows beyond this array use the last entry.
        /// </summary>
        public static readonly double[] RouteTrainingBonusPerClearByTrainerIndex =
        {
            1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
            1.0, 1.0, 1.0, 1.0, 1.0
        };

        /// <summary>Cap on the battle-clear route training multiplier.</summary>
        public const double RouteTrainingSpeedMultiplierCap = int.MaxValue;

        /// <summary>Weight applied to each clear for <paramref name="trainerIndexZeroBased"/>; out-of-range indices use the last configured weight.</summary>
        public static double RouteTrainingBonusWeightForTrainer(int trainerIndexZeroBased)
        {
            var weights = RouteTrainingBonusPerClearByTrainerIndex;
            if (weights.Length == 0)
            {
                return 0;
            }

            if (trainerIndexZeroBased < 0)
            {
                return 0;
            }

            if (trainerIndexZeroBased >= weights.Length)
            {
                return weights[^1];
            }

            return weights[trainerIndexZeroBased];
        }

        /// <summary>Unlock the next trainer row when the previous trainer’s level is at least this (2 = one full clear from level 1).</summary>
        public const int MinTrainerLevelToRevealNextBattle = 2;
    }

    /// <summary>Non-gameplay presentation that still affects how strong selected tabs read.</summary>
    public static class Ui
    {
        /// <summary>Opacity for the inactive main tab label (Routes / Battles).</summary>
        public const double InactiveMainTabLabelOpacity = 0.45;

        /// <summary>Opacity for the selected main tab label.</summary>
        public const double ActiveMainTabLabelOpacity = 1.0;
    }
}
