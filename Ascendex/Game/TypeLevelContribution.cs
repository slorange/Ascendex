namespace Ascendex.Game;

/// <summary>One type counter increment from a route Pokémon level-up.</summary>
public readonly record struct TypeLevelContribution(string TypeKey, int Points);

public static class TypeLevelContributionRules
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
