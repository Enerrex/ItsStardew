using ManifestTask.Renderer.Models;

namespace ManifestTask.Loader.Models;

public sealed class ManifestKindDefinition
{
    public string Template { get; set; } = "";
    public List<string> Required { get; set; } = [];
    public Dictionary<string, string> Defaults { get; set; } = new(StringComparer.Ordinal);

    public IReadOnlyList<string> Validate(RenderModel model)
    {
        var errors = new List<string>();

        foreach (var field in Required)
        {
            var value = field switch
            {
                "Name" => model.ManifestName,
                "Author" => model.ManifestAuthor,
                "Version" => model.ManifestVersion,
                "Description" => model.ManifestDescription,
                "UniqueID" => model.ManifestUniqueId,
                "EntryDll" => model.ManifestEntryDll,
                "ContentPackFor" => model.ManifestContentPackFor,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"Manifest field '{field}' is required for manifest kind '{Template}'.");
        }

        return errors;
    }
}