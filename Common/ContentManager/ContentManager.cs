using System.Collections.Generic;
using ItsStardewContentManager.Api.Assets.Drawables;
using ItsStardewContentManager.Api.Interfaces;
using ItsStardewContentManager.Internal.Assets;
using ItsStardewContentManager.Internal.Services;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ItsStardewContentManager;

public sealed class ContentManager : IContentManager
{
    private readonly ITextureRegistry _registry;

    public ContentManager(IModHelper helper, IMonitor monitor, string ownerId)
    {
        _registry = new TextureRegistry();

        ContentPackLoader loader = new
        (
            helper,
            monitor,
            _registry
        );
        loader.LoadAllOwnedPacks();

        monitor.Log
        (
            $"Loaded {_registry.GetAvailableRoles().Count} asset role(s) for {ownerId}.",
            LogLevel.Info
        );
    }

    public TextureAsset GetTextureAsset(AssetRole role)
    {
        return _registry.GetTextureAsset(role)!;
    }

    public bool TryGetTextureAsset(AssetRole role, out TextureAsset textureAsset)
    {
        return _registry.TryGetTextureAsset(role, out textureAsset);
    }

    public IReadOnlyCollection<AssetRole> GetAvailableRoles() => _registry.GetAvailableRoles();
}
