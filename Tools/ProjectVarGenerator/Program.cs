using System.Reflection;
using System.Security;
using System.Text;
using ProjectVarsGenerator.Constants;

namespace ProjectVarsGenerator
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                string repo_root = FindRepoRoot();
                string output_dir =
                    Path.Combine
                    (
                        repo_root,
                        ProjectPaths.GeneratedDir
                    );
                string output_path =
                    Path.Combine
                    (
                        output_dir,
                        "ManifestVariables.g.props"
                    );

                Directory.CreateDirectory(output_dir);

                Type unique_ids_type = typeof(UniqueIds.ProjectVars);

                List<GeneratedProperty> constants = [];
                CollectConstants
                (
                    unique_ids_type,
                    [],
                    constants
                );

                if (constants.Count == 0)
                {
                    throw new InvalidOperationException("No public const string fields were found on ModIds.");
                }

                string xml = BuildPropsFile(constants);

                File.WriteAllText
                (
                    output_path,
                    xml,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                );

                Console.WriteLine($"Wrote {output_path}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to generate Generated.ModIds.props.");
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static void CollectConstants(Type type, List<string> path, List<GeneratedProperty> constants)
        {
            List<string> current_path = [.. path, type.Name];

            FieldInfo[] fields =
                type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var string_fields =
                fields.Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string));

            var ordered_string_fields =
                string_fields.OrderBy
                (
                    field => field.Name,
                    StringComparer.Ordinal
                );

            constants.AddRange
            (
                ordered_string_fields.Select
                (field =>
                    new GeneratedProperty
                    (
                        path: [.. current_path, field.Name],
                        value: (string?)field.GetRawConstantValue() ?? string.Empty
                    )
                )
            );

            Type[] nested_types = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (Type nested_type in nested_types)
            {
                CollectConstants
                (
                    nested_type,
                    current_path,
                    constants
                );
            }
        }

        private static string BuildPropsFile(IEnumerable<GeneratedProperty> properties)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<Project>");
            xml.AppendLine("  <PropertyGroup>");

            foreach (GeneratedProperty property in properties)
            {
                string property_name =
                    string.Join
                    (
                        "_",
                        property.Path.Select(p => p.ToUpperInvariant())
                    );

                xml.Append
                        ("    <").
                    Append(property_name).
                    Append('>').
                    Append(SecurityElement.Escape(property.Value)).
                    Append("</").
                    Append(property_name).
                    AppendLine(">");
            }

            xml.AppendLine("  </PropertyGroup>");
            xml.AppendLine("</Project>");

            return xml.ToString();
        }

        private static string FindRepoRoot()
        {
            DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current is not null)
            {
                string candidate =
                    Path.Combine
                    (
                        current.FullName,
                        "Directory.Build.props"
                    );
                if (File.Exists(candidate))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new InvalidOperationException
            (
                "Could not locate repo root. Expected to find Directory.Build.props in this directory or a parent directory."
            );
        }

        private sealed class GeneratedProperty
        {
            public GeneratedProperty(IReadOnlyList<string> path, string value)
            {
                Path = path;
                Value = value;
            }

            public IReadOnlyList<string> Path { get; }

            public string Value { get; }
        }
    }
}