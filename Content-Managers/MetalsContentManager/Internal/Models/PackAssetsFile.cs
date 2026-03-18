using System.Collections.Generic;

namespace MetalsContentManager.Internal.Models;

internal sealed class PackAssetsFile
{
    public Dictionary<string, string> Textures { get; set; } = new();
}