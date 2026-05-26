namespace ItsStardewCasting.Framework.ContentManager.Constants;

public readonly record struct KeyScope(string Prefix)
{
    public string this[string key] => $"{Prefix}/{key}";

    public KeyScope Child(string childPrefix) => new($"{Prefix}/{childPrefix}");
}