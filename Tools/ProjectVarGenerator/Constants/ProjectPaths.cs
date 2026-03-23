namespace ProjectVarsGenerator.Constants;

public class ProjectPaths
{
    internal const string BUILD = "build";
    internal const string GENERATED = "generated";
    
    public static readonly string GeneratedDir = Path.Combine(BUILD, GENERATED);
}