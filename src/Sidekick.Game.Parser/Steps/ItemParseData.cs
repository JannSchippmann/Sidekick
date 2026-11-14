namespace Sidekick.Game.Parser.Steps;

/// <summary>
/// A piece of data on <see cref="Items.Item"/> that a parse step can provide or require.
/// Used only to order the parse steps; it is not stored on the item.
/// </summary>
public enum ItemParseData
{
    /// <summary>item.ItemClass.</summary>
    ItemClass,

    /// <summary>item.Properties.Rarity, as read from the item text.</summary>
    Rarity,

    /// <summary>item.Properties.MapArea.</summary>
    MapArea,

    /// <summary>item.Definition, item.InvariantDefinition, item.TradeItem, item.Type and item.Name.</summary>
    Definition,

    /// <summary>The properties read from the item text. Every property parse step provides it.</summary>
    Properties,

    /// <summary>item.Stats.</summary>
    Stats,

    /// <summary>The properties that are completed after the stats are parsed (fractured, desecrated, unique rarity from the definition, ...).</summary>
    PropertiesAfterStats,

    /// <summary>item.PseudoStats.</summary>
    PseudoStats,
}
