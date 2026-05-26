namespace ItsStardewContentManager.Internal.Assets;

public readonly record struct KeyScope(string Prefix, string Separator="/")
{
    public string this[string key] => $"{Prefix}/{key}";

    public KeyScope ChildScope(string childPrefix) => new($"{Prefix}{Separator}{childPrefix}");
}