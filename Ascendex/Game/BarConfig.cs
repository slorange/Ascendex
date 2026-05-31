namespace Ascendex.Game;

public sealed class SpeciesBarConfig
{
    public required string SpeciesRootName { get; init; }

    public double BaseProgressRequired { get; init; }

    public double ProgressRequiredPerLevelExponent { get; init; }

    public double CatchDifficultyMultiplier { get; init; } = 1.0;

    public bool AllowsCatching { get; init; } = true;

    public EvolutionStage[]? EvolutionChain { get; init; }
}

public sealed class TrainerBarConfig
{
    public required string TrainerId { get; init; }

    public double BaseProgressRequired { get; init; }

    public double ProgressRequiredPerLevelExponent { get; init; }
}
