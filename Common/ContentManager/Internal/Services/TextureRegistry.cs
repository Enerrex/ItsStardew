using System;
using System.Collections.Generic;
using System.Linq;
using ItsStardewContentManager.Api.Assets.Drawables;
using ItsStardewContentManager.Internal.Assets;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ItsStardewContentManager.Internal.Services;

internal sealed class TextureRegistry : ITextureRegistry
{
    private readonly Dictionary<AssetRole, TextureAsset> _textureAssets = new();

    private readonly Dictionary<string, string> _sourcePacks = new(StringComparer.OrdinalIgnoreCase);

    public void Register
    (
        TextureAsset asset
    )
    {
        AssetRole asset_role = asset.Role;
        if (string.IsNullOrWhiteSpace(asset_role))
        {
            throw new ArgumentException
            (
                "Asset role cannot be null or whitespace.",
                nameof(asset_role)
            );
        }

        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        if (!_textureAssets.TryAdd
            (
                asset_role,
                asset
            ))
        {
            string priorPack = _sourcePacks[asset_role];
            throw new InvalidOperationException
            (
                $"Duplicate asset role '{asset_role}'. Already registered " +
                $"by '{priorPack}', cannot also register from '{asset.SourcePackId}'."
            );
        }

        _sourcePacks[asset_role] = asset.SourcePackId;
    }

    public TextureAsset? GetTextureAsset(AssetRole role)
    {
        return _textureAssets.GetValueOrDefault(role);
    }

    public bool TryGetTextureAsset(AssetRole role, out TextureAsset asset)
    {
        return _textureAssets.TryGetValue
        (
            role,
            out asset
        );
    }

    public IReadOnlyCollection<AssetRole> GetAvailableRoles()
    {
        string KeySelector(AssetRole role) => role.Value;

        return _textureAssets.Keys
            .OrderBy(KeySelector, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}