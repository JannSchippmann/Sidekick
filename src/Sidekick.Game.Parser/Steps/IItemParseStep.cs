using Sidekick.Game.Parser.Items;
namespace Sidekick.Game.Parser.Steps;

/// <summary>
/// One step of <see cref="ItemParser.ParseItem"/>. The steps are ordered once, at initialization, by <see cref="ItemParseStepResolver"/>.
/// </summary>
public interface IItemParseStep
{
    /// <summary>
    /// Shown in ordering errors.
    /// </summary>
    string Name => GetType().Name;

    /// <summary>
    /// The data this step reads and writes on the item. A step runs after every other step that provides one of its requirements.
    /// </summary>
    ItemParseDependencies Dependencies { get; }

    void Parse(Item item);
}

/// <param name="Requires">The data this step reads. A step that requires and provides the same data refines it: it runs after the other providers and before the consumers.</param>
/// <param name="Provides">The data this step writes.</param>
public sealed record ItemParseDependencies(
    IReadOnlyCollection<ItemParseData> Requires,
    IReadOnlyCollection<ItemParseData> Provides)
{
    public static ItemParseDependencies Combine(IEnumerable<ItemParseDependencies> dependencies)
    {
        var list = dependencies.ToList();
        return new ItemParseDependencies(
            list.SelectMany(x => x.Requires).Distinct().ToList(),
            list.SelectMany(x => x.Provides).Distinct().ToList());
    }
}
