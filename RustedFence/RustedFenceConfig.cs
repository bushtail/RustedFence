using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace RustedFence;

public class RustedFenceConfig
{
    public HashSet<MongoId> RemoveFromBlacklist { get; set; } = [ ItemTpl.KEY_RUSTED_BLOODY ];
    public bool Debug { get; set; } = false;
}
