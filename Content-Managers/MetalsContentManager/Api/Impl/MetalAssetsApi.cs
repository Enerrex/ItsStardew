using System.Collections.Generic;
using MetalsContentManager.Internal.Services;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace MetalsContentManager.Api.Impl;

public class MetalsAssetsApi : IMetalsAssetsApi
{
    private readonly ITextureRegistry _registry;

    public MetalsAssetsApi(ITextureRegistry registry)
    {
        _registry = registry;
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