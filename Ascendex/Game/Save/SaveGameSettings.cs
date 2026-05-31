using System;

namespace Ascendex.Game.Save;

/// <summary>Persistence timing — not gameplay balance.</summary>
public sealed class SaveGameSettings
{
    public TimeSpan AutoSaveInterval { get; init; } = TimeSpan.FromSeconds(5);
}
