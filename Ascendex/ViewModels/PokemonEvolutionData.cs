using System.Collections.Generic;

namespace Ascendex.ViewModels;

/// <summary>One type counter increment from a route Pokémon level-up.</summary>
public readonly record struct TypeLevelContribution(string TypeKey, int Points);

/// <summary>
/// Species display stages by level. <see cref="MinLevel"/> is the first level at which that form is shown (inclusive).
/// <see cref="TypeKey"/> is the primary type (bar colors); <see cref="SecondaryTypeKey"/> is optional for dual typings and type counter splits.
/// Level-up evolutions use canonical Gen 1 RBY thresholds where applicable.
/// Stone substitutes: 25 for a single pre-evolution (e.g. Growlithe, Clefairy), 35 when evolving from a middle stage (e.g. Gloom, Nidorino).
/// Trade substitutes: final stage at 40 (e.g. Machamp, Alakazam, Golem, Gengar).
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

public static class PokemonEvolutionData
{
    private static readonly Dictionary<string, EvolutionStage[]> Chains = new()
    {
        ["Bulbasaur"] =
        [
            new(1, "Bulbasaur", "grass", "#78C86A", "#10210C", "poison"),
            new(16, "Ivysaur", "grass", "#5FAE52", "#081006", "poison"),
            new(32, "Venusaur", "grass", "#4E9448", "#071405", "poison"),
        ],
        ["Charmander"] =
        [
            new(1, "Charmander", "fire", "#E78A43", "#2B1202"),
            new(16, "Charmeleon", "fire", "#D96A2F", "#1E0C01"),
            new(36, "Charizard", "fire", "#F07828", "#2A0E02", "flying"),
        ],
        ["Squirtle"] =
        [
            new(1, "Squirtle", "water", "#73B9F2", "#071C2A"),
            new(16, "Wartortle", "water", "#5A9FD8", "#061525"),
            new(36, "Blastoise", "water", "#4A8BC8", "#050F1C"),
        ],
        ["Pikachu"] =
        [
            new(1, "Pikachu", "electric", "#F3D44F", "#2B2400"),
            new(25, "Raichu", "electric", "#F0C030", "#2A1F00"),
        ],
        ["Eevee"] =
        [
            new(1, "Eevee", "normal", "#C9A27A", "#24150A"),
            new(25, "Jolteon", "electric", "#F5D24A", "#2A2204"),
        ],
        ["Pidgey"] =
        [
            new(1, "Pidgey", "normal", "#D8C3A2", "#20150B", "flying"),
            new(18, "Pidgeotto", "normal", "#C4A882", "#1E1309", "flying"),
            new(36, "Pidgeot", "normal", "#B89268", "#1C1108", "flying"),
        ],
        ["Rattata"] =
        [
            new(1, "Rattata", "normal", "#B98AD3", "#130819"),
            new(20, "Raticate", "normal", "#A070C0", "#100615"),
        ],
        ["Spearow"] =
        [
            new(1, "Spearow", "normal", "#C68A58", "#201109", "flying"),
            new(20, "Fearow", "normal", "#B07848", "#1C0E07", "flying"),
        ],
        ["Nidoran♀"] =
        [
            new(1, "Nidoran♀", "poison", "#6CC6F8", "#06121A"),
            new(16, "Nidorina", "poison", "#5AA8D8", "#050F16"),
            new(35, "Nidoqueen", "ground", "#D0B05C", "#241904", "poison"),
        ],
        ["Nidoran♂"] =
        [
            new(1, "Nidoran♂", "poison", "#B98AD3", "#170A1E"),
            new(16, "Nidorino", "poison", "#9A70C0", "#120818"),
            new(35, "Nidoking", "ground", "#C9A067", "#211407", "poison"),
        ],
        ["Mankey"] =
        [
            new(1, "Mankey", "fighting", "#D7C1AE", "#1D1106"),
            new(28, "Primeape", "fighting", "#C5A898", "#1A0E05"),
        ],
        ["Caterpie"] =
        [
            new(1, "Caterpie", "bug", "#91D469", "#102009"),
            new(7, "Metapod", "bug", "#7AB852", "#0E1C07"),
            new(10, "Butterfree", "bug", "#8FA6F2", "#0F1430", "flying"),
        ],
        ["Weedle"] =
        [
            new(1, "Weedle", "bug", "#D5A14D", "#241304", "poison"),
            new(7, "Kakuna", "bug", "#C08E40", "#201003", "poison"),
            new(10, "Beedrill", "bug", "#A668C7", "#FFF5FF", "poison"),
        ],
        ["Ekans"] =
        [
            new(1, "Ekans", "poison", "#8B63B7", "#15091F"),
            new(22, "Arbok", "poison", "#7A5298", "#12071A"),
        ],
        ["Sandshrew"] =
        [
            new(1, "Sandshrew", "ground", "#D6B46E", "#241908"),
            new(22, "Sandslash", "ground", "#C4A25C", "#201507"),
        ],
        ["Jigglypuff"] =
        [
            new(1, "Jigglypuff", "fairy", "#F0A9D6", "#2B1020"),
            new(25, "Wigglytuff", "fairy", "#E88CC4", "#260E1C"),
        ],
        ["Magikarp"] =
        [
            new(1, "Magikarp", "water", "#E36B4E", "#2A0C05"),
            new(20, "Gyarados", "water", "#5D8FE8", "#071C2A", "flying"),
        ],
        ["Clefairy"] =
        [
            new(1, "Clefairy", "fairy", "#F4B8DE", "#2A1020"),
            new(25, "Clefable", "fairy", "#E8A0D0", "#250E1C"),
        ],
        ["Zubat"] =
        [
            new(1, "Zubat", "poison", "#6F74C9", "#F3F4FF", "flying"),
            new(22, "Golbat", "poison", "#5A5FB0", "#0E1028", "flying"),
        ],
        ["Paras"] =
        [
            new(1, "Paras", "bug", "#D4744B", "#2B1207", "grass"),
            new(24, "Parasect", "bug", "#C06040", "#260F06", "grass"),
        ],
        ["Oddish"] =
        [
            new(1, "Oddish", "grass", "#4F85D1", "#07192B", "poison"),
            new(21, "Gloom", "grass", "#5FB85B", "#081807", "poison"),
            new(35, "Vileplume", "grass", "#4A9448", "#061405", "poison"),
        ],
        ["Bellsprout"] =
        [
            new(1, "Bellsprout", "grass", "#A6D45C", "#102106", "poison"),
            new(21, "Weepinbell", "grass", "#8FC048", "#0E1C05", "poison"),
            new(35, "Victreebel", "grass", "#78A83C", "#0C1804", "poison"),
        ],
        ["Venonat"] =
        [
            new(1, "Venonat", "bug", "#A267BE", "#16091F"),
            new(31, "Venomoth", "bug", "#9A5CB0", "#14081C", "poison"),
        ],
        ["Abra"] =
        [
            new(1, "Abra", "psychic", "#E4C559", "#2A2106"),
            new(16, "Kadabra", "psychic", "#D4B548", "#261E05"),
            new(40, "Alakazam", "psychic", "#C4A538", "#221B04"),
        ],
        ["Vulpix"] =
        [
            new(1, "Vulpix", "fire", "#F3B16A", "#2A1404"),
            new(25, "Ninetales", "fire", "#E89A50", "#261003"),
        ],
        ["Meowth"] =
        [
            new(1, "Meowth", "normal", "#DCC584", "#241A08"),
            new(28, "Persian", "normal", "#C8B070", "#201607"),
        ],
        ["Growlithe"] =
        [
            new(1, "Growlithe", "fire", "#E88B49", "#2B1102"),
            new(25, "Arcanine", "fire", "#D87830", "#280E02"),
        ],
        ["Psyduck"] =
        [
            new(1, "Psyduck", "water", "#F1D25A", "#2A2103"),
            new(33, "Golduck", "water", "#5A9FD8", "#061525"),
        ],
        ["Poliwag"] =
        [
            new(1, "Poliwag", "water", "#7C9BE8", "#09122B"),
            new(25, "Poliwhirl", "water", "#6A88D8", "#080F24"),
            new(35, "Poliwrath", "water", "#5D8FE8", "#071C2A", "fighting"),
        ],
        ["Goldeen"] =
        [
            new(1, "Goldeen", "water", "#F39A74", "#2B1207"),
            new(33, "Seaking", "water", "#E88860", "#260F06"),
        ],
        ["Machop"] =
        [
            new(1, "Machop", "fighting", "#8BA4C8", "#0B1320"),
            new(28, "Machoke", "fighting", "#7890B0", "#09101C"),
            new(40, "Machamp", "fighting", "#657C98", "#070D18"),
        ],
        ["Geodude"] =
        [
            new(1, "Geodude", "rock", "#A68D63", "#211708", "ground"),
            new(25, "Graveler", "rock", "#8F7A52", "#1C1406", "ground"),
            new(40, "Golem", "rock", "#9A8060", "#1C1408", "ground"),
        ],
        ["Gastly"] =
        [
            new(1, "Gastly", "ghost", "#7B6AD0", "#F4F3FF", "poison"),
            new(25, "Haunter", "ghost", "#6A5AB8", "#0E0C24", "poison"),
            new(40, "Gengar", "ghost", "#5A4AA0", "#0C0A1E", "poison"),
        ],
        ["Cubone"] =
        [
            new(1, "Cubone", "ground", "#C7AF8F", "#21180B"),
            new(28, "Marowak", "ground", "#B89A78", "#1E1509"),
        ],
        ["Dratini"] =
        [
            new(1, "Dratini", "dragon", "#8C85EE", "#0E1030"),
            new(30, "Dragonair", "dragon", "#7A72D8", "#0C0E28"),
            new(55, "Dragonite", "dragon", "#6D6AE6", "#F4F5FF", "flying"),
        ],
        ["Diglett"] =
        [
            new(1, "Diglett", "ground", "#A46855", "#210E08"),
            new(26, "Dugtrio", "ground", "#925844", "#1D0C07"),
        ],
        ["Drowzee"] =
        [
            new(1, "Drowzee", "psychic", "#E1C95B", "#271F05"),
            new(26, "Hypno", "psychic", "#D0B848", "#241C04"),
        ],
        ["Ponyta"] =
        [
            new(1, "Ponyta", "fire", "#F28C54", "#2A1103"),
            new(40, "Rapidash", "fire", "#E87840", "#260E02"),
        ],
        ["Doduo"] =
        [
            new(1, "Doduo", "normal", "#C9A067", "#211407", "flying"),
            new(31, "Dodrio", "normal", "#B89052", "#1E1206", "flying"),
        ],
        ["Tentacool"] =
        [
            new(1, "Tentacool", "water", "#5FB4D5", "#072028", "poison"),
            new(30, "Tentacruel", "water", "#4A9EB8", "#061A20", "poison"),
        ],
        ["Slowpoke"] =
        [
            new(1, "Slowpoke", "water", "#E8A8BC", "#2A1020"),
            new(37, "Slowbro", "water", "#5D8FE8", "#071C2A", "psychic"),
        ],
        ["Horsea"] =
        [
            new(1, "Horsea", "water", "#5A8FE5", "#071A2A"),
            new(32, "Seadra", "water", "#4878D0", "#061523"),
        ],
        ["Staryu"] =
        [
            new(1, "Staryu", "water", "#C48E4D", "#241507"),
            new(25, "Starmie", "water", "#5D8FE8", "#071C2A", "psychic"),
        ],
        ["Exeggcute"] =
        [
            new(1, "Exeggcute", "grass", "#F0B5D2", "#2A0F1D"),
            new(25, "Exeggutor", "grass", "#5FB85B", "#081807", "psychic"),
        ],
        ["Rhyhorn"] =
        [
            new(1, "Rhyhorn", "ground", "#A78B71", "#22180D", "rock"),
            new(42, "Rhydon", "ground", "#8F7558", "#1E150B", "rock"),
        ],
        ["Magnemite"] =
        [
            new(1, "Magnemite", "electric", "#B6C4D7", "#10161D"),
            new(30, "Magneton", "electric", "#9EACBE", "#0E1218"),
        ],
        ["Voltorb"] =
        [
            new(1, "Voltorb", "electric", "#E65B5B", "#2A0808"),
            new(30, "Electrode", "electric", "#D44848", "#260606"),
        ],
        ["Seel"] =
        [
            new(1, "Seel", "water", "#D7EEF8", "#0C1C22"),
            new(34, "Dewgong", "water", "#D7EEF8", "#0C1C22", "ice"),
        ],
        ["Shellder"] =
        [
            new(1, "Shellder", "water", "#7766C8", "#F4F3FF"),
            new(25, "Cloyster", "water", "#7766C8", "#F4F3FF", "ice"),
        ],
        ["Krabby"] =
        [
            new(1, "Krabby", "water", "#E3714D", "#2A0C05"),
            new(28, "Kingler", "water", "#D06038", "#260A04"),
        ],
        ["Omanyte"] =
        [
            new(1, "Omanyte", "rock", "#7A92D6", "#09142A", "water"),
            new(40, "Omastar", "rock", "#6880C0", "#081122", "water"),
        ],
        ["Kabuto"] =
        [
            new(1, "Kabuto", "rock", "#8A7363", "#1E120C", "water"),
            new(40, "Kabutops", "rock", "#786250", "#1A100A", "water"),
        ],
        ["Koffing"] =
        [
            new(1, "Koffing", "poison", "#9162B2", "#14091C"),
            new(35, "Weezing", "poison", "#7E5298", "#110818"),
        ],
        ["Grimer"] =
        [
            new(1, "Grimer", "poison", "#7A5C97", "#F7F4FF"),
            new(38, "Muk", "poison", "#684A82", "#120A18"),
        ],
    };

    public static EvolutionStage[]? TryGetChain(string rootSpeciesName) =>
        Chains.TryGetValue(rootSpeciesName, out var stages) ? stages : null;
}
