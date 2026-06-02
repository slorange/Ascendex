using System;

namespace Ascendex.Game;

public static class ShinyRules
{
    public static double GetCatchRate(RunState state) =>
        GameBalance.Shinies.BaseCatchRate
        * (1.0 + state.ShinyCharmCount * GameBalance.Shinies.ShinyCharmRateMultiplierPerCharm);

    /// <summary>Rolls on first catch (level 0 → 1). Updates run shiny state and pending guarantees.</summary>
    public static void ApplyFirstCatchShinyRoll(RunState state, SpeciesProgress progress)
    {
        var speciesRoot = progress.SpeciesRootName;
        var hasLifetimeGuarantee = state.LifetimeShinySpeciesRoots.Contains(speciesRoot);
        var hasPendingGuarantee = state.PendingGuaranteedShinies > 0;
        var guaranteed = hasLifetimeGuarantee || hasPendingGuarantee;

        if (hasPendingGuarantee && !hasLifetimeGuarantee)
        {
            state.PendingGuaranteedShinies--;
        }

        var rollHit = Random.Shared.NextDouble() < GetCatchRate(state);
        var isShiny = guaranteed || rollHit;

        if (isShiny)
        {
            progress.IsShiny = true;
            state.LifetimeShinySpeciesRoots.Add(speciesRoot);
        }

        if (guaranteed && rollHit)
        {
            state.PendingGuaranteedShinies++;
        }
    }

    public static bool IsSpeciesShinyForPokedex(RunState state, string speciesRoot)
    {
        if (state.SpeciesByRoot.TryGetValue(speciesRoot, out var progress)
            && progress.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute)
        {
            return progress.IsShiny;
        }

        return false;
    }
}
