using System.Collections.Generic;
using ItsStardewContentManager.Api.Assets.Drawables;
using ItsStardewContentManager.Internal.Assets;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ItsStardewContentManager.Internal.Services;

public interface ITextureRegistry
{
    void Register(TextureAsset asset);
    public TextureAsset? GetTextureAsset(AssetRole role);
    public bool TryGetTextureAsset(AssetRole role, out TextureAsset asset);
    
    IReadOnlyCollection<AssetRole> GetAvailableRoles();
}
