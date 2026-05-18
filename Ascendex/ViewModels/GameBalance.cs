using System;

namespace Ascendex.ViewModels;

/// <summary>
/// Numeric knobs for pacing, scaling, unlocks, and economy. Tune here; lookup helpers live beside this type; presentation lives in <see cref="MagicNumbersUI"/>.
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

		/// <summary>Starting required progress for a new route Pokémon (before per-level exponent scaling).</summary>
		public const double DefaultBaseProgressRequired = 20;

		/// <summary>
		/// Route Pokémon: required progress for the next fill = <see cref="PokemonTrainingBarViewModel.BaseProgressRequired"/> × this^Level (Level starts at 0).
		/// </summary>
		public const double RoutePokemonProgressRequiredPerLevelExponent = 1.1;

        /// <summary>Floor for external speed multipliers so bad values cannot freeze the bar.</summary>
        public const double MinExternalSpeedMultiplier = 0.01;

        /// <summary>Ceiling for external speed multipliers so absurd values do not skip levels in one tick.</summary>
        public const double MaxExternalSpeedMultiplier = int.MaxValue;

        /// <summary>Used when no speed callback is supplied, or the callback returns NaN/infinity.</summary>
        public const double NeutralSpeedMultiplier = 1.0;
    }

    /// <summary>How many type counter points a route Pokémon grants per level-up, by evolution chain length and active stage.</summary>
    public static class TypeLevelUp
    {
        /// <summary>No evolution chain (single form): points to its type(s) each level.</summary>
        public const int SingleFormSpeciesPointsPerLevel = 8;

        /// <summary>Two-form chain: first stage then final stage points per level.</summary>
        public const int TwoFormFirstStagePoints = 6;

        /// <summary>Two-form chain: final stage points per level.</summary>
        public const int TwoFormFinalStagePoints = 10;

        /// <summary>Three-or-more-form chain: first, middle, and late stages (third index onward uses the same value).</summary>
        public const int ThreePlusFormFirstStagePoints = 4;

        public const int ThreePlusFormMiddleStagePoints = 8;

        public const int ThreePlusFormLateStagePoints = 12;
    }

    /// <summary>Route areas and wild Pokémon bar pacing.</summary>
    public static class Routes
    {
        /// <summary>Pass a route when any Pokémon in that area reaches at least this level (1 = caught once).</summary>
        public const int MinPokemonLevelToPassRoute = 1;

        /// <summary>
        /// Catch progress per tick = <see cref="Training.ProgressPerTick"/> × this × external speed multipliers.
        /// Lower values make the first bar (level 0 → 1) feel slower than training.
        /// </summary>
        public const double CatchSpeedMultiplier = 0.05;

        /// <summary>Extra catch speed while no route Pokémon has been caught yet (multiplies <see cref="CatchSpeedMultiplier"/>).</summary>
        public const double FirstCatchSpeedMultiplier = 100.0;
    }

    /// <summary>Gym / Elite Four battle list pacing and cross-mode bonuses.</summary>
    public static class Battles
    {
        /// <summary>Brock’s starting required progress; each later trainer multiplies by <see cref="PerTrainerDifficultyStep"/>.</summary>
        public const double FirstTrainerBaseProgress = 50000;

        /// <summary>Each trainer after the first: base × step^(order−1). Raise for a steeper difficulty curve.</summary>
        public const double PerTrainerDifficultyStep = 1.5;

        /// <summary>
        /// Battle trainers: required progress for the next fill = <see cref="PokemonTrainingBarViewModel.BaseProgressRequired"/> × this^Level (Level starts at 0).
        /// </summary>
        public const double BattleProgressRequiredPerLevelExponent = 1.13;

        /// <summary>Battle bar speed uses: min(cap, baseline + bonus × total type levels from route training).</summary>
        public const double BattleSpeedMultiplierBaseline = 1.0;

        /// <summary>Battle bar speed: add this × (sum of all type counter points) to the baseline.</summary>
        public const double BattleSpeedBonusPerTotalTypeLevel = 0.1;

        /// <summary>Cap on the type-based battle speed multiplier.</summary>
        public const double BattleSpeedMultiplierCap = int.MaxValue;

        /// <summary>Route training uses: min(cap, baseline + Σ (clears on trainer i × <see cref="RouteTrainingBonusPerClearByTrainerIndex"/>[i])).</summary>
        public const double RouteTrainingSpeedMultiplierBaseline = 1.0;

        /// <summary>
        /// Route training speed bonus per clear for each battle row, same order as <see cref="GameProgression"/> trainers.
        /// Each trainer's completed cycles (bar level, starting from 0) add this weight to the bonus sum. Extra rows beyond this array use the last entry.
        /// </summary>
        public static readonly double[] RouteTrainingBonusPerClearByTrainerIndex =
        {
            1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
            1.0, 1.0, 1.0, 1.0, 1.0
        };

        /// <summary>Cap on the battle-clear route training multiplier.</summary>
        public const double RouteTrainingSpeedMultiplierCap = int.MaxValue;

        /// <summary>
        /// Route catch speed from gym clears = <see cref="RouteTrainingSpeedMultiplierBaseline"/>
        /// + (<see cref="RouteTrainingBonusPerClearByTrainerIndex"/> total − baseline) × this.
        /// 0 = gyms do not speed up catching; 1 = same gym bonus as training.
        /// </summary>
        public const double RouteCatchFractionOfTrainingGymBonus = 0.25;

        /// <summary>Unlock the next trainer row when the previous trainer’s level is at least this (1 = one full clear from starting level 0).</summary>
        public const int MinTrainerLevelToRevealNextBattle = 1;
    }
}
