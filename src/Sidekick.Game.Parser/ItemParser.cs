using Microsoft.Extensions.Logging;
using Sidekick.Common.Exceptions;
using Sidekick.Common.Initialization;
using Sidekick.Common.Settings;
using Sidekick.Game.Parser.Items;
using Sidekick.Game.Parser.Properties;
using Sidekick.Game.Parser.Steps;
namespace Sidekick.Game.Parser;

public class ItemParser
(
    ILogger<ItemParser> logger,
    StatParser statParser,
    PseudoParser pseudoParser,
    PropertyParser propertyParser,
    ItemDefinitionParser itemDefinitionParser,
    ISettingsService settingsService,
    ItemClassParser itemClassParser,
    TextParser textParser
) : IInitializableService
{
    private GameType Game { get; set; }

    /// <summary>
    /// The parse steps in dependency order. Resolved once per initialization; the property definitions are recreated by
    /// <see cref="PropertyParser.Initialize"/>, which runs before this service because it is a constructor dependency.
    /// </summary>
    private List<IItemParseStep> Steps { get; set; } = [];

    public async Task Initialize()
    {
        Game = await settingsService.GetGame();

        // Listed in the historical call order. The resolver only moves a step when a dependency requires it.
        Steps = ItemParseStepResolver.Resolve([
            itemClassParser,
            itemDefinitionParser,
            ..propertyParser.GetParseSteps(),
            statParser,
            ..propertyParser.GetParseAfterStatsSteps(),
            pseudoParser,
        ]);
    }

    public Item ParseItem(string? text)
    {
        if (string.IsNullOrEmpty(text)) throw new UnparsableException(text);

        try
        {
            var rawText = textParser.NormalizeText(text);
            if (rawText == null) throw new UnparsableException(text);

            var item = new Item(Game, rawText);

            foreach (var step in Steps)
            {
                step.Parse(item);
            }

            return item;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Could not parse item.");
            throw new UnparsableException(text);
        }
    }
}
