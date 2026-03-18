using System.Text;

namespace ConsoleUtil;

class Program
{
    private static readonly HashSet<string> SkippedDirectories =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin",
            "obj",
            ".git",
            ".idea",
            
        };

    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: DirectoryTreePrinter <path-to-file-or-folder>");
            return 1;
        }

        string input_path = Path.GetFullPath(args[0]);

        string root_directory;
        if (File.Exists(input_path))
        {
            root_directory = Path.GetDirectoryName(input_path)!;
        }
        else if (Directory.Exists(input_path))
        {
            root_directory = input_path;
        }
        else
        {
            Console.WriteLine($"Path does not exist: {input_path}");
            return 1;
        }

        var sb = new StringBuilder();
        sb.AppendLine(new DirectoryInfo(root_directory).Name);

        PrintTree(root_directory, sb, "");

        Console.WriteLine(sb.ToString());
        return 0;
    }

    static void PrintTree(string directoryPath, StringBuilder sb, string indent)
    {
        DirectoryInfo dir;
        try
        {
            dir = new DirectoryInfo(directoryPath);
        }
        catch (Exception ex)
        {
            sb.AppendLine($"{indent}[Error reading directory: {ex.Message}]");
            return;
        }

        FileSystemInfo[] entries;
        try
        {
            entries = dir.GetFileSystemInfos()
                .Where(e => !ShouldSkip(e))
                .OrderBy(e => e is FileInfo) // directories first
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception ex)
        {
            sb.AppendLine($"{indent}[Access denied or error: {ex.Message}]");
            return;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            bool is_last = i == entries.Length - 1;
            string connector = is_last ? "└── " : "├── ";
            sb.AppendLine($"{indent}{connector}{entries[i].Name}");

            if (entries[i] is DirectoryInfo sub_dir)
            {
                string child_indent = indent + (is_last ? "    " : "│   ");
                PrintTree(sub_dir.FullName, sb, child_indent);
            }
        }
    }

    static bool ShouldSkip(FileSystemInfo entry)
    {
        return entry is DirectoryInfo dir
            && SkippedDirectories.Contains(dir.Name);
    }
}