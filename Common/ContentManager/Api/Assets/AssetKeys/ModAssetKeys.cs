using ItsStardewContentManager.Internal.Assets;

namespace ItsStardewContentManager.Api.Assets.AssetKeys;

public sealed record class ModAssetKeys(string ModId)
{
    public string Texture(AssetRole role) => $"Mods/{ModId}/Textures/{role.Value}";
}