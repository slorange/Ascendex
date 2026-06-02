using System.Collections.Generic;
using Ascendex.Game.Content;

namespace Ascendex.Game;

public readonly record struct PokedexCellFill(int CellIndex, string FillColorHex, bool IsShiny);

public static class PokedexRules
{
    public static IEnumerable<PokedexCellFill> GetFilledCells(RunState state)
    {
        foreach (var (speciesRoot, progress) in state.SpeciesByRoot)
        {
            if (progress.Level < 1)
            {
                continue;
            }

            var isShiny = progress.IsShiny;
            var chain = KantoSpeciesCatalog.TryGetEvolutionChain(speciesRoot);
            if (chain is { Length: > 0 })
            {
                foreach (var stage in chain)
                {
                    if (stage.MinLevel > progress.Level)
                    {
                        continue;
                    }

                    if (KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(stage.Name, out var idx))
                    {
                        yield return new PokedexCellFill(idx, ResolveFillColor(stage.Name, isShiny), isShiny);
                    }
                }

                continue;
            }

            if (!KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(speciesRoot, out var cellIndex))
            {
                continue;
            }

            yield return new PokedexCellFill(cellIndex, ResolveFillColor(speciesRoot, isShiny), isShiny);
        }
    }

    private static string ResolveFillColor(string dexSpeciesName, bool _)
    {
        if (KantoSpeciesCatalog.TryGetTypeKeyForDexName(dexSpeciesName, out var typeKey))
        {
            return TypeCatalog.NormalHexForTypeKey(typeKey);
        }

        return "#696969";
    }
}
