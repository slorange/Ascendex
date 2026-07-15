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
            foreach (var fill in GetFilledCellsForRoot(state, speciesRoot))
            {
                yield return fill;
            }
        }
    }

    public static IEnumerable<PokedexCellFill> GetFilledCellsForRoot(RunState state, string speciesRoot)
    {
        if (!state.SpeciesByRoot.TryGetValue(speciesRoot, out var progress) || progress.Level < 1)
        {
            yield break;
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

            yield break;
        }

        if (KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(speciesRoot, out var cellIndex))
        {
            yield return new PokedexCellFill(cellIndex, ResolveFillColor(speciesRoot, isShiny), isShiny);
        }
    }

    public static IEnumerable<int> GetCellIndicesForRoot(string speciesRoot)
    {
        var chain = KantoSpeciesCatalog.TryGetEvolutionChain(speciesRoot);
        if (chain is { Length: > 0 })
        {
            foreach (var stage in chain)
            {
                if (KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(stage.Name, out var idx))
                {
                    yield return idx;
                }
            }

            yield break;
        }

        if (KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(speciesRoot, out var cellIndex))
        {
            yield return cellIndex;
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
