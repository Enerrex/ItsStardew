using System.Reflection;
using System.Security;
using System.Text;

namespace Writers
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                string repo_root = FindRepoRoot();
                string output_dir = Path.Combine
                (
                    repo_root,
                    "build"
                );
                string output_path = Path.Combine
                (
                    output_dir,
                    "Generated.ModIds.props"
                );

                Directory.CreateDirectory
                (
                    output_dir
                );

                Type mod_ids_type = typeof(ModIds.ModIds);

                FieldInfo[] fields = mod_ids_type.GetFields
                (
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                );

                List<GeneratedProperty> constants = fields.Where
                                                    (field => field.IsLiteral &&
                                                              !field.IsInitOnly &&
                                                              field.FieldType == typeof(string)
                                                    ).
                                                    OrderBy
                                                    (
                                                        field => field.Name,
                                                        StringComparer.Ordinal
                                                    ).
                                                    Select
                                                    (field => new GeneratedProperty
                                                        (
                                                            propertyName: $"UniqueId_{field.Name}",
                                                            value: (string?)field.GetRawConstantValue() ?? string.Empty
                                                        )
                                                    ).
                                                    ToList();

                if (constants.Count == 0)
                {
                    throw new InvalidOperationException
                    (
                        "No public const string fields were found on ModIds."
                    );
                }

                string xml = BuildPropsFile
                (
                    constants
                );

                File.WriteAllText
                (
                    output_path,
                    xml,
                    new UTF8Encoding
                    (
                        encoderShouldEmitUTF8Identifier: false
                    )
                );

                Console.WriteLine
                (
                    $"Wrote {output_path}"
                );
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine
                (
                    "Failed to generate Generated.ModIds.props."
                );
                Console.Error.WriteLine
                (
                    ex
                );
                return 1;
            }
        }

        private static string BuildPropsFile(IEnumerable<GeneratedProperty> properties)
        {
            var xml = new StringBuilder();
            xml.AppendLine
            (
                "<Project>"
            );
            xml.AppendLine
            (
                "  <PropertyGroup>"
            );

            foreach (GeneratedProperty property in properties)
            {
                xml.Append
                    (
                        "    <"
                    ).
                    Append
                    (
                        property.PropertyName
                    ).
                    Append
                    (
                        '>'
                    ).
                    Append
                    (
                        SecurityElement.Escape
                        (
                            property.Value
                        )
                    ).
                    Append
                    (
                        "</"
                    ).
                    Append
                    (
                        property.PropertyName
                    ).
                    AppendLine
                    (
                        ">"
                    );
            }

            xml.AppendLine
            (
                "  </PropertyGroup>"
            );
            xml.AppendLine
            (
                "</Project>"
            );

            return xml.ToString();
        }

        private static string FindRepoRoot()
        {
            DirectoryInfo? current = new DirectoryInfo
            (
                AppContext.BaseDirectory
            );

            while (current is not null)
            {
                string candidate = Path.Combine
                (
                    current.FullName,
                    "Directory.Build.props"
                );
                if (File.Exists
                    (
                        candidate
                    ))
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
            public GeneratedProperty(string propertyName, string value)
            {
                PropertyName = propertyName;
                Value = value;
            }

            public string PropertyName { get; }

            public string Value { get; }
        }
    }
}