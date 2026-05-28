using System.Collections.Generic;
using ItsStardewContentManager.Api.Assets.Drawables;
using ItsStardewContentManager.Internal.Assets;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ItsStardewContentManager.Api.Interfaces;

public interface IContentManager
{
    TextureAsset GetTextureAsset(AssetRole role);
    bool TryGetTextureAsset(AssetRole role, out TextureAsset textureAsset);

    IReadOnlyCollection<AssetRole> GetAvailableRoles();
}
