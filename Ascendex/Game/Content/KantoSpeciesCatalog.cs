using System;
using System.Collections.Generic;
using System.Linq;
using Ascendex.Game;

namespace Ascendex.Game.Content;

/// <summary>Kanto species data: National Dex order, evolution chains, and single-form palettes.</summary>
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
            new(0, "Bulbasaur", "grass", "#78C86A", "#689878", "poison"),
            new(16, "Ivysaur", "grass", "#5FAE52", "#508868", "poison"),
            new(32, "Venusaur", "grass", "#489040", "#407850", "poison"),
        ],
        ["Charmander"] =
        [
            new(0, "Charmander", "fire", "#E78A43", "#F8C848"),
            new(16, "Charmeleon", "fire", "#D96A2F", "#E8A838"),
            new(36, "Charizard", "fire", "#F08028", "#303030", "flying"),
        ],
        ["Squirtle"] =
        [
            new(0, "Squirtle", "water", "#73B9F2", "#9888D0"),
            new(16, "Wartortle", "water", "#5A9FD8", "#8878C0"),
            new(36, "Blastoise", "water", "#3C7CC0", "#6860A8"),
        ],
        ["Pikachu"] =
        [
            new(0, "Pikachu", "electric", "#F3D44F", "#D0A820"),
            new(25, "Raichu", "electric", "#F0C030", "#C89818"),
        ],
        ["Eevee"] =
        [
            new(0, "Eevee", "normal", "#C9A27A", "#F8F0D8"),
            new(25, "Vaporeon", "water", "#6890F0", "#8868C8"),
        ],
        ["Pidgey"] =
        [
            new(0, "Pidgey", "normal", "#D8C3A2", "#C8B898", "flying"),
            new(18, "Pidgeotto", "normal", "#C4A882", "#B89870", "flying"),
            new(36, "Pidgeot", "normal", "#B89268", "#A88858", "flying"),
        ],
        ["Rattata"] =
        [
            new(0, "Rattata", "normal", "#B98AD3", "#58A878"),
            new(20, "Raticate", "normal", "#A070C0", "#489868"),
        ],
        ["Spearow"] =
        [
            new(0, "Spearow", "normal", "#C68A58", "#D89868", "flying"),
            new(20, "Fearow", "normal", "#B07848", "#C88858", "flying"),
        ],
        ["Nidoran(f)"] =
        [
            new(0, "Nidoran(f)", "poison", "#6CC6F8", "#9888F8"),
            new(16, "Nidorina", "poison", "#5AA8D8", "#8870C8"),
            new(35, "Nidoqueen", "ground", "#8CA8C0", "#78C0A8", "poison"),
        ],
        ["Nidoran(m)"] =
        [
            new(0, "Nidoran(m)", "poison", "#B98AD3", "#78A8D3"),
            new(16, "Nidorino", "poison", "#9A70C0", "#6898C0"),
            new(35, "Nidoking", "ground", "#9878C8", "#6898A8", "poison"),
        ],
        ["Mankey"] =
        [
            new(0, "Mankey", "fighting", "#D7C1AE", "#E8D0B8"),
            new(28, "Primeape", "fighting", "#C5A898", "#D8C0A8"),
        ],
        ["Caterpie"] =
        [
            new(0, "Caterpie", "bug", "#91D469", "#B8D878"),
            new(7, "Metapod", "bug", "#7AB852", "#98C868"),
            new(10, "Butterfree", "bug", "#8FA6F2", "#F080A0", "flying"),
        ],
        ["Weedle"] =
        [
            new(0, "Weedle", "bug", "#D5A14D", "#E8C860", "poison"),
            new(7, "Kakuna", "bug", "#C08E40", "#D8B050", "poison"),
            new(10, "Beedrill", "bug", "#E8D038", "#F0E850", "poison"),
        ],
        ["Ekans"] =
        [
            new(0, "Ekans", "poison", "#8B63B7", "#B87898"),
            new(22, "Arbok", "poison", "#984868", "#A85878"),
        ],
        ["Sandshrew"] =
        [
            new(0, "Sandshrew", "ground", "#D6B46E", "#E8C880"),
            new(22, "Sandslash", "ground", "#C4A25C", "#D8B870"),
        ],
        ["Jigglypuff"] =
        [
            new(0, "Jigglypuff", "fairy", "#F0A9D6", "#F8C8E0"),
            new(25, "Wigglytuff", "fairy", "#E88CC4", "#F0A8D0"),
        ],
        ["Magikarp"] =
        [
            new(0, "Magikarp", "water", "#E36B4E", "#F8D030"),
            new(20, "Gyarados", "water", "#3A78C8", "#C83830", "flying"),
        ],
        ["Clefairy"] =
        [
            new(0, "Clefairy", "fairy", "#F4B8DE", "#F8D8E8"),
            new(25, "Clefable", "fairy", "#E8A0D0", "#F0C0E0"),
        ],
        ["Zubat"] =
        [
            new(0, "Zubat", "poison", "#6F74C9", "#9888E8", "flying"),
            new(22, "Golbat", "poison", "#7058A8", "#8878C8", "flying"),
        ],
        ["Paras"] =
        [
            new(0, "Paras", "bug", "#D4744B", "#E89868", "grass"),
            new(24, "Parasect", "bug", "#C86448", "#D88858", "grass"),
        ],
        ["Oddish"] =
        [
            new(0, "Oddish", "grass", "#4F85D1", "#6898D8", "poison"),
            new(21, "Gloom", "grass", "#6B7FD6", "#8898E8", "poison"),
            new(35, "Vileplume", "grass", "#DE3D3D", "#F87878", "poison"),
        ],
        ["Bellsprout"] =
        [
            new(0, "Bellsprout", "grass", "#A6D45C", "#B8E878", "poison"),
            new(21, "Weepinbell", "grass", "#8FC048", "#A0D858", "poison"),
            new(35, "Victreebel", "grass", "#78A83C", "#88C848", "poison"),
        ],
        ["Venonat"] =
        [
            new(0, "Venonat", "bug", "#A267BE", "#6888E8"),
            new(31, "Venomoth", "bug", "#C4A8E0", "#8898F0", "poison"),
        ],
        ["Abra"] =
        [
            new(0, "Abra", "psychic", "#E4C559", "#F8D878"),
            new(16, "Kadabra", "psychic", "#D4B548", "#E8C868"),
            new(40, "Alakazam", "psychic", "#C4A538", "#E8D050"),
        ],
        ["Vulpix"] =
        [
            new(0, "Vulpix", "fire", "#F3B16A", "#C0B0A8"),
            new(25, "Ninetales", "fire", "#E8DCC8", "#C8C8D8"),
        ],
        ["Meowth"] =
        [
            new(0, "Meowth", "normal", "#DCC584", "#E8D898"),
            new(28, "Persian", "normal", "#F0E2C0", "#F8F0D0"),
        ],
        ["Growlithe"] =
        [
            new(0, "Growlithe", "fire", "#E88B49", "#F8B878"),
            new(25, "Arcanine", "fire", "#D86828", "#E8C040"),
        ],
        ["Psyduck"] =
        [
            new(0, "Psyduck", "water", "#F1D25A", "#F8E878"),
            new(33, "Golduck", "water", "#5A9FD8", "#4888C8"),
        ],
        ["Poliwag"] =
        [
            new(0, "Poliwag", "water", "#7C9BE8", "#6890E8"),
            new(25, "Poliwhirl", "water", "#6A88D8", "#5880D8"),
            new(35, "Poliwrath", "water", "#4A7098", "#407888", "fighting"),
        ],
        ["Goldeen"] =
        [
            new(0, "Goldeen", "water", "#F39A74", "#F8B888"),
            new(33, "Seaking", "water", "#F08858", "#F8A068"),
        ],
        ["Machop"] =
        [
            new(0, "Machop", "fighting", "#8BA4C8", "#A0B8D8"),
            new(28, "Machoke", "fighting", "#7890B0", "#90A8C8"),
            new(40, "Machamp", "fighting", "#657C98", "#7898B0"),
        ],
        ["Geodude"] =
        [
            new(0, "Geodude", "rock", "#A68D63", "#B8A878", "ground"),
            new(25, "Graveler", "rock", "#8F7A52", "#A09068", "ground"),
            new(40, "Golem", "rock", "#787860", "#889878", "ground"),
        ],
        ["Gastly"] =
        [
            new(0, "Gastly", "ghost", "#7B6AD0", "#9898E8", "poison"),
            new(25, "Haunter", "ghost", "#5868B8", "#7888D0", "poison"),
            new(40, "Gengar", "ghost", "#6068B8", "#6868A8", "poison"),
        ],
        ["Cubone"] =
        [
            new(0, "Cubone", "ground", "#C7AF8F", "#D8C8A8"),
            new(28, "Marowak", "ground", "#D09068", "#E8A878"),
        ],
        ["Dratini"] =
        [
            new(0, "Dratini", "dragon", "#8C85EE", "#9898F8"),
            new(30, "Dragonair", "dragon", "#7BA8E8", "#8898F0"),
            new(55, "Dragonite", "dragon", "#F2A24A", "#A8C878", "flying"),
        ],
        ["Diglett"] =
        [
            new(0, "Diglett", "ground", "#A46855", "#B87868"),
            new(26, "Dugtrio", "ground", "#925844", "#A86858"),
        ],
        ["Drowzee"] =
        [
            new(0, "Drowzee", "psychic", "#E1C95B", "#F0D878"),
            new(26, "Hypno", "psychic", "#D0B848", "#E0C858"),
        ],
        ["Ponyta"] =
        [
            new(0, "Ponyta", "fire", "#F28C54", "#F8A868"),
            new(40, "Rapidash", "fire", "#E88A38", "#5098D8"),
        ],
        ["Doduo"] =
        [
            new(0, "Doduo", "normal", "#C9A067", "#D8B078", "flying"),
            new(31, "Dodrio", "normal", "#A87850", "#B89068", "flying"),
        ],
        ["Tentacool"] =
        [
            new(0, "Tentacool", "water", "#68C8E8", "#8898F0", "poison"),
            new(30, "Tentacruel", "water", "#4078A8", "#5878C8", "poison"),
        ],
        ["Slowpoke"] =
        [
            new(0, "Slowpoke", "water", "#E8A8BC", "#F8C8D8"),
            new(37, "Slowbro", "water", "#D888A8", "#E8A8C8", "psychic"),
        ],
        ["Horsea"] =
        [
            new(0, "Horsea", "water", "#5A8FE5", "#6898F0"),
            new(32, "Seadra", "water", "#3A68C0", "#5078D0"),
        ],
        ["Staryu"] =
        [
            new(0, "Staryu", "water", "#C48E4D", "#D8A058"),
            new(25, "Starmie", "water", "#B060D0", "#C878E8", "psychic"),
        ],
        ["Exeggcute"] =
        [
            new(0, "Exeggcute", "grass", "#F0B5D2", "#F8D0E0"),
            new(25, "Exeggutor", "grass", "#DCC848", "#E8D858", "psychic"),
        ],
        ["Rhyhorn"] =
        [
            new(0, "Rhyhorn", "ground", "#908878", "#A89888", "rock"),
            new(42, "Rhydon", "ground", "#7E7568", "#908878", "rock"),
        ],
        ["Magnemite"] =
        [
            new(0, "Magnemite", "electric", "#A8B4C4", "#B8C8D8"),
            new(30, "Magneton", "electric", "#8898A8", "#98A8B8"),
        ],
        ["Voltorb"] =
        [
            new(0, "Voltorb", "electric", "#E65B5B", "#6890E8"),
            new(30, "Electrode", "electric", "#E8E8E8", "#6890E8"),
        ],
        ["Seel"] =
        [
            new(0, "Seel", "water", "#D7EEF8", "#E8F8FF"),
            new(34, "Dewgong", "water", "#B0D8EC", "#C0E8F8", "ice"),
        ],
        ["Shellder"] =
        [
            new(0, "Shellder", "water", "#7766C8", "#9888E8"),
            new(25, "Cloyster", "water", "#584878", "#7878A8", "ice"),
        ],
        ["Krabby"] =
        [
            new(0, "Krabby", "water", "#E3714D", "#F89070"),
            new(28, "Kingler", "water", "#E85030", "#F86848"),
        ],
        ["Omanyte"] =
        [
            new(0, "Omanyte", "rock", "#7A92D6", "#8898E8", "water"),
            new(40, "Omastar", "rock", "#5A78C0", "#6888D0", "water"),
        ],
        ["Kabuto"] =
        [
            new(0, "Kabuto", "rock", "#8A7363", "#989878", "water"),
            new(40, "Kabutops", "rock", "#5A7868", "#689878", "water"),
        ],
        ["Koffing"] =
        [
            new(0, "Koffing", "poison", "#9162B2", "#A878C8"),
            new(35, "Weezing", "poison", "#8090A8", "#98A8C0"),
        ],
        ["Grimer"] =
        [
            new(0, "Grimer", "poison", "#7A5C97", "#9898D0"),
            new(38, "Muk", "poison", "#584858", "#7878A8"),
        ],
    };

    private static readonly Dictionary<string, (string TypeKey, string NormalColor, string ShinyColor)> StandaloneSpecies =
        new(StringComparer.Ordinal)
        {
            ["Flareon"] = ("fire", "#F87050", "#F0C060"),
            ["Jolteon"] = ("electric", "#F8E030", "#F0D848"),
            ["Onix"] = ("rock", "#9CA8A8", "#78A858"),
            ["Scyther"] = ("bug", "#78D0A0", "#90E8B8"),
            ["Pinsir"] = ("bug", "#904830", "#A85840"),
            ["Porygon"] = ("normal", "#E898D0", "#F8B8E0"),
            ["Farfetch'd"] = ("normal", "#C8A878", "#D8B888"),
            ["Mr. Mime"] = ("psychic", "#F09CC3", "#F8B8D8"),
            ["Hitmonlee"] = ("fighting", "#B87860", "#D89878"),
            ["Hitmonchan"] = ("fighting", "#E05050", "#F07878"),
            ["Lapras"] = ("ice", "#88C8E8", "#A8E0F8"),
            ["Tauros"] = ("normal", "#B08058", "#C89868"),
            ["Lickitung"] = ("normal", "#F0B8D0", "#F8D0E0"),
            ["Chansey"] = ("normal", "#FFE8F0", "#FFF0F8"),
            ["Tangela"] = ("grass", "#5078D8", "#6898E8"),
            ["Kangaskhan"] = ("normal", "#D0A880", "#E0C090"),
            ["Electabuzz"] = ("electric", "#F0DC40", "#F8E858"),
            ["Jynx"] = ("ice", "#C86898", "#E888B8"),
            ["Aerodactyl"] = ("rock", "#B898D0", "#C8A8E0"),
            ["Magmar"] = ("fire", "#F07040", "#F89058"),
            ["Ditto"] = ("normal", "#E8C0E8", "#F8D8F8"),
            ["Snorlax"] = ("normal", "#2E4A6E", "#485878"),
            ["Zapdos"] = ("electric", "#F8D030", "#F8E878"),
            ["Articuno"] = ("ice", "#78D8F8", "#F878A8"),
            ["Moltres"] = ("fire", "#F87830", "#F87898"),
            ["Mewtwo"] = ("psychic", "#A070C8", "#58C878"),
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

    /// <summary>Primary type for a National Dex display name (evolution stage or standalone).</summary>
    public static bool TryGetTypeKeyForDexName(string dexSpeciesName, out string typeKey)
    {
        foreach (var stages in EvolutionChains.Values)
        {
            foreach (var stage in stages)
            {
                if (stage.Name == dexSpeciesName)
                {
                    typeKey = stage.TypeKey;
                    return true;
                }
            }
        }

        if (StandaloneSpecies.TryGetValue(dexSpeciesName, out var standalone))
        {
            typeKey = standalone.TypeKey;
            return true;
        }

        typeKey = string.Empty;
        return false;
    }

    public static string ResolveRouteBarColor(string speciesRootName)
    {
        var chain = TryGetEvolutionChain(speciesRootName);
        if (chain is { Length: > 0 })
        {
            return chain[0].NormalColor;
        }

        if (StandaloneSpecies.TryGetValue(speciesRootName, out var standalone))
        {
            return standalone.NormalColor;
        }

        return TypeCatalog.NormalHexForTypeKey(PrimaryTypeKey(speciesRootName));
    }

    public static string ResolveTrainerBarColor(string typeKey) =>
        TypeCatalog.NormalHexForTypeKey(typeKey);

    /// <summary>Normal and shiny palette for a National Dex species name.</summary>
    public static bool TryGetColorsForDexName(string dexSpeciesName, out string normalColor, out string shinyColor)
    {
        foreach (var stages in EvolutionChains.Values)
        {
            foreach (var stage in stages)
            {
                if (stage.Name == dexSpeciesName)
                {
                    normalColor = stage.NormalColor;
                    shinyColor = stage.ShinyColor;
                    return true;
                }
            }
        }

        if (StandaloneSpecies.TryGetValue(dexSpeciesName, out var standalone))
        {
            normalColor = standalone.NormalColor;
            shinyColor = standalone.ShinyColor;
            return true;
        }

        normalColor = string.Empty;
        shinyColor = string.Empty;
        return false;
    }
}
