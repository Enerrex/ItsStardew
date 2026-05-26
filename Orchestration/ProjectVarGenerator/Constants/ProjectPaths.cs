namespace ProjectVarsGenerator.Constants;

public class ProjectPaths
{
    internal const string Build = "build";
    internal const string Generated = "generated";
    
    public static readonly string GeneratedDir = Path.Combine(Build, Generated);
}