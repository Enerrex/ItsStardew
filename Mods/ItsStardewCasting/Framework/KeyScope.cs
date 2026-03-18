namespace ItsStardewCasting.Framework;

public readonly record struct KeyScope(string Prefix)
{
    public string this[string key] => $"{Prefix}/{key}";

    public KeyScope Child(string childPrefix) => new($"{Prefix}/{childPrefix}");
}