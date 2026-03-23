using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace MetalsContentManager.Api.Interfaces;

public interface IMetalsAssetsApi
{
    Texture2D GetTexture(string role);
    bool TryGetTexture(string role, out Texture2D texture);

    IAssetName GetAssetName(string role);
    bool TryGetAssetName(string role, out IAssetName assetName);

    IReadOnlyCollection<string> GetAvailableRoles();
}