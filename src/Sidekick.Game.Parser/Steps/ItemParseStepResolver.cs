namespace Sidekick.Game.Parser.Steps;

/// <summary>
/// Orders the item parse steps from their declared <see cref="ItemParseDependencies"/>, like
/// <see cref="Sidekick.Common.Initialization.InitializationOrderResolver"/> does for services.
/// Runs once at initialization, not per item.
/// </summary>
public static class ItemParseStepResolver
{
    /// <summary>
    /// Returns the steps so that each one runs after every other step that provides one of its requirements.
    /// Steps without a constraint between them keep the order they were given in.
    /// </summary>
    /// <exception cref="InvalidOperationException">A requirement has no provider, or the dependencies are circular.</exception>
    public static List<IItemParseStep> Resolve(IReadOnlyList<IItemParseStep> steps)
    {
        var providers = new Dictionary<ItemParseData, List<int>>();
        for (var index = 0; index < steps.Count; index++)
        {
            foreach (var data in steps[index].Dependencies.Provides.Distinct())
            {
                if (!providers.TryGetValue(data, out var list)) providers[data] = list = [];
                list.Add(index);
            }
        }

        var dependencies = new HashSet<int>[steps.Count];
        var dependents = new List<int>[steps.Count];
        for (var index = 0; index < steps.Count; index++) dependents[index] = [];

        for (var index = 0; index < steps.Count; index++)
        {
            dependencies[index] = [];
            foreach (var data in steps[index].Dependencies.Requires.Distinct())
            {
                var others = providers.GetValueOrDefault(data)?.Where(provider => provider != index).ToList() ?? [];
                if (others.Count == 0)
                {
                    throw new InvalidOperationException($"The item parse step {steps[index].Name} requires {data}, but no other step provides it.");
                }

                dependencies[index].UnionWith(others);
            }

            foreach (var dependency in dependencies[index]) dependents[dependency].Add(index);
        }

        // Kahn's algorithm. The queue is keyed by the original position, so the result stays as close to the given order as the dependencies allow.
        var pending = dependencies.Select(x => x.Count).ToArray();
        var ready = new PriorityQueue<int, int>();
        for (var index = 0; index < steps.Count; index++)
        {
            if (pending[index] == 0) ready.Enqueue(index, index);
        }

        var sorted = new List<IItemParseStep>(steps.Count);
        while (ready.TryDequeue(out var index, out _))
        {
            sorted.Add(steps[index]);
            foreach (var dependent in dependents[index])
            {
                if (--pending[dependent] == 0) ready.Enqueue(dependent, dependent);
            }
        }

        if (sorted.Count != steps.Count)
        {
            throw new InvalidOperationException($"Circular item parse step dependency: {DescribeCycle(steps, dependencies, pending)}");
        }

        return sorted;
    }

    private static string DescribeCycle(IReadOnlyList<IItemParseStep> steps, HashSet<int>[] dependencies, int[] pending)
    {
        // Every step left over waits on at least one other left-over step, so walking those edges must come back to a step already seen.
        var path = new List<int>();
        var current = Array.FindIndex(pending, x => x > 0);
        while (!path.Contains(current))
        {
            path.Add(current);
            current = dependencies[current].First(dependency => pending[dependency] > 0);
        }

        var cycle = path.Skip(path.IndexOf(current)).Append(current).ToList();
        var parts = new List<string>();
        for (var i = 0; i < cycle.Count - 1; i++)
        {
            var consumer = steps[cycle[i]];
            var provider = steps[cycle[i + 1]];
            var data = consumer.Dependencies.Requires.Intersect(provider.Dependencies.Provides);
            parts.Add($"{consumer.Name} requires {string.Join(", ", data)} from {provider.Name}");
        }

        return string.Join("; ", parts) + ".";
    }
}
