using MetalsContentManager.Api;
using MetalsContentManager.Api.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ItsStardewCasting;

public sealed class ModEntry : Mod
{
    private IMetalsAssetsApi? assetsApi;
    private Texture2D? castingMachineSheet;
    private Texture2D? moldsOverlaySheet;
    private Texture2D? moldsInventorySheet;

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        assetsApi = Helper.ModRegistry.GetApi<IMetalsAssetsApi>("ItsStardew.MetalsContentManager");
        if (assetsApi is null)
        {
            Monitor.Log("MetalsContentManager API is unavailable.", LogLevel.Error);
            return;
        }

        castingMachineSheet = assetsApi.GetTexture(MetalsAssetRoles.CastingMachineBase);
        moldsOverlaySheet = assetsApi.GetTexture(MetalsAssetRoles.MoldsOverlay);
        moldsInventorySheet = assetsApi.GetTexture(MetalsAssetRoles.MoldsInventory);

        Monitor.Log("Metals asset API connected successfully.", LogLevel.Info);
    }
}