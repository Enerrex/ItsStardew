using ItsStardewContentManager.Internal.Assets;
using Microsoft.Xna.Framework;

namespace ItsStardewContentManager.Api.Assets.Drawables;

public readonly record struct DrawableTileSprite
(
    AssetRole TextureRole,
    int TileIndex,
    int TileWidth,
    int TileHeight,
    Vector2 Origin,
    float Scale = 4f
)
{
    public Rectangle GetSourceRect(int textureWidth)
    {
        int columns = textureWidth / TileWidth;

        int x = TileIndex % columns;
        int y = TileIndex / columns;

        return new Rectangle
        (
            x * TileWidth,
            y * TileHeight,
            TileWidth,
            TileHeight
        );
    }
}