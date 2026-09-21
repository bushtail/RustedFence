using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace RustedFence;

[Injectable(TypePriority = OnLoadOrder.Preload + 1), UsedImplicitly]
public class RustedFence(FenceConfig fenceConfig, ModHelper modHelper, ISptLogger<RustedFence> logger) : IOnLoad
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
    }
}
