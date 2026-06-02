using System.Collections.Generic;

namespace Ascendex.Game.Content;

/// <summary>Primary type colors for Pokédex fills and trainer bar fallbacks (Kanto-relevant types only).</summary>
public static class TypeCatalog
{
    private static readonly IReadOnlyDictionary<string, BarPalette> BarPaletteByTypeKey =
        new Dictionary<string, BarPalette>
        {
            ["normal"] = new("#B9B1A0"),
            ["fighting"] = new("#C95B49"),
            ["flying"] = new("#8FA6F2"),
            ["poison"] = new("#A668C7"),
            ["ground"] = new("#D0B05C"),
            ["rock"] = new("#B89F52"),
            ["bug"] = new("#99B63B"),
            ["ghost"] = new("#6E5AAE"),
            ["fire"] = new("#E6823D"),
            ["water"] = new("#5D8FE8"),
            ["grass"] = new("#5FB85B"),
            ["electric"] = new("#E7C63A"),
            ["psychic"] = new("#E96C9B"),
            ["ice"] = new("#7FD6D8"),
            ["dragon"] = new("#6D6AE6"),
            ["fairy"] = new("#E5A5D3"),
        };

    /// <summary>Types shown on the Routes tab type counter panel, in display order.</summary>
    public static readonly string[] CounterTypeKeys =
    [
        "normal", "fighting", "flying", "poison", "ground", "rock", "bug", "ghost",
        "fire", "water", "grass", "electric", "psychic", "ice", "dragon", "fairy",
    ];

    public static string NormalHexForTypeKey(string typeKey) =>
        BarPaletteByTypeKey.TryGetValue(typeKey, out var palette) ? palette.NormalColor : "#696969";

    public static BarPalette BarPaletteForTypeKey(string typeKey) =>
        BarPaletteByTypeKey.TryGetValue(typeKey, out var palette) ? palette : new BarPalette("#696969");
}
