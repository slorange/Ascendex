namespace Ascendex.ViewModels;

/// <summary>
/// Presentation-only constants (chrome, labels, timer text). Game pacing and economy live in <see cref="Ascendex.Game.GameBalance"/>.
/// </summary>
public static class MagicNumbersUI
{
    public static class Tabs
    {
        /// <summary>Bottom bar tab (Routes / Battles): fill when that tab is selected.</summary>
        public const string MainTabSelectedBackground = "#323841";

        /// <summary>Bottom bar tab: fill when that tab is not selected.</summary>
        public const string MainTabUnselectedBackground = "#22252C";
    }

    public static class TrainingBar
    {
        /// <summary>Bar outline when not actively training.</summary>
        public const double IdleOutlineThickness = 1;

        /// <summary>Bar outline when this row is the one training.</summary>
        public const double ActiveOutlineThickness = 4;
    }

    public static class TimeRemaining
    {
        /// <summary>Time-remaining text uses m:ss at or above this many seconds; below shows "Ns".</summary>
        public const int SecondsBeforeMinuteTimeFormat = 60;

        /// <summary>If a full bar at the current tick rate would finish in less than this, the bar renders full while training and the timer shows zero (actual progress and level-ups unchanged).</summary>
        public const double UltraFastFullBarMaxDurationSeconds = 0.1;
    }

    public static class SpeedBoost
    {
        /// <summary>Bank-time banner when actively consuming at 3×.</summary>
        public const string ActiveBackground = "#2A4A2E";

        public const string ActiveForeground = "#8FFF9A";

        /// <summary>Bank-time banner when bank is stored but no bar is active.</summary>
        public const string IdleBackground = "#2E333B";

        public const string IdleForeground = "#B8C0CC";
    }
}
