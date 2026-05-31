using System.Collections.Generic;
using System.Linq;
using Ascendex.Game.Content;

namespace Ascendex.Game;

public static class ProgressionRules
{
    private static readonly Dictionary<string, string[]> SpeciesRootNamesByRouteId =
        KantoRouteCatalog.All.ToDictionary(
            route => route.Id,
            route => route.Spawns.Select(spawn => spawn.SpeciesRootName).ToArray());

    public static bool IsStepComplete(RunState state, ProgressionStep step) =>
        step.Kind switch
        {
            ProgressionStepKind.Route => IsRouteComplete(state, step.TargetId),
            ProgressionStepKind.Trainer => IsTrainerComplete(state, step.TargetId),
            _ => false,
        };

    public static bool IsRouteUnlocked(RunState state, int orderIndex)
    {
        if (orderIndex == 0)
        {
            return true;
        }

        return IsStepComplete(state, KantoProgressionCatalog.Order[orderIndex - 1]);
    }

    public static bool IsRouteVisible(RunState state, string routeId)
    {
        for (var i = 0; i < KantoProgressionCatalog.Order.Length; i++)
        {
            var step = KantoProgressionCatalog.Order[i];
            if (step.Kind == ProgressionStepKind.Route && step.TargetId == routeId)
            {
                return IsRouteUnlocked(state, i);
            }
        }

        foreach (var optional in KantoProgressionCatalog.OptionalRouteUnlocks)
        {
            if (optional.RouteId == routeId)
            {
                return IsStepComplete(state, optional.UnlockWhen);
            }
        }

        return false;
    }

    public static bool IsTrainerVisible(RunState state, string trainerId)
    {
        for (var i = 0; i < KantoProgressionCatalog.Order.Length; i++)
        {
            var step = KantoProgressionCatalog.Order[i];
            if (step.Kind == ProgressionStepKind.Trainer && step.TargetId == trainerId)
            {
                return IsRouteUnlocked(state, i);
            }
        }

        return false;
    }

    private static bool IsRouteComplete(RunState state, string routeId)
    {
        if (!SpeciesRootNamesByRouteId.TryGetValue(routeId, out var speciesRootNames))
        {
            return false;
        }

        return speciesRootNames.Any(name =>
            state.SpeciesByRoot.TryGetValue(name, out var progress)
            && progress.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute);
    }

    private static bool IsTrainerComplete(RunState state, string trainerId) =>
        state.TrainersById.TryGetValue(trainerId, out var progress)
        && progress.Level >= GameBalance.Battles.MinTrainerLevelToRevealNextBattle;
}
