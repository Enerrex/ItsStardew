using System.Collections.Generic;

namespace ItsStardewContentManager.Internal.Models;

internal sealed class PackAssetsFile
{
    public Dictionary<string, string> Textures { get; set; } = new();
}
