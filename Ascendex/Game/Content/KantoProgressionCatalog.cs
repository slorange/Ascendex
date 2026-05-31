namespace Ascendex.Game.Content;

public enum ProgressionStepKind
{
    Route,
    Trainer,
}

public readonly record struct ProgressionStep(ProgressionStepKind Kind, string TargetId);

public readonly record struct OptionalRouteUnlock(string RouteId, ProgressionStep UnlockWhen);

/// <summary>Linear world order: each step unlocks when the previous step is complete.</summary>
public static class KantoProgressionCatalog
{
    /// <summary>Side routes: visible when <see cref="OptionalRouteUnlock.UnlockWhen"/> is complete, but not required for the next entry in <see cref="Order"/>.</summary>
    public static readonly OptionalRouteUnlock[] OptionalRouteUnlocks =
    [
        new(RouteIds.VictoryRoad, new(ProgressionStepKind.Trainer, TrainerIds.Giovanni)),
    ];

    public static readonly ProgressionStep[] Order =
    [
        new(ProgressionStepKind.Route, RouteIds.PalletTown),
        new(ProgressionStepKind.Route, RouteIds.Route1),
        new(ProgressionStepKind.Route, RouteIds.Route22),
        new(ProgressionStepKind.Route, RouteIds.ViridianForest),
        new(ProgressionStepKind.Trainer, TrainerIds.Brock),
        new(ProgressionStepKind.Route, RouteIds.Route3),
        new(ProgressionStepKind.Route, RouteIds.MtMoon),
        new(ProgressionStepKind.Route, RouteIds.Route24),
        new(ProgressionStepKind.Trainer, TrainerIds.Misty),
        new(ProgressionStepKind.Route, RouteIds.Route7),
        new(ProgressionStepKind.Route, RouteIds.GoodRod),
        new(ProgressionStepKind.Route, RouteIds.RouteX),
        new(ProgressionStepKind.Trainer, TrainerIds.LtSurge),
        new(ProgressionStepKind.Route, RouteIds.RockTunnel),
        new(ProgressionStepKind.Route, RouteIds.PokemonTower),
        new(ProgressionStepKind.Route, RouteIds.Celadon),
        new(ProgressionStepKind.Trainer, TrainerIds.Erika),
        new(ProgressionStepKind.Route, RouteIds.CyclingRoad),
        new(ProgressionStepKind.Route, RouteIds.SafariZone1),
        new(ProgressionStepKind.Route, RouteIds.SafariZone2),
        new(ProgressionStepKind.Trainer, TrainerIds.Koga),
        new(ProgressionStepKind.Route, RouteIds.SuperRod),
        new(ProgressionStepKind.Route, RouteIds.SaffronCity),
        new(ProgressionStepKind.Trainer, TrainerIds.Sabrina),
        new(ProgressionStepKind.Route, RouteIds.PowerPlant),
        new(ProgressionStepKind.Route, RouteIds.SeafoamIslands),
        new(ProgressionStepKind.Route, RouteIds.PokemonMansion),
        new(ProgressionStepKind.Route, RouteIds.PokemonLabCinnabar),
        new(ProgressionStepKind.Trainer, TrainerIds.Blaine),
        new(ProgressionStepKind.Trainer, TrainerIds.Giovanni),
        new(ProgressionStepKind.Trainer, TrainerIds.Lorelei),
        new(ProgressionStepKind.Trainer, TrainerIds.Bruno),
        new(ProgressionStepKind.Trainer, TrainerIds.Agatha),
        new(ProgressionStepKind.Trainer, TrainerIds.Lance),
        new(ProgressionStepKind.Trainer, TrainerIds.Blue),
        new(ProgressionStepKind.Route, RouteIds.CeruleanCave),
    ];
}
