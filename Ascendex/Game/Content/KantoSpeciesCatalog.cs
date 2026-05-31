using System;
using System.Collections.Generic;
using System.Linq;
using Ascendex.Game;

namespace Ascendex.Game.Content;

/// <summary>Kanto species data: National Dex order, evolution chains, and single-form bar palettes.</summary>
public static class KantoSpeciesCatalog
{
    /// <summary>National Dex species #001–#150 for a 10×15 grid.</summary>
    public static readonly string[] NationalDexNames =
    [
        "Bulbasaur", "Ivysaur", "Venusaur", "Charmander", "Charmeleon", "Charizard", "Squirtle", "Wartortle", "Blastoise", "Caterpie",
        "Metapod", "Butterfree", "Weedle", "Kakuna", "Beedrill", "Pidgey", "Pidgeotto", "Pidgeot", "Rattata", "Raticate",
        "Spearow", "Fearow", "Ekans", "Arbok", "Pikachu", "Raichu", "Sandshrew", "Sandslash", "Nidoran(f)", "Nidorina",
        "Nidoqueen", "Nidoran(m)", "Nidorino", "Nidoking", "Clefairy", "Clefable", "Vulpix", "Ninetales", "Jigglypuff", "Wigglytuff",
        "Zubat", "Golbat", "Oddish", "Gloom", "Vileplume", "Paras", "Parasect", "Venonat", "Venomoth", "Diglett",
        "Dugtrio", "Meowth", "Persian", "Psyduck", "Golduck", "Mankey", "Primeape", "Growlithe", "Arcanine", "Poliwag",
        "Poliwhirl", "Poliwrath", "Abra", "Kadabra", "Alakazam", "Machop", "Machoke", "Machamp", "Bellsprout", "Weepinbell",
        "Victreebel", "Tentacool", "Tentacruel", "Geodude", "Graveler", "Golem", "Ponyta", "Rapidash", "Slowpoke", "Slowbro",
        "Magnemite", "Magneton", "Farfetch'd", "Doduo", "Dodrio", "Seel", "Dewgong", "Grimer", "Muk", "Shellder",
        "Cloyster", "Gastly", "Haunter", "Gengar", "Onix", "Drowzee", "Hypno", "Krabby", "Kingler", "Voltorb",
        "Electrode", "Exeggcute", "Exeggutor", "Cubone", "Marowak", "Hitmonlee", "Hitmonchan", "Lickitung", "Koffing", "Weezing",
        "Rhyhorn", "Rhydon", "Chansey", "Tangela", "Kangaskhan", "Horsea", "Seadra", "Goldeen", "Seaking", "Staryu",
        "Starmie", "Mr. Mime", "Scyther", "Jynx", "Electabuzz", "Magmar", "Pinsir", "Tauros", "Magikarp", "Gyarados",
        "Lapras", "Ditto", "Eevee", "Vaporeon", "Jolteon", "Flareon", "Porygon", "Omanyte", "Omastar", "Kabuto",
        "Kabutops", "Aerodactyl", "Snorlax", "Articuno", "Zapdos", "Moltres", "Dratini", "Dragonair", "Dragonite", "Mewtwo",
    ];

    public static readonly IReadOnlyDictionary<string, int> CellIndexBySpeciesName =
        NationalDexNames.Select((name, i) => (name, i)).ToDictionary(t => t.name, t => t.i, StringComparer.Ordinal);

    public const int NationalDexCellCount = 150;

