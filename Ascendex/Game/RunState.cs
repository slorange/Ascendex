using System.Collections.Generic;

namespace Ascendex.Game;

/// <summary>Serializable-friendly snapshot of in-run player progress.</summary>
public sealed class RunState
{
    public Dictionary<string, SpeciesProgress> SpeciesByRoot { get; } = new();

    public Dictionary<string, TrainerProgress> TrainersById { get; } = new();

    public Dictionary<string, int> TypeCounterCounts { get; } = new();

    public string SelectedRouteId { get; set; } = string.Empty;

    public bool CeladonAlternateEeveelutionsUnlocked { get; set; }
}
