using Sidekick.Game.Parser.Items;
using Sidekick.Game.Parser.Steps;
using Xunit;
namespace Sidekick.Game.Parser.Tests.Steps;

public class ItemParseStepResolverTests
{
    private sealed class TestStep(string name, ItemParseData[] requires, ItemParseData[] provides) : IItemParseStep
    {
        public string Name => name;

        public ItemParseDependencies Dependencies { get; } = new(requires, provides);

        public void Parse(Item item) {}
    }

    private static List<string> Resolve(params TestStep[] steps) => ItemParseStepResolver.Resolve(steps).Select(x => x.Name).ToList();

    [Fact]
    public void StepsWithoutDependenciesKeepTheirOrder()
    {
        var order = Resolve(
            new TestStep("A", [], [ItemParseData.ItemClass]),
            new TestStep("B", [], [ItemParseData.Rarity]),
            new TestStep("C", [], [ItemParseData.MapArea]));

        Assert.Equal(["A", "B", "C"], order);
    }

    [Fact]
    public void ProvidersRunBeforeTheirConsumers()
    {
        // Same shape as the item parser: the definition is listed before the properties it needs.
        var order = Resolve(
            new TestStep("ItemClass", [], [ItemParseData.ItemClass]),
            new TestStep("Definition", [ItemParseData.Rarity, ItemParseData.MapArea, ItemParseData.ItemClass], [ItemParseData.Definition]),
            new TestStep("Quality", [ItemParseData.Rarity, ItemParseData.Definition], [ItemParseData.Properties]),
            new TestStep("Rarity", [], [ItemParseData.Rarity, ItemParseData.Properties]),
            new TestStep("MapArea", [], [ItemParseData.MapArea, ItemParseData.Properties]),
            new TestStep("Stats", [ItemParseData.Properties], [ItemParseData.Stats]));

        Assert.Equal(["ItemClass", "Rarity", "MapArea", "Definition", "Quality", "Stats"], order);
    }

    [Fact]
    public void ConsumersWaitForEveryProvider()
    {
        var order = Resolve(
            new TestStep("Stats", [ItemParseData.Properties], [ItemParseData.Stats]),
            new TestStep("First", [], [ItemParseData.Properties]),
            new TestStep("Second", [], [ItemParseData.Properties]));

        Assert.Equal(["First", "Second", "Stats"], order);
    }

    [Fact]
    public void RefiningStepRunsBetweenOtherProvidersAndConsumers()
    {
        var order = Resolve(
            new TestStep("Consumer", [ItemParseData.ItemClass], [ItemParseData.Properties]),
            new TestStep("Refiner", [ItemParseData.ItemClass], [ItemParseData.ItemClass]),
            new TestStep("Provider", [], [ItemParseData.ItemClass]));

        Assert.Equal(["Provider", "Refiner", "Consumer"], order);
    }

    [Fact]
    public void CircularDependencyThrows()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Resolve(
            new TestStep("Independent", [], [ItemParseData.ItemClass]),
            new TestStep("Definition", [ItemParseData.Rarity], [ItemParseData.Definition]),
            new TestStep("Rarity", [ItemParseData.Definition], [ItemParseData.Rarity])));

        Assert.StartsWith("Circular item parse step dependency:", exception.Message);
        Assert.Contains("Definition requires Rarity from Rarity", exception.Message);
        Assert.Contains("Rarity requires Definition from Definition", exception.Message);
        Assert.DoesNotContain("Independent", exception.Message);
    }

    [Fact]
    public void LongerCycleIsReported()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Resolve(
            new TestStep("A", [ItemParseData.Stats], [ItemParseData.Rarity]),
            new TestStep("B", [ItemParseData.Rarity], [ItemParseData.Definition]),
            new TestStep("C", [ItemParseData.Definition], [ItemParseData.Stats])));

        Assert.Contains("A requires Stats from C", exception.Message);
        Assert.Contains("C requires Definition from B", exception.Message);
        Assert.Contains("B requires Rarity from A", exception.Message);
    }

    [Fact]
    public void MissingProviderThrows()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Resolve(
            new TestStep("Pseudo", [ItemParseData.Stats], [ItemParseData.PseudoStats])));

        Assert.Equal("The item parse step Pseudo requires Stats, but no other step provides it.", exception.Message);
    }

    [Fact]
    public void RequiringOnlyYourOwnDataThrows()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Resolve(
            new TestStep("Refiner", [ItemParseData.ItemClass], [ItemParseData.ItemClass])));

        Assert.Contains("requires ItemClass, but no other step provides it", exception.Message);
    }
}
