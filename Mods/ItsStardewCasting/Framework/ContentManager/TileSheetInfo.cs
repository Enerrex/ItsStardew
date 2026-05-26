namespace ItsStardewCasting.Framework.ContentManager;

public sealed record TileSheetInfo
(
    string TextureName,
    string AssetPath,
    int Width,
    int TileWidth,
    int Height,
    int TileHeight,
    int DrawScale = 4
);