    private static readonly Dictionary<string, EvolutionStage[]> EvolutionChains = new(StringComparer.Ordinal)
    {
        ["Bulbasaur"] =
        [
            new(0, "Bulbasaur", "grass", "#78C86A", "#10210C", "poison"),
            new(16, "Ivysaur", "grass", "#5FAE52", "#081006", "poison"),
            new(32, "Venusaur", "grass", "#489040", "#071003", "poison"),
        ],
        ["Charmander"] =
        [
            new(0, "Charmander", "fire", "#E78A43", "#2B1202"),
            new(16, "Charmeleon", "fire", "#D96A2F", "#1E0C01"),
            new(36, "Charizard", "fire", "#F08028", "#2A0C02", "flying"),
        ],
        ["Squirtle"] =
        [
            new(0, "Squirtle", "water", "#73B9F2", "#071C2A"),
            new(16, "Wartortle", "water", "#5A9FD8", "#061525"),
            new(36, "Blastoise", "water", "#3C7CC0", "#040C14"),
        ],
        ["Pikachu"] =
        [
            new(0, "Pikachu", "electric", "#F3D44F", "#2B2400"),
            new(25, "Raichu", "electric", "#F0C030", "#2A1F00"),
        ],
        ["Eevee"] =
        [
            new(0, "Eevee", "normal", "#C9A27A", "#24150A"),
            new(25, "Vaporeon", "water", "#6890F0", "#0C1830"),
        ],
        ["Pidgey"] =
        [
            new(0, "Pidgey", "normal", "#D8C3A2", "#20150B", "flying"),
            new(18, "Pidgeotto", "normal", "#C4A882", "#1E1309", "flying"),
            new(36, "Pidgeot", "normal", "#B89268", "#1C1108", "flying"),
        ],
        ["Rattata"] =
        [
            new(0, "Rattata", "normal", "#B98AD3", "#130819"),
            new(20, "Raticate", "normal", "#A070C0", "#100615"),
        ],
        ["Spearow"] =
        [
            new(0, "Spearow", "normal", "#C68A58", "#201109", "flying"),
            new(20, "Fearow", "normal", "#B07848", "#1C0E07", "flying"),
        ],
        ["Nidoran(f)"] =
        [
            new(0, "Nidoran(f)", "poison", "#6CC6F8", "#06121A"),
            new(16, "Nidorina", "poison", "#5AA8D8", "#050F16"),
            new(35, "Nidoqueen", "ground", "#8CA8C0", "#101820", "poison"),
        ],
        ["Nidoran(m)"] =
        [
            new(0, "Nidoran(m)", "poison", "#B98AD3", "#170A1E"),
            new(16, "Nidorino", "poison", "#9A70C0", "#120818"),
            new(35, "Nidoking", "ground", "#9878C8", "#120818", "poison"),
        ],
        ["Mankey"] =
        [
            new(0, "Mankey", "fighting", "#D7C1AE", "#1D1106"),
            new(28, "Primeape", "fighting", "#C5A898", "#1A0E05"),
        ],
        ["Caterpie"] =
        [
            new(0, "Caterpie", "bug", "#91D469", "#102009"),
            new(7, "Metapod", "bug", "#7AB852", "#0E1C07"),
            new(10, "Butterfree", "bug", "#8FA6F2", "#0F1430", "flying"),
        ],
        ["Weedle"] =
        [
            new(0, "Weedle", "bug", "#D5A14D", "#241304", "poison"),
            new(7, "Kakuna", "bug", "#C08E40", "#201003", "poison"),
            new(10, "Beedrill", "bug", "#E8D038", "#1A1602", "poison"),
        ],
        ["Ekans"] =
        [
            new(0, "Ekans", "poison", "#8B63B7", "#15091F"),
            new(22, "Arbok", "poison", "#984868", "#180410"),
        ],
        ["Sandshrew"] =
        [
            new(0, "Sandshrew", "ground", "#D6B46E", "#241908"),
            new(22, "Sandslash", "ground", "#C4A25C", "#201507"),
        ],
        ["Jigglypuff"] =
        [
            new(0, "Jigglypuff", "fairy", "#F0A9D6", "#2B1020"),
            new(25, "Wigglytuff", "fairy", "#E88CC4", "#260E1C"),
        ],
        ["Magikarp"] =
        [
            new(0, "Magikarp", "water", "#E36B4E", "#2A0C05"),
            new(20, "Gyarados", "water", "#3A78C8", "#051018", "flying"),
        ],
        ["Clefairy"] =
        [
            new(0, "Clefairy", "fairy", "#F4B8DE", "#2A1020"),
            new(25, "Clefable", "fairy", "#E8A0D0", "#250E1C"),
        ],
        ["Zubat"] =
        [
            new(0, "Zubat", "poison", "#6F74C9", "#F3F4FF", "flying"),
            new(22, "Golbat", "poison", "#7058A8", "#0E0818", "flying"),
        ],
        ["Paras"] =
        [
            new(0, "Paras", "bug", "#D4744B", "#2B1207", "grass"),
            new(24, "Parasect", "bug", "#C86448", "#1C0C06", "grass"),
        ],
        ["Oddish"] =
        [
            new(0, "Oddish", "grass", "#4F85D1", "#07192B", "poison"),
            new(21, "Gloom", "grass", "#6B7FD6", "#0A1228", "poison"),
            new(35, "Vileplume", "grass", "#DE3D3D", "#2A0505", "poison"),
        ],
        ["Bellsprout"] =
        [
            new(0, "Bellsprout", "grass", "#A6D45C", "#102106", "poison"),
            new(21, "Weepinbell", "grass", "#8FC048", "#0E1C05", "poison"),
            new(35, "Victreebel", "grass", "#78A83C", "#0C1804", "poison"),
        ],
        ["Venonat"] =
        [
            new(0, "Venonat", "bug", "#A267BE", "#16091F"),
            new(31, "Venomoth", "bug", "#C4A8E0", "#180E22", "poison"),
        ],
        ["Abra"] =
        [
            new(0, "Abra", "psychic", "#E4C559", "#2A2106"),
            new(16, "Kadabra", "psychic", "#D4B548", "#261E05"),
            new(40, "Alakazam", "psychic", "#C4A538", "#221B04"),
        ],
        ["Vulpix"] =
        [
            new(0, "Vulpix", "fire", "#F3B16A", "#2A1404"),
            new(25, "Ninetales", "fire", "#E8DCC8", "#4A3C34"),
        ],
        ["Meowth"] =
        [
            new(0, "Meowth", "normal", "#DCC584", "#241A08"),
            new(28, "Persian", "normal", "#F0E2C0", "#2A2210"),
        ],
        ["Growlithe"] =
        [
            new(0, "Growlithe", "fire", "#E88B49", "#2B1102"),
            new(25, "Arcanine", "fire", "#D86828", "#201004"),
        ],
        ["Psyduck"] =
        [
            new(0, "Psyduck", "water", "#F1D25A", "#2A2103"),
            new(33, "Golduck", "water", "#5A9FD8", "#061525"),
        ],
        ["Poliwag"] =
        [
            new(0, "Poliwag", "water", "#7C9BE8", "#09122B"),
            new(25, "Poliwhirl", "water", "#6A88D8", "#080F24"),
            new(35, "Poliwrath", "water", "#4A7098", "#060C14", "fighting"),
        ],
        ["Goldeen"] =
        [
            new(0, "Goldeen", "water", "#F39A74", "#2B1207"),
            new(33, "Seaking", "water", "#F08858", "#261006"),
        ],
        ["Machop"] =
        [
            new(0, "Machop", "fighting", "#8BA4C8", "#0B1320"),
            new(28, "Machoke", "fighting", "#7890B0", "#09101C"),
            new(40, "Machamp", "fighting", "#657C98", "#070D18"),
        ],
        ["Geodude"] =
        [
            new(0, "Geodude", "rock", "#A68D63", "#211708", "ground"),
            new(25, "Graveler", "rock", "#8F7A52", "#1C1406", "ground"),
            new(40, "Golem", "rock", "#787860", "#141408", "ground"),
        ],
        ["Gastly"] =
        [
            new(0, "Gastly", "ghost", "#7B6AD0", "#F4F3FF", "poison"),
            new(25, "Haunter", "ghost", "#5868B8", "#0A1028", "poison"),
            new(40, "Gengar", "ghost", "#483068", "#F0E8F8", "poison"),
        ],
        ["Cubone"] =
        [
            new(0, "Cubone", "ground", "#C7AF8F", "#21180B"),
            new(28, "Marowak", "ground", "#D09068", "#1A1208"),
        ],
        ["Dratini"] =
        [
            new(0, "Dratini", "dragon", "#8C85EE", "#0E1030"),
            new(30, "Dragonair", "dragon", "#7BA8E8", "#0A1624"),
            new(55, "Dragonite", "dragon", "#F2A24A", "#3E1F08", "flying"),
        ],
        ["Diglett"] =
        [
            new(0, "Diglett", "ground", "#A46855", "#210E08"),
            new(26, "Dugtrio", "ground", "#925844", "#1D0C07"),
        ],
        ["Drowzee"] =
        [
            new(0, "Drowzee", "psychic", "#E1C95B", "#271F05"),
            new(26, "Hypno", "psychic", "#D0B848", "#241C04"),
        ],
        ["Ponyta"] =
        [
            new(0, "Ponyta", "fire", "#F28C54", "#2A1103"),
            new(40, "Rapidash", "fire", "#E88A38", "#281004"),
        ],
        ["Doduo"] =
        [
            new(0, "Doduo", "normal", "#C9A067", "#211407", "flying"),
            new(31, "Dodrio", "normal", "#A87850", "#1C140A", "flying"),
        ],
        ["Tentacool"] =
        [
            new(0, "Tentacool", "water", "#68C8E8", "#062020", "poison"),
            new(30, "Tentacruel", "water", "#4078A8", "#050C14", "poison"),
        ],
        ["Slowpoke"] =
        [
            new(0, "Slowpoke", "water", "#E8A8BC", "#2A1020"),
            new(37, "Slowbro", "water", "#D888A8", "#241018", "psychic"),
        ],
        ["Horsea"] =
        [
            new(0, "Horsea", "water", "#5A8FE5", "#071A2A"),
            new(32, "Seadra", "water", "#3A68C0", "#050C18"),
        ],
        ["Staryu"] =
        [
            new(0, "Staryu", "water", "#C48E4D", "#241507"),
            new(25, "Starmie", "water", "#B060D0", "#16081E", "psychic"),
        ],
        ["Exeggcute"] =
        [
            new(0, "Exeggcute", "grass", "#F0B5D2", "#2A0F1D"),
            new(25, "Exeggutor", "grass", "#DCC848", "#252008", "psychic"),
        ],
        ["Rhyhorn"] =
        [
            new(0, "Rhyhorn", "ground", "#908878", "#1C1612", "rock"),
            new(42, "Rhydon", "ground", "#7E7568", "#181410", "rock"),
        ],
        ["Magnemite"] =
        [
            new(0, "Magnemite", "electric", "#A8B4C4", "#0C1018"),
            new(30, "Magneton", "electric", "#8898A8", "#0A1016"),
        ],
        ["Voltorb"] =
        [
            new(0, "Voltorb", "electric", "#E65B5B", "#2A0808"),
            new(30, "Electrode", "electric", "#E8E8E8", "#303030"),
        ],
        ["Seel"] =
        [
            new(0, "Seel", "water", "#D7EEF8", "#0C1C22"),
            new(34, "Dewgong", "water", "#B0D8EC", "#082028", "ice"),
        ],
        ["Shellder"] =
        [
            new(0, "Shellder", "water", "#7766C8", "#F4F3FF"),
            new(25, "Cloyster", "water", "#584878", "#F0E8FF", "ice"),
        ],
        ["Krabby"] =
        [
            new(0, "Krabby", "water", "#E3714D", "#2A0C05"),
            new(28, "Kingler", "water", "#E85030", "#1C0804"),
        ],
        ["Omanyte"] =
        [
            new(0, "Omanyte", "rock", "#7A92D6", "#09142A", "water"),
            new(40, "Omastar", "rock", "#5A78C0", "#081018", "water"),
        ],
        ["Kabuto"] =
        [
            new(0, "Kabuto", "rock", "#8A7363", "#1E120C", "water"),
            new(40, "Kabutops", "rock", "#5A7868", "#0C140E", "water"),
        ],
        ["Koffing"] =
        [
            new(0, "Koffing", "poison", "#9162B2", "#14091C"),
            new(35, "Weezing", "poison", "#8090A8", "#101418"),
        ],
        ["Grimer"] =
        [
            new(0, "Grimer", "poison", "#7A5C97", "#F7F4FF"),
            new(38, "Muk", "poison", "#584858", "#F0E8F0"),
        ],
    };

