using StardewModdingAPI;

namespace ItsStardewCasting.Framework.ContentManager;

public class CastingContentManager
{
    private ItsStardewContentManager.ContentManager _contentManager;

    public CastingContentManager(ItsStardewContentManager.ContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    public static CastingContentManager FromMod(Mod mod, IModHelper helper)
    {
        var castingContentManager =
            new CastingContentManager
            (
                new ItsStardewContentManager.ContentManager
                (
                    helper,
                    mod.Monitor,
                    mod.ModManifest.UniqueID
                )
            );
        
        return castingContentManager;
    }
}