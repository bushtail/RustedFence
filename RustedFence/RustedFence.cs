using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace RustedFence;

[Injectable(TypePriority = OnLoadOrder.Preload + 1), UsedImplicitly]
public class RustedFence(FenceConfig fenceConfig, ModHelper modHelper, ISptLogger<RustedFence> logger, TemplateTable templateTable) : IOnLoad
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private const string ConfigFileName = "config.jsonc";
    
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var blacklist = fenceConfig.Blacklist;
        var modPath = modHelper.GetAbsolutePathToModFolder(assembly);

        if (!File.Exists(Path.Combine(modPath, ConfigFileName)))
        {
            await File.WriteAllTextAsync(Path.Combine(modPath, ConfigFileName), JsonSerializer.Serialize(new RustedFenceConfig(), _jsonSerializerOptions), cancellationToken);
        }

        var config = modHelper.GetJsonDataFromFile<RustedFenceConfig>(modPath, ConfigFileName);

        foreach (var item in config.RemoveFromBlacklist)
        {
            if (!blacklist.Remove(item))
            {
                logger.Warning($"Item with id { item } was not found in the blacklist, and is therefore not needed in your config.");
            }
            else
            {
                if (config.Debug)
                {
                    logger.Info($"Item with id { item } successfully removed from Fence's blacklist.");
                }
            }
        }
        
        if (config.IncreaseKeyCost && config.RemoveFromBlacklist.Contains(ItemTpl.KEY_RUSTED_BLOODY))
        {
            var handbook = templateTable.Handbook;

            foreach (var item in handbook.Items)
            {
                if (item.Id == ItemTpl.KEY_RUSTED_BLOODY)
                {
                    item.Price = 1239477;
                }
            }
        }
    }
}
