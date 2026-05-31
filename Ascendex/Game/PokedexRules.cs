using System.Collections.Generic;
using Ascendex.Game.Content;

namespace Ascendex.Game;

public readonly record struct PokedexCellFill(int CellIndex, string TypeKey);

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
                        yield return new PokedexCellFill(idx, stage.TypeKey);
                    }
                }

                continue;
            }

            if (!KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(speciesRoot, out var cellIndex))
            {
                continue;
            }

            yield return new PokedexCellFill(cellIndex, KantoSpeciesCatalog.PrimaryTypeKey(speciesRoot));
        }
    }
}
