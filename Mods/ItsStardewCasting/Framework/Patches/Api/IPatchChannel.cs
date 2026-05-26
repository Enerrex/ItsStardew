using HarmonyLib;

namespace ItsStardewContentManager.Framework.Patches.Api;

internal interface IPatchChannel
{
    void Apply(Harmony harmony);
}