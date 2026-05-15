using System.Collections.Generic;
using Avalonia.Media;

namespace Ascendex.ViewModels;

/// <summary>Primary (accent) colors by type key for UI such as the Pokédex grid.</summary>
public static class PokemonTypeBrushes
{
    private static readonly IReadOnlyDictionary<string, string> AccentHexByTypeKey =
        new Dictionary<string, string>
        {
            ["normal"] = "#B9B1A0",
            ["fighting"] = "#C95B49",
            ["flying"] = "#8FA6F2",
            ["poison"] = "#A668C7",
            ["ground"] = "#D0B05C",
            ["rock"] = "#B89F52",
            ["bug"] = "#99B63B",
            ["ghost"] = "#6E5AAE",
            ["fire"] = "#E6823D",
            ["water"] = "#5D8FE8",
            ["grass"] = "#5FB85B",
            ["electric"] = "#E7C63A",
            ["psychic"] = "#E96C9B",
            ["ice"] = "#7FD6D8",
            ["dragon"] = "#6D6AE6",
            ["fairy"] = "#E5A5D3",
        };

    public static IBrush AccentBrushForTypeKey(string typeKey)
    {
        if (AccentHexByTypeKey.TryGetValue(typeKey, out var hex))
        {
            return Brush.Parse(hex);
        }

        return Brushes.DimGray;
    }
}
