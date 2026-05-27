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
    private readonly Dictionary<string, TextureAsset> _textureAssets = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, string> _sourcePacks = new(StringComparer.OrdinalIgnoreCase);

    public void Register
    (
        TextureAsset asset
    )
    {
        var asset_role = asset.Role;
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

    public IReadOnlyCollection<string> GetAvailableRoles()
    {
        string KeySelector(string p) => p;

        return _textureAssets.Keys.OrderBy
                         (
                             KeySelector,
                             StringComparer.OrdinalIgnoreCase
                         ).
                         ToArray();
    }
}