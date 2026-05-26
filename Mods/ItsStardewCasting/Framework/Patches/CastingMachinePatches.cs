using HarmonyLib;
using ItsStardewContentManager.Framework.Constants;
using StardewValley;

namespace ItsStardewContentManager.Framework.Patches;

[HarmonyPatch
(
    typeof(Object),
    nameof(Object.performObjectDropInAction)
)]
internal static class CastingMachinePatches
{
    private static bool Prefix
    (
        Object instance,
        Item dropInItem,
        bool probe,
        Farmer who,
        bool returnFalseIfItemConsumed,
        ref bool result
    )
    {
        return !ModEntry.TryHandleCastingMachineDropIn
               (
                   instance,
                   dropInItem,
                   probe,
                   who,
                   returnFalseIfItemConsumed,
                   out result
               );
    }
}

[HarmonyPatch
(
    typeof(Object),
    nameof(Object.checkForAction)
)]
internal static class CastingMachineActionPatches
{
    private static bool Prefix
    (
        Object instance,
        Farmer who,
        bool justCheckingForActivity,
        ref bool result
    )
    {
        return !ModEntry.TryCollectReadyOutput
               (
                   instance,
                   who,
                   justCheckingForActivity,
                   out result
               );
    }
}

[HarmonyPatch
(
    typeof(Object),
    nameof(Object.placementAction)
)]
internal static class RingMoldPlacementPatches
{
    private static bool Prefix
    (
        Object instance,
        GameLocation location,
        int x,
        int y,
        Farmer who,
        ref bool result
    )
    {
        if (instance.QualifiedItemId != CastingItemIds.QualifiedRingMold)
        {
            return true;
        }

        result = false;
        return false;
    }
}