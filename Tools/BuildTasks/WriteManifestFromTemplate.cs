using System.Text;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace BuildTasks;

public sealed class WriteManifestFromTemplate : Task
{
    [Required] public string TemplateFile { get; set; } = string.Empty;

    [Required] public string OutputFile { get; set; } = string.Empty;

    public ITaskItem[] Replacements { get; set; } = Array.Empty<ITaskItem>();

    public bool WriteOnlyWhenDifferent { get; set; } = true;

    public override bool Execute()
    {
        try
        {
            if (!File.Exists(TemplateFile))
            {
                Log.LogError($"Template file does not exist: '{TemplateFile}'.");
                return false;
            }

            string template_text = File.ReadAllText
            (
                TemplateFile,
                Encoding.UTF8
            );
            string rendered_text = template_text;

            foreach (ITaskItem replacement in Replacements)
            {
                string token = replacement.GetMetadata("Token");
                string value = replacement.GetMetadata("Value");

                if (string.IsNullOrWhiteSpace(token))
                {
                    Log.LogWarning
                        ($"Skipping replacement item '{replacement.ItemSpec}' because metadata 'Token' is empty.");
                    continue;
                }

                if (string.IsNullOrEmpty(value))
                {
                    Log.LogMessage
                    (
                        MessageImportance.Low,
                        $"Skipping token '{token}' because replacement value is empty."
                    );
                    continue;
                }

                rendered_text = rendered_text.Replace
                (
                    token,
                    value,
                    StringComparison.Ordinal
                );
            }

            string output_directory = Path.GetDirectoryName(OutputFile) ?? string.Empty;
            if (!string.IsNullOrEmpty(output_directory))
            {
                Directory.CreateDirectory(output_directory);
            }

            if (WriteOnlyWhenDifferent &&
                File.Exists(OutputFile) &&
                string.Equals
                (
                    File.ReadAllText
                    (
                        OutputFile,
                        Encoding.UTF8
                    ),
                    rendered_text,
                    StringComparison.Ordinal
                ))
            {
                Log.LogMessage
                (
                    MessageImportance.Low,
                    $"Manifest unchanged: '{OutputFile}'."
                );
                return true;
            }

            File.WriteAllText
            (
                OutputFile,
                rendered_text,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            );
            Log.LogMessage
            (
                MessageImportance.High,
                $"Wrote manifest: '{OutputFile}'."
            );

            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException
            (
                ex,
                showStackTrace: true,
                showDetail: true,
                file: null
            );
            return false;
        }
    }
}