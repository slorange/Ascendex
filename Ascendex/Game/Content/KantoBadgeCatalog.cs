namespace Ascendex.Game.Content;

public enum BadgeTier
{
    Gym,
    EliteFour,
    Champion,
}

/// <summary>Kanto league honors tied to battle trainers. Gyms use classic badge framing; E4 and Champion use distinct league art.</summary>
public readonly record struct BadgeDefinition(string TrainerId, BadgeTier Tier, string DisplayName);

public static class KantoBadgeCatalog
{
    public static readonly BadgeDefinition[] GymBadges =
    [
        new(TrainerIds.Brock, BadgeTier.Gym, "Boulder"),
        new(TrainerIds.Misty, BadgeTier.Gym, "Cascade"),
        new(TrainerIds.LtSurge, BadgeTier.Gym, "Thunder"),
        new(TrainerIds.Erika, BadgeTier.Gym, "Rainbow"),
        new(TrainerIds.Koga, BadgeTier.Gym, "Soul"),
        new(TrainerIds.Sabrina, BadgeTier.Gym, "Marsh"),
        new(TrainerIds.Blaine, BadgeTier.Gym, "Volcano"),
        new(TrainerIds.Giovanni, BadgeTier.Gym, "Earth"),
    ];

    public static readonly BadgeDefinition[] LeagueHonors =
    [
        new(TrainerIds.Lorelei, BadgeTier.EliteFour, "Lorelei"),
        new(TrainerIds.Bruno, BadgeTier.EliteFour, "Bruno"),
        new(TrainerIds.Agatha, BadgeTier.EliteFour, "Agatha"),
        new(TrainerIds.Lance, BadgeTier.EliteFour, "Lance"),
        new(TrainerIds.Blue, BadgeTier.Champion, "Champion"),
    ];
}
