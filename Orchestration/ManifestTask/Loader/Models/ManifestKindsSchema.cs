namespace ManifestTask.Loader.Models;

public sealed class ManifestKindsSchema
{
    public Dictionary<string, ManifestKindDefinition> Kinds { get; set; } = new(StringComparer.Ordinal);

    public ManifestKindDefinition GetKind(string kind)
    {
        if (!Kinds.TryGetValue(kind, out var value))
        {
            throw new InvalidOperationException($"Unknown manifest kind '{kind}'.");
        }

        return value;
    }
}