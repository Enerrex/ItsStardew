namespace ItsStardewCasting.Framework;

public sealed class ModKeys
{
    private readonly KeyScope _scope;

    public ModKeys(string modId)
    {
        _scope = new KeyScope(modId);

    }

    public string InstalledMold => _scope["InstalledMold"];
    public string TestingVisualState => _scope["TestingVisualState"];
    
}