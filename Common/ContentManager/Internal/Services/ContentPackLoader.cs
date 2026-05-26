using System;
using ItsStardewContentManager.Internal.Assets;
using ItsStardewContentManager.Internal.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ItsStardewContentManager.Internal.Services;

internal sealed class ContentPackLoader
(
    IModHelper helper,
    IMonitor monitor,
    ITextureRegistry registry
)
{
    private readonly IModHelper _helper = helper;
    private readonly IMonitor _monitor = monitor;
    private readonly ITextureRegistry _registry = registry;

    public void LoadAllOwnedPacks()
    {
        foreach (IContentPack pack in _helper.ContentPacks.GetOwned())
        {
            LoadPack(pack);
        }
    }

    private void LoadPack(IContentPack pack)
    {
        const string METADATA_FILE = "assets.json";

        if (!pack.HasFile(METADATA_FILE))
        {
            _monitor.Log
            (
                $"Skipping content pack '{pack.Manifest.UniqueID}' because '{METADATA_FILE}' is missing.",
                LogLevel.Warn
            );
            return;
        }

        PackAssetsFile? metadata = pack.ReadJsonFile<PackAssetsFile>(METADATA_FILE);
        if (metadata is null)
        {
            _monitor.Log
            (
                $"Skipping content pack '{pack.Manifest.UniqueID}' because '{METADATA_FILE}' could not be parsed.",
                LogLevel.Error
            );
            return;
        }

        if (metadata.Textures.Count == 0)
        {
            _monitor.Log
            (
                $"Content pack '{pack.Manifest.UniqueID}' defines no textures in '{METADATA_FILE}'.",
                LogLevel.Warn
            );
            return;
        }

        foreach ((string role, string relativePath) in metadata.Textures)
        {
            RegisterTexture
            (
                pack,
                role,
                relativePath
            );
        }
    }

    private void RegisterTexture(IContentPack pack, string role, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            _monitor.Log
            (
                $"Pack '{pack.Manifest.UniqueID}' contains a blank asset role. Entry skipped.",
                LogLevel.Error
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            _monitor.Log
            (
                $"Pack '{pack.Manifest.UniqueID}' defines role '{role}' with a blank path. Entry skipped.",
                LogLevel.Error
            );
            return;
        }

        if (!pack.HasFile(relativePath))
        {
            _monitor.Log
            (
                $"Pack '{pack.Manifest.UniqueID}' defines role '{role}' with missing file '{relativePath}'.",
                LogLevel.Error
            );
            return;
        }

        try
        {
            Texture2D texture = pack.ModContent.Load<Texture2D>(relativePath);
            IAssetName internalAssetName = pack.ModContent.GetInternalAssetName(relativePath);

            AssetRole assetRole = new(role);
            string publicAssetPath = assetKeys.Texture(role);

            _registry.Register
            (
                new TextureAsset
                (
                    assetRole,
                    texture,
                    internalAssetName,
                    pack.Manifest.UniqueID,
                    publicAssetPath
                )
            );

            _monitor.Log($"Registered asset role '{role}' from '{pack.Manifest.UniqueID}' as '{assetName}'.");
        }
        catch (Exception ex)
        {
            _monitor.Log
            (
                $"Failed loading role '{role}' from pack '{pack.Manifest.UniqueID}' at '{relativePath}': {ex}",
                LogLevel.Error
            );
        }
    }
}