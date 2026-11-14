using Sidekick.Game.Parser.Items;
using Sidekick.Game.Parser.Steps;
namespace Sidekick.Game.Parser.Properties;

/// <summary>
/// Runs <see cref="PropertyDefinition.Parse"/> of one property as an item parse step.
/// </summary>
public sealed class PropertyParseStep(PropertyDefinition definition) : IItemParseStep
{
    public string Name => $"{definition.GetType().Name}.{nameof(PropertyDefinition.Parse)}";

    public ItemParseDependencies Dependencies => definition.ParseDependencies;

    public void Parse(Item item) => definition.Parse(item);
}

/// <summary>
/// Runs <see cref="PropertyDefinition.ParseAfterStats"/> of one property as an item parse step.
/// </summary>
public sealed class PropertyParseAfterStatsStep(PropertyDefinition definition) : IItemParseStep
{
    public string Name => $"{definition.GetType().Name}.{nameof(PropertyDefinition.ParseAfterStats)}";

    public ItemParseDependencies Dependencies => definition.ParseAfterStatsDependencies;

    public void Parse(Item item) => definition.ParseAfterStats(item);
}
