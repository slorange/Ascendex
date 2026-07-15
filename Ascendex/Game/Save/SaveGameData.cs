using System;
using System.Collections.Generic;

namespace Ascendex.Game.Save;

public static class SaveGameVersions
{
    public const int Current = 5;
}

/// <summary>Versioned, JSON-serializable player save.</summary>
public sealed class SaveGameData
{
    public int Version { get; set; } = SaveGameVersions.Current;

    public DateTimeOffset SavedAtUtc { get; set; }

    public int SelectedMainTab { get; set; }

    public string SelectedRouteId { get; set; } = string.Empty;

    public bool CeladonAlternateEeveelutionsUnlocked { get; set; }

    public double BankTimeSeconds { get; set; }

    public bool ChampionResetUnlocked { get; set; }

    public int ChampionResetCount { get; set; }

    public int ExpShareCount { get; set; }

    public List<string> SpeciesTrainingOrder { get; set; } = new();

    public int PokedexResetCount { get; set; }

    public int ShinyCharmCount { get; set; }

    public List<string> LifetimeShinySpeciesRoots { get; set; } = new();

    public int PendingGuaranteedShinies { get; set; }

    public long Pokedollars { get; set; }

    public List<string> OwnedShopItemIds { get; set; } = new();

    public int UnassignedVitaminCount { get; set; }

    public bool VitaminApplySectionUnlocked { get; set; }

    public Dictionary<string, int> VitaminDosesBySpeciesRoot { get; set; } = new();

    public Dictionary<string, SpeciesProgressData> Species { get; set; } = new();

    public Dictionary<string, TrainerProgressData> Trainers { get; set; } = new();

    public Dictionary<string, int> TypeCounterCounts { get; set; } = new();
}

public sealed class SpeciesProgressData
{
    public int Level { get; set; }

    public double Progress { get; set; }

    public bool IsTraining { get; set; }

    public bool IsCatching { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool IsShiny { get; set; }
}

public sealed class TrainerProgressData
{
    public int Level { get; set; }

    public double Progress { get; set; }

    public bool IsTraining { get; set; }
}
