using ManifestTask.Loader.Models;
using ManifestTask.Renderer.Models;

namespace ManifestTask.Models;

internal static class ManifestInference
{
    public static RenderModel BuildModel(ManifestKindDefinition kind, ManifestInputs inputs)
    {
        var entry_dll = string.IsNullOrWhiteSpace(inputs.ManifestEntryDll)
                            ? inputs.TargetFileName
                            : inputs.ManifestEntryDll;

        string minimum_api_version = kind.Defaults.GetValueOrDefault
        (
            "MinimumApiVersion",
            ""
        );


        return new RenderModel
        {
            ManifestName = inputs.ManifestName,
            ManifestAuthor = inputs.ManifestAuthor,
            ManifestVersion = inputs.ManifestVersion,
            ManifestDescription = inputs.ManifestDescription,
            ManifestUniqueId = inputs.ManifestUniqueId,
            ManifestEntryDll = entry_dll,
            ManifestMinimumApiVersion = minimum_api_version,
            ManifestContentPackFor = inputs.ManifestContentPackFor
        };
    }
}