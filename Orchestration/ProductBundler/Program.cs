using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace ProductBundler;

internal static class Program
{
    private static readonly string[] GlobalExcludedSourceDirectories = ["bin", "obj"];
    private static readonly string[] GlobalExcludedSourceExtensions =
    [
        ".cs",
        ".csproj",
        ".dotsettings",
        ".props",
        ".targets",
        ".projitems",
        ".sln",
        ".slnx",
        ".user",
        ".suo",
        ".cache"
    ];
    private static readonly string[] GlobalExcludedBuildOutputSuffixes = [".zip"];
    private static readonly string[] ReleaseExcludedBuildOutputSuffixes = [".pdb", ".deps.json"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static int Main(string[] args)
    {
        try
        {
            string configuration = args.FirstOrDefault() is { Length: > 0 } arg ? arg : "Debug";
            bool includeDebugArtifacts = IsDebugConfiguration(configuration);
            string repoRoot = FindRepoRoot();
            string productsRoot = Path.Combine(repoRoot, "bin", "Products");
            string stagingRoot = Path.Combine(productsRoot, "_staging");

            Directory.CreateDirectory(productsRoot);
            DeleteDirectoryContents(productsRoot);
            Directory.CreateDirectory(stagingRoot);

            List<ProjectBundle> modRoots = LoadModRoots(repoRoot);
            List<ProjectBundle> contentPacks = LoadContentPacks(repoRoot);

            foreach (ProjectBundle modRoot in modRoots.Where(bundle => bundle.BundleEnabled))
            {
                string productName = modRoot.ProductName ?? modRoot.ProjectName;
                string productDisplayName = includeDebugArtifacts ? $"{productName} [DEBUG]" : productName;
                string productStageRoot = Path.Combine(stagingRoot, SanitizeFileName(productDisplayName));
                string productZipPath = Path.Combine(productsRoot, SanitizeFileName(productDisplayName) + ".zip");

                DeletePath(productStageRoot);
                DeletePath(productZipPath);
                Directory.CreateDirectory(productStageRoot);

                int memberCount = 0;
                memberCount += CopyProjectPayload(modRoot, configuration, includeDebugArtifacts, productStageRoot);

                foreach (ProjectBundle contentPack in contentPacks.Where(bundle =>
                    bundle.BundleEnabled &&
                    string.Equals(bundle.BundleFor, modRoot.UniqueId, StringComparison.OrdinalIgnoreCase)))
                {
                    memberCount += CopyProjectPayload(contentPack, configuration, includeDebugArtifacts, productStageRoot);
                }

                CreateZip(productStageRoot, productZipPath);
                DeletePath(productStageRoot);

                Console.WriteLine($"Created product zip: {productZipPath} ({memberCount} project payload(s))");
            }

            DeletePath(stagingRoot);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to build products.");
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int CopyProjectPayload(ProjectBundle project, string configuration, bool includeDebugArtifacts, string productStageRoot)
    {
        string destinationRoot = Path.Combine(productStageRoot, project.PayloadFolderName);

        CopyFilteredDirectory(project.ProjectDir, destinationRoot, ShouldIncludeSourceFile);

        if (project.IncludeBuildOutput)
        {
            string outputRoot = FindProjectOutputRoot(project, configuration);
            CopyFilteredDirectory(outputRoot, destinationRoot, relativePath => ShouldIncludeBuildOutputFile(relativePath, includeDebugArtifacts));
        }

        return 1;
    }

    private static List<ProjectBundle> LoadModRoots(string repoRoot)
    {
        string modsRoot = Path.Combine(repoRoot, "Mods");
        if (!Directory.Exists(modsRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(modsRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(csprojPath => LoadProjectBundle(repoRoot, csprojPath, isModRoot: true))
            .Where(bundle => bundle is not null)
            .Cast<ProjectBundle>()
            .ToList();
    }

    private static List<ProjectBundle> LoadContentPacks(string repoRoot)
    {
        string contentPacksRoot = Path.Combine(repoRoot, "Content-Packs");
        if (!Directory.Exists(contentPacksRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(contentPacksRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(csprojPath => LoadProjectBundle(repoRoot, csprojPath, isModRoot: false))
            .Where(bundle => bundle is not null)
            .Cast<ProjectBundle>()
            .ToList();
    }

    private static ProjectBundle? LoadProjectBundle(string repoRoot, string csprojPath, bool isModRoot)
    {
        string projectDir = Path.GetDirectoryName(csprojPath)!;
        string repoRelativePath = Path.GetRelativePath(repoRoot, projectDir);
        string projectName = Path.GetFileNameWithoutExtension(csprojPath);
        EvaluatedProjectProperties evaluated = GetEvaluatedProjectProperties(csprojPath);
        string? assemblyName = evaluated.AssemblyName ?? projectName;
        string? bundleName = evaluated.ProductBundleName;
        string? bundleEnabledText = evaluated.ProductBundleEnabled;
        bool bundleEnabled = !string.Equals(bundleEnabledText, "false", StringComparison.OrdinalIgnoreCase);
        string manifestPath = Path.Combine(projectDir, "manifest.json");

        if (!File.Exists(manifestPath))
        {
            return null;
        }

        ProjectManifest manifest = ReadManifest(manifestPath);
        string uniqueId = manifest.UniqueId ?? string.Empty;
        string? bundleFor = evaluated.ProductBundleFor;

        if (!isModRoot && string.IsNullOrWhiteSpace(bundleFor))
        {
            bundleFor = manifest.ContentPackForUniqueId;
        }

        if (isModRoot && string.IsNullOrWhiteSpace(bundleName))
        {
            bundleName = evaluated.ModFolderName;
        }

        if (string.IsNullOrWhiteSpace(bundleName))
        {
            bundleName = manifest.Name ?? projectName;
        }

        return new ProjectBundle(
            ProjectDir: projectDir,
            RepoRelativePath: repoRelativePath,
            ProjectName: projectName,
            AssemblyName: assemblyName ?? projectName,
            ProductName: bundleName,
            UniqueId: uniqueId,
            BundleFor: bundleFor,
            BundleEnabled: bundleEnabled,
            IncludeBuildOutput: isModRoot);
    }

    private static EvaluatedProjectProperties GetEvaluatedProjectProperties(string csprojPath)
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.ArgumentList.Add("msbuild");
        startInfo.ArgumentList.Add(csprojPath);
        startInfo.ArgumentList.Add("-nologo");
        startInfo.ArgumentList.Add("-getProperty:AssemblyName");
        startInfo.ArgumentList.Add("-getProperty:ModFolderName");
        startInfo.ArgumentList.Add("-getProperty:ProductBundleEnabled");
        startInfo.ArgumentList.Add("-getProperty:ProductBundleName");
        startInfo.ArgumentList.Add("-getProperty:ProductBundleFor");

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Could not start msbuild for '{csprojPath}'.");
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to evaluate properties for '{csprojPath}'.{Environment.NewLine}{stderr}");
        }

        using JsonDocument document = JsonDocument.Parse(stdout);
        JsonElement properties = document.RootElement.GetProperty("Properties");

        string? ReadProperty(string propertyName)
        {
            return properties.TryGetProperty(propertyName, out JsonElement valueElement) ? valueElement.GetString() : null;
        }

        return new EvaluatedProjectProperties(
            AssemblyName: ReadProperty("AssemblyName"),
            ModFolderName: ReadProperty("ModFolderName"),
            ProductBundleEnabled: ReadProperty("ProductBundleEnabled"),
            ProductBundleName: ReadProperty("ProductBundleName"),
            ProductBundleFor: ReadProperty("ProductBundleFor"));
    }

    private static ProjectManifest ReadManifest(string manifestPath)
    {
        using FileStream stream = File.OpenRead(manifestPath);
        ProjectManifest? manifest = JsonSerializer.Deserialize<ProjectManifest>(stream, JsonOptions);
        return manifest ?? new ProjectManifest();
    }

    private static string FindProjectOutputRoot(ProjectBundle project, string configuration)
    {
        string projectBinRoot = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "bin",
            project.ProjectGroup,
            project.ProjectName,
            configuration);

        projectBinRoot = Path.GetFullPath(projectBinRoot);

        if (!Directory.Exists(projectBinRoot))
        {
            throw new DirectoryNotFoundException($"Build output not found for {project.ProjectName} at '{projectBinRoot}'.");
        }

        string assemblyPattern = project.AssemblyName + ".dll";
        string? outputDll = Directory.EnumerateFiles(projectBinRoot, assemblyPattern, SearchOption.AllDirectories).FirstOrDefault();
        if (outputDll is null)
        {
            throw new FileNotFoundException($"Could not find '{assemblyPattern}' under '{projectBinRoot}'.");
        }

        return Path.GetDirectoryName(outputDll)!;
    }

    private static void CopyFilteredDirectory(string sourceRoot, string destinationRoot, Func<string, bool> includeFile)
    {
        if (!Directory.Exists(sourceRoot))
        {
            return;
        }

        foreach (string filePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceRoot, filePath);
            if (!includeFile(relativePath))
            {
                continue;
            }

            string destinationPath = Path.Combine(destinationRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(filePath, destinationPath, overwrite: true);
        }
    }

    private static bool ShouldIncludeSourceFile(string relativePath)
    {
        if (IsPathInAnyDirectory(relativePath, GlobalExcludedSourceDirectories))
        {
            return false;
        }

        string extension = Path.GetExtension(relativePath);
        return !GlobalExcludedSourceExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private static bool ShouldIncludeBuildOutputFile(string relativePath, bool includeDebugArtifacts)
    {
        string fileName = Path.GetFileName(relativePath);

        if (GlobalExcludedBuildOutputSuffixes.Any(suffix => fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!includeDebugArtifacts && ReleaseExcludedBuildOutputSuffixes.Any(suffix => fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    private static bool IsPathInAnyDirectory(string relativePath, IEnumerable<string> directoryNames)
    {
        string[] parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(part => directoryNames.Contains(part, StringComparer.OrdinalIgnoreCase));
    }

    private static void CreateZip(string sourceRoot, string destinationZipPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destinationZipPath)!);

        using FileStream zipStream = File.Create(destinationZipPath);
        using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);

        foreach (string filePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceRoot, filePath);
            ZipArchiveEntry entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
            using Stream entryStream = entry.Open();
            using FileStream sourceStream = File.OpenRead(filePath);
            sourceStream.CopyTo(entryStream);
        }
    }

    private static void DeletePath(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            return;
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static void DeleteDirectoryContents(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly))
        {
            DeletePath(filePath);
        }

        foreach (string childDirectory in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly))
        {
            DeletePath(childDirectory);
        }
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root.");
    }

    private static string SanitizeFileName(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value.Trim();
    }

    private static bool IsDebugConfiguration(string configuration)
    {
        return string.Equals(configuration, "Debug", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ProjectBundle(
        string ProjectDir,
        string RepoRelativePath,
        string ProjectName,
        string AssemblyName,
        string? ProductName,
        string UniqueId,
        string? BundleFor,
        bool BundleEnabled,
        bool IncludeBuildOutput)
    {
        public string ProjectGroup => RepoRelativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

        public string PayloadFolderName => SanitizeFileName(ProductName ?? ProjectName);
    }

    private sealed class ProjectManifest
    {
        public string? Name { get; set; }

        public string? UniqueId { get; set; }

        public ContentPackReference? ContentPackFor { get; set; }

        public sealed class ContentPackReference
        {
            public string? UniqueId { get; set; }
        }

        public string? ContentPackForUniqueId => ContentPackFor?.UniqueId;
    }

    private sealed record EvaluatedProjectProperties(
        string? AssemblyName,
        string? ModFolderName,
        string? ProductBundleEnabled,
        string? ProductBundleName,
        string? ProductBundleFor);
}
