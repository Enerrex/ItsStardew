using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace MetalsContentManager.Internal.Services;

internal sealed class TextureRegistry : ITextureRegistry
{
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, IAssetName> _assetNames = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, string> _sourcePacks = new(StringComparer.OrdinalIgnoreCase);

    public void Register
    (
        string role,
        Texture2D texture,
        IAssetName assetName,
        string sourcePackId
    )
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException
            (
                "Asset role cannot be null or whitespace.",
                nameof(role)
            );

        if (texture is null)
            throw new ArgumentNullException(nameof(texture));

        if (_textures.ContainsKey(role))
        {
            string priorPack = _sourcePacks[role];
            throw new InvalidOperationException
            (
                $"Duplicate asset role '{role}'. Already registered by '{priorPack}', cannot also register from '{sourcePackId}'."
            );
        }

        _textures[role] = texture;
        _assetNames[role] = assetName;
        _sourcePacks[role] = sourcePackId;
    }

    public Texture2D GetTexture(string role)
    {
        if (!TryGetTexture
            (
                role,
                out Texture2D texture
            ))
            throw new KeyNotFoundException($"No texture registered for asset role '{role}'.");

        return texture;
    }

    public bool TryGetTexture(string role, out Texture2D texture) =>
        _textures.TryGetValue
        (
            role,
            out texture!
        );

    public IAssetName GetAssetName(string role)
    {
        if (!TryGetAssetName
            (
                role,
                out IAssetName assetName
            ))
            throw new KeyNotFoundException($"No asset name registered for asset role '{role}'.");

        return assetName;
    }

    public bool TryGetAssetName(string role, out IAssetName assetName) =>
        _assetNames.TryGetValue
        (
            role,
            out assetName!
        );

    public IReadOnlyCollection<string> GetAvailableRoles() =>
        _textures.Keys.OrderBy
                  (
                      p => p,
                      StringComparer.OrdinalIgnoreCase
                  ).
                  ToArray();
}