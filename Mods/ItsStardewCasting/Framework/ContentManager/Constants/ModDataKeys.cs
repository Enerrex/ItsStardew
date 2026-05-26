namespace ItsStardewContentManager.Framework.ContentManager.Constants;

public sealed class ModDataKeys
{
    private readonly KeyScope _scope;

    public ModDataKeys(string modId)
    {
        _scope = new KeyScope(modId);

    }

    public string InstalledMold => _scope["InstalledMold"];
    public string MachineState => _scope["MachineState"];
    public string ProcessingSecondsRemaining => _scope["ProcessingSecondsRemaining"];
}
