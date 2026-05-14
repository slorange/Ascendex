using System.Collections.Generic;

namespace Ascendex.ViewModels;

/// <summary>One type counter increment from a route Pokémon level-up.</summary>
public readonly record struct TypeLevelContribution(string TypeKey, int Points);

/// <summary>
/// Species display stages by bar level (starts at 0). <see cref="MinLevel"/> is the lowest level at which that form is shown (inclusive).
/// <see cref="TypeKey"/> is the primary type (bar colors); <see cref="SecondaryTypeKey"/> is optional for dual typings and type counter splits.
/// The first stage uses MinLevel 0; later stages use the same numeric thresholds as classic Gen 1 RBY levels (e.g. Ivysaur at bar 16).
/// Stone and trade stand-ins in the data follow those same level numbers.
/// <see cref="AccentColor"/> / <see cref="ForegroundColor"/> follow recognizable Sugimori-style palettes (not pure type chips).
/// </summary>
public readonly record struct EvolutionStage(
    int MinLevel,
    string Name,
    string TypeKey,
    string AccentColor,
    string ForegroundColor,
    string? SecondaryTypeKey = null);

public static class PokemonTypeContribution
{
    /// <summary>Mono-type gets all points; dual-type splits with the remainder on the primary type.</summary>
    public static TypeLevelContribution[] SplitTotal(string primaryTypeKey, string? secondaryTypeKey, int totalPoints)
    {
        if (string.IsNullOrEmpty(secondaryTypeKey))
        {
            return [new TypeLevelContribution(primaryTypeKey, totalPoints)];
        }

        var primaryShare = (totalPoints + 1) / 2;
        var secondaryShare = totalPoints / 2;
        return
        [
            new TypeLevelContribution(primaryTypeKey, primaryShare),
            new TypeLevelContribution(secondaryTypeKey, secondaryShare),
        ];
    }

    /// <summary>Same keys as <see cref="SplitTotal"/> but negated points (for reversing a prior split).</summary>
    public static TypeLevelContribution[] Negate(TypeLevelContribution[] contributions)
    {
        var result = new TypeLevelContribution[contributions.Length];
        for (var i = 0; i < contributions.Length; i++)
        {
            var c = contributions[i];
            result[i] = new TypeLevelContribution(c.TypeKey, -c.Points);
        }

        return result;
    }
}

/// <summary>Evolution-stage bar colors for route Pokémon. Covers every species in this dictionary (Kanto lines used in Ascendex), not the full 151 National Dex.</summary>
public static class PokemonEvolutionData
{
    private static readonly Dictionary<string, EvolutionStage[]> Chains = new()
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
            new(25, "Jolteon", "electric", "#F8E030", "#262004"),
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

    public static EvolutionStage[]? TryGetChain(string rootSpeciesName) =>
        Chains.TryGetValue(rootSpeciesName, out var stages) ? stages : null;
}