    private static readonly Dictionary<string, (string TypeKey, BarPalette Palette)> StandaloneSpecies = new(StringComparer.Ordinal)
    {
        ["Flareon"] = ("fire", new BarPalette("#F87050", "#2A0C08")),
        ["Jolteon"] = ("electric", new BarPalette("#F8E030", "#262004")),
        ["Onix"] = ("rock", new BarPalette("#9CA8A8", "#0E1416")),
        ["Scyther"] = ("bug", new BarPalette("#78D0A0", "#0C2014")),
        ["Pinsir"] = ("bug", new BarPalette("#904830", "#1C0C06")),
        ["Porygon"] = ("normal", new BarPalette("#E898D0", "#2A1020")),
        ["Farfetch'd"] = ("normal", new BarPalette("#C8A878", "#24180A")),
        ["Mr. Mime"] = ("psychic", new BarPalette("#F09CC3", "#2B0C19")),
        ["Hitmonlee"] = ("fighting", new BarPalette("#B87860", "#1C1008")),
        ["Hitmonchan"] = ("fighting", new BarPalette("#E05050", "#2A0808")),
        ["Lapras"] = ("ice", new BarPalette("#88C8E8", "#08222C")),
        ["Tauros"] = ("normal", new BarPalette("#B08058", "#1C1008")),
        ["Lickitung"] = ("normal", new BarPalette("#F0B8D0", "#2A1018")),
        ["Chansey"] = ("normal", new BarPalette("#FFE8F0", "#3A1824")),
        ["Tangela"] = ("grass", new BarPalette("#5078D8", "#0A1028")),
        ["Kangaskhan"] = ("normal", new BarPalette("#D0A880", "#24180C")),
        ["Electabuzz"] = ("electric", new BarPalette("#F0DC40", "#1A1604")),
        ["Jynx"] = ("ice", new BarPalette("#C86898", "#1C0818")),
        ["Aerodactyl"] = ("rock", new BarPalette("#B898D0", "#181020")),
        ["Magmar"] = ("fire", new BarPalette("#F07040", "#281008")),
        ["Ditto"] = ("normal", new BarPalette("#E8C0E8", "#301828")),
        ["Snorlax"] = ("normal", new BarPalette("#2E4A6E", "#E8F0F8")),
        ["Zapdos"] = ("electric", new BarPalette("#F8D030", "#2A2000")),
        ["Articuno"] = ("ice", new BarPalette("#78D8F8", "#082028")),
        ["Moltres"] = ("fire", new BarPalette("#F87830", "#2A1000")),
        ["Mewtwo"] = ("psychic", new BarPalette("#A070C8", "#180820")),
    };

