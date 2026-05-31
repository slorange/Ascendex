namespace Ascendex.Game;

/// <summary>
/// Species display stages by bar level (starts at 0). <see cref="MinLevel"/> is the lowest level at which that form is shown (inclusive).
/// <see cref="TypeKey"/> is the primary type (bar colors); <see cref="SecondaryTypeKey"/> is optional for dual typings and type counter splits.
/// </summary>
public readonly record struct EvolutionStage(
    int MinLevel,
    string Name,
    string TypeKey,
    string AccentColor,
    string ForegroundColor,
    string? SecondaryTypeKey = null);
