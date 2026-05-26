using ItsStardewContentManager.Internal.Assets;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace ItsStardewContentManager.Api.Assets.Drawables;

public readonly record struct DrawableSprite
(
    AssetRole TextureRole,
    Rectangle SourceRect,
    Vector2 Origin,
    float Scale = 4f
);