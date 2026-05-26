using System.Collections.Generic;
using ItsStardewContentManager.Api.Interfaces;
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

    public Texture2D GetTexture(string role)
    {
        return _registry.GetTexture
        (
            role
        );
    }

    public bool TryGetTexture(string role, out Texture2D texture)
    {
        return _registry.TryGetTexture
        (
            role,
            out texture
        );
    }

    public IAssetName GetAssetName(string role)
    {
        return _registry.GetAssetName
        (
            role
        );
    }

    public bool TryGetAssetName(string role, out IAssetName assetName)
    {
        return _registry.TryGetAssetName
        (
            role,
            out assetName
        );
    }

    public IReadOnlyCollection<string> GetAvailableRoles() => _registry.GetAvailableRoles();
}
