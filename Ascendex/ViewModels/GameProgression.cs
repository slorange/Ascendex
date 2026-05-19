namespace Ascendex.ViewModels;

public enum ProgressionEntryKind
{
    Route,
    Trainer
}

public readonly record struct ProgressionEntry(ProgressionEntryKind Kind, string Key);

/// <summary>Linear world order: each step unlocks when the previous step is complete.</summary>
public static class GameProgression
{
    /// <summary>Side routes: visible when <see cref="UnlockWhen"/> is complete, but not required for the next entry in <see cref="Order"/>.</summary>
    public static readonly (string RouteKey, ProgressionEntry UnlockWhen)[] OptionalRouteUnlocks =
    [
        ("Victory Road", new(ProgressionEntryKind.Trainer, "Giovanni")),
    ];

    public static readonly ProgressionEntry[] Order =
    [
        new(ProgressionEntryKind.Route, "Pallet Town"),
        new(ProgressionEntryKind.Route, "Route 1"),
        new(ProgressionEntryKind.Route, "Route 22"),
        new(ProgressionEntryKind.Route, "Viridian Forest"),
        new(ProgressionEntryKind.Trainer, "Brock"),
        new(ProgressionEntryKind.Route, "Route 3"),
        new(ProgressionEntryKind.Route, "Mt Moon"),
        new(ProgressionEntryKind.Route, "Route 24"),
        new(ProgressionEntryKind.Trainer, "Misty"),
        new(ProgressionEntryKind.Route, "Route 7"),
        new(ProgressionEntryKind.Route, "Good Rod"),
        new(ProgressionEntryKind.Route, "Route X"),
        new(ProgressionEntryKind.Trainer, "Lt. Surge"),
        new(ProgressionEntryKind.Route, "Rock Tunnel"),
        new(ProgressionEntryKind.Route, "Pokemon Tower"),
        new(ProgressionEntryKind.Route, "Celadon"),
        new(ProgressionEntryKind.Trainer, "Erika"),
        new(ProgressionEntryKind.Route, "Cycling Road"),
        new(ProgressionEntryKind.Route, "Safari Zone 1"),
        new(ProgressionEntryKind.Route, "Safari Zone 2"),
        new(ProgressionEntryKind.Trainer, "Koga"),
        new(ProgressionEntryKind.Route, "Super Rod"),
        new(ProgressionEntryKind.Route, "Saffron City"),
        new(ProgressionEntryKind.Trainer, "Sabrina"),
        new(ProgressionEntryKind.Route, "Power Plant"),
        new(ProgressionEntryKind.Route, "Seafoam Islands"),
        new(ProgressionEntryKind.Route, "Pokemon Mansion"),
        new(ProgressionEntryKind.Route, "Pokemon Lab Cinnabar"),
        new(ProgressionEntryKind.Trainer, "Blaine"),
        new(ProgressionEntryKind.Trainer, "Giovanni"),
        new(ProgressionEntryKind.Trainer, "Lorelei"),
        new(ProgressionEntryKind.Trainer, "Bruno"),
        new(ProgressionEntryKind.Trainer, "Agatha"),
        new(ProgressionEntryKind.Trainer, "Lance"),
        new(ProgressionEntryKind.Trainer, "Blue"),
        new(ProgressionEntryKind.Route, "Cerulean Cave"),
    ];
}
