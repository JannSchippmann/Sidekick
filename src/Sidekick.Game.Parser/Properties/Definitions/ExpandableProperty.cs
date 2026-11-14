using Sidekick.Game.Parser.Filters.Types;
using Sidekick.Game.Parser.Items;
using Sidekick.Game.Parser.Steps;
namespace Sidekick.Game.Parser.Properties.Definitions;

public class ExpandableProperty
(
    string? label,
    params PropertyDefinition[] definitions
) : PropertyDefinition
{
    public List<PropertyDefinition> Definitions { get; } = definitions.ToList();

    public override string Label => label ?? string.Empty;

    public override ItemParseDependencies ParseDependencies => ItemParseDependencies.Combine(Definitions.Select(x => x.ParseDependencies));

    public override ItemParseDependencies ParseAfterStatsDependencies => ItemParseDependencies.Combine(Definitions.Select(x => x.ParseAfterStatsDependencies));

    public override void Parse(Item item)
    {
        foreach (var definition in Definitions)
        {
            definition.Parse(item);
        }
    }

    public override void ParseAfterStats(Item item)
    {
        foreach (var definition in Definitions)
        {
            definition.ParseAfterStats(item);
        }
    }

    public override async Task<TradeFilter?> GetFilter(Item item)
    {
        List<TradeFilter> filters = [];
        foreach (var definition in Definitions)
        {
            var definitionFilter = await definition.GetFilter(item);
            if (definitionFilter != null)
            {
                filters.Add(definitionFilter);
            }
        }

        if (filters.Count == 0) return null;

        return new ExpandableFilter(Label, false, filters.ToArray());
    }
}