    public static EvolutionStage[]? TryGetEvolutionChain(string rootSpeciesName) =>
        EvolutionChains.TryGetValue(rootSpeciesName, out var stages) ? stages : null;

    /// <summary>Maps a National Dex display name to the route species root used for progress.</summary>
    public static bool TryGetRootForDexName(string dexSpeciesName, out string speciesRoot)
    {
        if (EvolutionChains.ContainsKey(dexSpeciesName))
        {
            speciesRoot = dexSpeciesName;
            return true;
        }

        foreach (var (root, stages) in EvolutionChains)
        {
            foreach (var stage in stages)
            {
                if (stage.Name == dexSpeciesName)
                {
                    speciesRoot = root;
                    return true;
                }
            }
        }

        if (StandaloneSpecies.ContainsKey(dexSpeciesName))
        {
            speciesRoot = dexSpeciesName;
            return true;
        }

        speciesRoot = string.Empty;
        return false;
    }

    public static string PrimaryTypeKey(string speciesRootName)
    {
        var chain = TryGetEvolutionChain(speciesRootName);
        if (chain is { Length: > 0 })
        {
            return chain[0].TypeKey;
        }

        if (StandaloneSpecies.TryGetValue(speciesRootName, out var standalone))
        {
            return standalone.TypeKey;
        }

        return "normal";
    }

    public static BarPalette ResolveRouteBarPalette(string speciesRootName)
    {
        var chain = TryGetEvolutionChain(speciesRootName);
        if (chain is { Length: > 0 })
        {
            return new BarPalette(chain[0].AccentColor, chain[0].ForegroundColor);
        }

        if (StandaloneSpecies.TryGetValue(speciesRootName, out var standalone))
        {
            return standalone.Palette;
        }

        return TypeCatalog.BarPaletteForTypeKey(PrimaryTypeKey(speciesRootName));
    }

    public static BarPalette ResolveTrainerBarPalette(string typeKey) =>
        TypeCatalog.BarPaletteForTypeKey(typeKey);
}
