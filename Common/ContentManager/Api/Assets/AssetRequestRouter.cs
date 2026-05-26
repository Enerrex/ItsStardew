using ItsStardewContentManager.Internal.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ItsStardewContentManager.Api.Assets;

public sealed class AssetRequestRouter
{
    private readonly TextureRegistry _registry;
    private readonly IMonitor _monitor;

    public AssetRequestRouter(TextureRegistry registry, IMonitor monitor)
    {
        _registry = registry;
        _monitor = monitor;
    }

    public void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        foreach (TextureAsset asset in _registry.GetAllTextureAssets())
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(asset.PublicAssetPath))
                continue;

            e.LoadFrom
            (
                () => asset.Texture,
                AssetLoadPriority.Exclusive
            );

            return;
        }
    }
}