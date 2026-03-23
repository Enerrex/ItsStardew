namespace ManifestTask.Models;

public sealed class ManifestInputs
{
    public string ManifestName { get; set; } = "";
    public string ManifestAuthor { get; set; } = "";
    public string ManifestVersion { get; set; } = "";
    public string ManifestDescription { get; set; } = "";
    public string ManifestUniqueId { get; set; } = "";
    public string ManifestEntryDll { get; set; } = "";
    public string TargetFileName { get; set; } = "";
    public string ManifestContentPackFor { get; set; } = "";
}