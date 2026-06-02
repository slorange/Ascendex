namespace Ascendex.Game;

/// <summary>
/// Species display stages by bar level (starts at 0). <see cref="MinLevel"/> is the lowest level at which that form is shown (inclusive).
/// <see cref="TypeKey"/> is the primary type; <see cref="SecondaryTypeKey"/> is optional for dual typings and type counter splits.
/// <see cref="NormalColor"/> is the dominant sprite color for route bar fills; <see cref="ShinyColor"/> is the shiny palette for collections meta.
/// </summary>
public readonly record struct EvolutionStage(
    int MinLevel,
    string Name,
    string TypeKey,
    string NormalColor,
    string ShinyColor,
    string? SecondaryTypeKey = null);
