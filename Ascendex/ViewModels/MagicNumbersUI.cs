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

    public static class PokedexGrid
    {
        public const int Columns = 15;

        public const int Rows = 10;

        public const double MinCellSize = 10;

        public const double HorizontalMarginTotal = 32;
    }

    public static class BadgeGrid
    {
        public const int GymColumns = 8;

        public const int LeagueColumns = 5;

        public const string UnearnedBackground = "#1A1D22";

        public const string UnearnedBorder = "#5F6470";

        public const string EarnedGymBorder = "#E8C547";

        public const string EarnedLeagueBorder = "#C0C8D8";

        public const string EarnedChampionBorder = "#FF6B8A";
    }

    public static class CollectionsDetail
    {
        public const string FlyoutBackground = "#2E333B";

        public const double MaxWidth = 280;

        public const double FontSize = 14;

        public const double Padding = 12;
    }
}
