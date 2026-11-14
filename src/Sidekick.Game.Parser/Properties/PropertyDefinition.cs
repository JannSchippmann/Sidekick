using System.Globalization;
using System.Text.RegularExpressions;
using Sidekick.Game.Parser.Filters.Types;
using Sidekick.Game.Parser.Items;
using Sidekick.Game.Parser.Steps;
namespace Sidekick.Game.Parser.Properties;

public abstract class PropertyDefinition
{
    /// <summary>
    /// By default, properties are parsed once the item class, rarity and definition are known.
    /// </summary>
    public static ItemParseDependencies DefaultParseDependencies { get; } = new(
        [ItemParseData.ItemClass, ItemParseData.Rarity, ItemParseData.Definition],
        [ItemParseData.Properties]);

    /// <summary>
    /// By default, the after-stats parsing runs once the stats are parsed.
    /// </summary>
    public static ItemParseDependencies DefaultParseAfterStatsDependencies { get; } = new(
        [ItemParseData.Stats],
        [ItemParseData.PropertiesAfterStats]);

    public abstract string Label { get; }

    /// <summary>
    /// Orders <see cref="Parse"/> among the item parse steps. Override when the property is needed earlier, or needs other data.
    /// </summary>
    public virtual ItemParseDependencies ParseDependencies => DefaultParseDependencies;

    /// <summary>
    /// Orders <see cref="ParseAfterStats"/> among the item parse steps.
    /// </summary>
    public virtual ItemParseDependencies ParseAfterStatsDependencies => DefaultParseAfterStatsDependencies;

    public virtual void Parse(Item item) {}

    public virtual void ParseAfterStats(Item item) {}

    public virtual Task<TradeFilter?> GetFilter(Item item) { return Task.FromResult<TradeFilter?>(null); }

    protected static bool GetBool(Regex pattern, OriginalText originalText) => originalText.TryParseRegex(pattern, out _);

    protected static bool GetBool(Regex pattern, OriginalBlock originalBlock) => originalBlock.TryParseRegex(pattern, out _);

    protected static string? GetString(Regex pattern, OriginalText originalText)
    {
        return originalText.TryParseRegex(pattern, out var match) ? match.Groups[1].Value.Trim(' ', ':') : null;
    }

    protected static int GetInt(Regex pattern, OriginalText originalText)
    {
        if (!originalText.TryParseRegex(pattern, out var match)) return 0;

        return int.TryParse(match.Groups[1].Value, out var result) ? result : 0;
    }

    protected static int GetInt(Regex pattern, OriginalBlock originalBlock)
    {
        if (!originalBlock.TryParseRegex(pattern, out var match)) return 0;

        return int.TryParse(match.Groups[1].Value, out var result) ? result : 0;
    }

    protected static int GetInt(Regex pattern, string value)
    {
        var match = pattern.Match(value);
        if (!match.Success) return 0;

        return int.TryParse(match.Groups[1].Value, out var result) ? result : 0;
    }

    protected static double GetDouble(Regex pattern, OriginalText originalText)
    {
        if (!originalText.TryParseRegex(pattern, out var match)) return 0;

        var value = match.Groups[1].Value
            .Replace(",", ".")
            .TrimEnd('%');

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0;
    }
}
