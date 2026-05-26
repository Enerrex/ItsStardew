using System.Text.Json;
using ManifestTask.Loader.Models;

namespace ManifestTask.Loader;

internal static class ManifestLoader
{
    public static ManifestKindsSchema LoadKinds(string path)
    {
        var json = File.ReadAllText(path);
        var schema = JsonSerializer.Deserialize<ManifestKindsSchema>
        (
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (schema is null)
        {
            throw new InvalidOperationException($"Unable to deserialize manifest kinds file '{path}'.");
        }

        return schema;
    }
}