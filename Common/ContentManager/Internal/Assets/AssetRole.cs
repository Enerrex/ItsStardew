namespace ItsStardewContentManager.Internal.Assets;

public readonly record struct AssetRole(string Value)
{
    public override string ToString() => Value;

    public static implicit operator string(AssetRole role) => role.Value;
}