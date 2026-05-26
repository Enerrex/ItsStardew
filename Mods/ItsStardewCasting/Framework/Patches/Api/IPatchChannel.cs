using HarmonyLib;

namespace ItsStardewCasting.Framework.Patches.Api;

internal interface IPatchChannel
{
    void Apply(Harmony harmony);
}