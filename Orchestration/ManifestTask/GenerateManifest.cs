using System.Text;
using ManifestTask.Loader;
using ManifestTask.Models;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;


namespace ManifestTask;

public sealed class GenerateManifest : Task
{
    [Required] public string ProjectPath { get; set; } = "";
    [Required] public string ProjectDirectory { get; set; } = "";
    [Required] public string ProjectName { get; set; } = "";
    [Required] public string AssemblyName { get; set; } = "";
    [Required] public string TargetFileName { get; set; } = "";
    [Required] public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    [Required] public string Author { get; set; } = "";
    [Required] public string ManifestKind { get; set; } = "";
    [Required] public string ManifestName { get; set; } = "";
    [Required] public string ManifestUniqueId { get; set; } = "";
    public string ManifestEntryDll { get; set; } = "";
    [Required] public string OutputPath { get; set; } = "";
    [Required] public string TemplateRoot { get; set; } = "";
    [Required] public string ManifestKindsFile { get; set; } = "";
    public string ContentPackFor { get; set; } = "";

    public override bool Execute()
    {
        try
        {
            var schema = ManifestLoader.LoadKinds(ManifestKindsFile);
            var kind = schema.GetKind(ManifestKind);

            var template_path =
                Path.Combine
                (
                    TemplateRoot,
                    kind.Template
                );
            if (!File.Exists(template_path))
            {
                Log.LogError($"Manifest template not found: '{template_path}'.");
                return false;
            }

            var model =
                ManifestInference.BuildModel
                (
                    kind,
                    new ManifestInputs
                    {
                        ManifestName = ManifestName,
                        ManifestAuthor = Author,
                        ManifestVersion = Version,
                        ManifestDescription = Description,
                        ManifestUniqueId = ManifestUniqueId,
                        ManifestEntryDll = ManifestEntryDll,
                        TargetFileName = TargetFileName,
                        ManifestContentPackFor = ContentPackFor
                    }
                );

            var validation_errors = kind.Validate(model);
            foreach (var error in validation_errors)
            {
                Log.LogError(error);
            }

            if (Log.HasLoggedErrors)
            {
                return false;
            }

            var template =
                File.ReadAllText
                (
                    template_path,
                    Encoding.UTF8
                );
            var json =
                Renderer.Renderer.Render
                (
                    template,
                    model
                );

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);

            if (File.Exists(OutputPath))
            {
                var existing =
                    File.ReadAllText
                    (
                        OutputPath,
                        Encoding.UTF8
                    );
                if (StringComparer.Ordinal.Equals
                    (
                        existing,
                        json
                    ))
                {
                    Log.LogMessage
                    (
                        MessageImportance.Low,
                        $"Manifest unchanged: {OutputPath}"
                    );
                    return true;
                }
            }

            File.WriteAllText
            (
                OutputPath,
                json,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            );
            Log.LogMessage
            (
                MessageImportance.High,
                $"Generated manifest: {OutputPath}"
            );
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException
            (
                ex,
                showStackTrace: true
            );
            return false;
        }
    }
}