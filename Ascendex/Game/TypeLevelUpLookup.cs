namespace Ascendex.Game;

/// <summary>
/// Maps evolution chain shape to type points per level using balance constants.
/// </summary>
public static class TypeLevelUpLookup
{
    public static int PointsForChainStage(int evolutionChainLength, int activeStageIndexZeroBased)
    {
        if (evolutionChainLength <= 1)
        {
            return ViewModels.GameBalance.TypeLevelUp.SingleFormSpeciesPointsPerLevel;
        }

        if (evolutionChainLength == 2)
        {
            return activeStageIndexZeroBased == 0
                ? ViewModels.GameBalance.TypeLevelUp.TwoFormFirstStagePoints
                : ViewModels.GameBalance.TypeLevelUp.TwoFormFinalStagePoints;
        }

        return activeStageIndexZeroBased switch
        {
            0 => ViewModels.GameBalance.TypeLevelUp.ThreePlusFormFirstStagePoints,
            1 => ViewModels.GameBalance.TypeLevelUp.ThreePlusFormMiddleStagePoints,
            _ => ViewModels.GameBalance.TypeLevelUp.ThreePlusFormLateStagePoints,
        };
    }
}
