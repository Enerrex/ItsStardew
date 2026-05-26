using ItsStardewContentManager.Internal.Assets;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ItsStardewContentManager.Api.Assets.Drawables;

public sealed record TextureAsset
(
    AssetRole Role,
    Texture2D Texture,
    IAssetName InternalAssetName,
    string SourcePackId,
    string PublicAssetPath
);