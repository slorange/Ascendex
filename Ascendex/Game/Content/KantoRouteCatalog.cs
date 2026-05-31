namespace Ascendex.Game.Content;

public readonly record struct RouteSpawn(
    string SpeciesRootName,
    bool IsBoss = false,
    bool AllowsCatching = true,
    bool StartsHidden = false);

public readonly record struct RouteDefinition(
    string Id,
    string ShortLabel,
    string DisplayName,
    RouteSpawn[] Spawns);

public static class KantoRouteCatalog
{
    public static readonly RouteDefinition[] All =
    [
        new(RouteIds.PalletTown, "PT", "Pallet Town",
        [
            new("Bulbasaur"), new("Charmander"), new("Squirtle"), new("Pikachu"),
        ]),
        new(RouteIds.Route1, "R1", "Route 1",
        [
            new("Pidgey"), new("Rattata"),
        ]),
        new(RouteIds.Route22, "R22", "Route 22",
        [
            new("Spearow"), new("Nidoran(f)"), new("Nidoran(m)"), new("Mankey"),
        ]),
        new(RouteIds.ViridianForest, "VF", "Viridian Forest",
        [
            new("Caterpie"), new("Weedle"),
        ]),
        new(RouteIds.Route3, "R3", "Route 3",
        [
            new("Ekans"), new("Sandshrew"), new("Jigglypuff"), new("Magikarp"),
        ]),
        new(RouteIds.MtMoon, "MM", "Mt Moon",
        [
            new("Clefairy"), new("Zubat"), new("Paras"),
        ]),
        new(RouteIds.Route24, "R24", "Route 24",
        [
            new("Oddish"), new("Bellsprout"), new("Venonat"), new("Abra"),
        ]),
        new(RouteIds.Route7, "R7", "Route 7",
        [
            new("Vulpix"), new("Meowth"), new("Growlithe"),
        ]),
        new(RouteIds.GoodRod, "GR", "Good Rod",
        [
            new("Psyduck"), new("Poliwag"), new("Goldeen"),
        ]),
        new(RouteIds.RouteX, "RX", "Route X",
        [
            new("Diglett"), new("Farfetch'd"), new("Drowzee"), new("Mr. Mime"),
        ]),
        new(RouteIds.RockTunnel, "RT", "Rock Tunnel",
        [
            new("Machop"), new("Geodude"), new("Onix"),
        ]),
        new(RouteIds.PokemonTower, "TWR", "Pokemon Tower",
        [
            new("Gastly"), new("Cubone"),
        ]),
        new(RouteIds.Celadon, "GC", "Celadon",
        [
            new("Porygon"), new("Dratini"), new("Eevee"),
            new("Flareon", AllowsCatching: false, StartsHidden: true),
            new("Jolteon", AllowsCatching: false, StartsHidden: true),
        ]),
        new(RouteIds.CyclingRoad, "CR", "Cycling Road",
        [
            new("Ponyta"), new("Doduo"), new("Snorlax", IsBoss: true),
        ]),
        new(RouteIds.SafariZone1, "SZ1", "Safari Zone 1",
        [
            new("Exeggcute"), new("Rhyhorn"), new("Tauros"), new("Scyther"), new("Pinsir"),
        ]),
        new(RouteIds.SafariZone2, "SZ2", "Safari Zone 2",
        [
            new("Lickitung"), new("Chansey"), new("Tangela"), new("Kangaskhan"),
        ]),
        new(RouteIds.SuperRod, "SR", "Super Rod",
        [
            new("Tentacool"), new("Slowpoke"), new("Horsea"), new("Staryu"),
        ]),
        new(RouteIds.SaffronCity, "SC", "Saffron City",
        [
            new("Hitmonlee"), new("Hitmonchan"), new("Lapras"),
        ]),
        new(RouteIds.PowerPlant, "PP", "Power Plant",
        [
            new("Magnemite"), new("Voltorb"), new("Electabuzz"), new("Zapdos", IsBoss: true),
        ]),
        new(RouteIds.SeafoamIslands, "SFI", "Seafoam Islands",
        [
            new("Seel"), new("Shellder"), new("Krabby"), new("Jynx"), new("Articuno", IsBoss: true),
        ]),
        new(RouteIds.PokemonMansion, "PM", "Pokemon Mansion",
        [
            new("Koffing"), new("Magmar"), new("Ditto"), new("Grimer"),
        ]),
        new(RouteIds.PokemonLabCinnabar, "LAB", "Pokemon Lab Cinnabar",
        [
            new("Omanyte"), new("Kabuto"), new("Aerodactyl"),
        ]),
        new(RouteIds.VictoryRoad, "VR", "Victory Road",
        [
            new("Moltres", IsBoss: true),
        ]),
        new(RouteIds.CeruleanCave, "CC", "Cerulean Cave",
        [
            new("Mewtwo", IsBoss: true),
        ]),
    ];
}
