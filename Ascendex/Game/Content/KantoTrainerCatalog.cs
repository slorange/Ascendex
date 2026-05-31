namespace Ascendex.Game.Content;

public readonly record struct TrainerDefinition(string Id, string DisplayName, string TypeKey);

public static class KantoTrainerCatalog
{
    public static readonly TrainerDefinition[] All =
    [
        new(TrainerIds.Brock, "Brock", "rock"),
        new(TrainerIds.Misty, "Misty", "water"),
        new(TrainerIds.LtSurge, "Lt. Surge", "electric"),
        new(TrainerIds.Erika, "Erika", "grass"),
        new(TrainerIds.Koga, "Koga", "poison"),
        new(TrainerIds.Sabrina, "Sabrina", "psychic"),
        new(TrainerIds.Blaine, "Blaine", "fire"),
        new(TrainerIds.Giovanni, "Giovanni", "ground"),
        new(TrainerIds.Lorelei, "Lorelei", "ice"),
        new(TrainerIds.Bruno, "Bruno", "fighting"),
        new(TrainerIds.Agatha, "Agatha", "ghost"),
        new(TrainerIds.Lance, "Lance", "dragon"),
        new(TrainerIds.Blue, "Blue", "normal"),
    ];
}
