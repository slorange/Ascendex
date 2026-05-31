using System.Collections.Generic;

namespace Ascendex.Game.Content;

/// <summary>Primary type colors for Pokédex fills and bar fallbacks (Kanto-relevant types only).</summary>
public static class TypeCatalog
{
    private static readonly IReadOnlyDictionary<string, BarPalette> BarPaletteByTypeKey =
        new Dictionary<string, BarPalette>
        {
            ["normal"] = new("#B9B1A0", "#1E1A14"),
            ["fighting"] = new("#C95B49", "#FFF4F0"),
            ["flying"] = new("#8FA6F2", "#0F1430"),
            ["poison"] = new("#A668C7", "#FFF5FF"),
            ["ground"] = new("#D0B05C", "#241904"),
            ["rock"] = new("#B89F52", "#211807"),
            ["bug"] = new("#99B63B", "#152006"),
            ["ghost"] = new("#6E5AAE", "#F7F4FF"),
            ["fire"] = new("#E6823D", "#281003"),
            ["water"] = new("#5D8FE8", "#F4F8FF"),
            ["grass"] = new("#5FB85B", "#081807"),
            ["electric"] = new("#E7C63A", "#261E00"),
            ["psychic"] = new("#E96C9B", "#2A0713"),
            ["ice"] = new("#7FD6D8", "#082022"),
            ["dragon"] = new("#6D6AE6", "#F4F5FF"),
            ["fairy"] = new("#E5A5D3", "#2B1020"),
        };

    /// <summary>Types shown on the Routes tab type counter panel, in display order.</summary>
    public static readonly string[] CounterTypeKeys =
    [
        "normal", "fighting", "flying", "poison", "ground", "rock", "bug", "ghost",
        "fire", "water", "grass", "electric", "psychic", "ice", "dragon", "fairy",
    ];

    public static string AccentHexForTypeKey(string typeKey) =>
        BarPaletteByTypeKey.TryGetValue(typeKey, out var palette) ? palette.AccentColor : "#696969";

    public static BarPalette BarPaletteForTypeKey(string typeKey) =>
        BarPaletteByTypeKey.TryGetValue(typeKey, out var palette) ? palette : new BarPalette("#696969", "#1A1A1A");
}
