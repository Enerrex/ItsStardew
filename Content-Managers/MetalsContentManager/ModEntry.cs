using System.Collections.Generic;
using MetalsContentManager.Api;
using MetalsContentManager.Api.Impl;
using MetalsContentManager.Internal.Services;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace MetalsContentManager;

public class ModEntry : Mod
{
    private ITextureRegistry registry = null!;

    public override void Entry(IModHelper helper)
    {
        registry = new TextureRegistry();

        ContentPackLoader loader = new
        (
            helper,
            Monitor,
            registry
        );
        loader.LoadAllOwnedPacks();

        Monitor.Log
        (
            $"Loaded {registry.GetAvailableRoles().Count} asset role(s) for {ModManifest.UniqueID}.",
            LogLevel.Info
        );
    }

    public override object GetApi()
    {
        return new MetalsAssetsApi
        (
            registry
        );
    }
}