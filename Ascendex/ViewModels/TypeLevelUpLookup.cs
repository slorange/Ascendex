namespace Ascendex.ViewModels;

/// <summary>
/// Maps evolution chain shape to type points per level using <see cref="GameBalance.TypeLevelUp"/> constants.
/// </summary>
public static class TypeLevelUpLookup
{
    public static int PointsForChainStage(int evolutionChainLength, int activeStageIndexZeroBased)
    {
        if (evolutionChainLength <= 1)
        {
            return GameBalance.TypeLevelUp.SingleFormSpeciesPointsPerLevel;
        }

        if (evolutionChainLength == 2)
        {
            return activeStageIndexZeroBased == 0
                ? GameBalance.TypeLevelUp.TwoFormFirstStagePoints
                : GameBalance.TypeLevelUp.TwoFormFinalStagePoints;
        }

        return activeStageIndexZeroBased switch
        {
            0 => GameBalance.TypeLevelUp.ThreePlusFormFirstStagePoints,
            1 => GameBalance.TypeLevelUp.ThreePlusFormMiddleStagePoints,
            _ => GameBalance.TypeLevelUp.ThreePlusFormLateStagePoints,
        };
    }
}
