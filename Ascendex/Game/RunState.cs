using System;
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

    /// <summary>Seconds of bank time; consumed at 3× rate while actively training or catching.</summary>
    public double BankTimeSeconds { get; set; }

    public bool ChampionResetUnlocked { get; set; }

    public int ChampionResetCount { get; set; }

    public int ExpShareCount { get; set; }

    /// <summary>FIFO order of species root names currently training (oldest first).</summary>
    public List<string> SpeciesTrainingOrder { get; } = new();

    public int PokedexResetCount { get; set; }

    public int ShinyCharmCount { get; set; }

    /// <summary>Species roots ever caught shiny; guarantees a shiny on the next catch after a reset.</summary>
    public HashSet<string> LifetimeShinySpeciesRoots { get; } = new(StringComparer.Ordinal);

    /// <summary>Banked guaranteed shinies passed forward when a guaranteed catch also wins the shiny roll.</summary>
    public int PendingGuaranteedShinies { get; set; }
}
