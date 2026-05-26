using HarmonyLib;
using ItsStardewCasting.Framework.Patches.Api;
using ItsStardewCasting.Framework.Patches.Util;

namespace ItsStardewCasting.Framework.Patches.Dispatch;

internal abstract class PatchChannelBase<T> : IPatchChannel
{
    protected readonly OrderedHandlerList<T> Handlers = new();

    public void Add(T handler, int order = 0)
    {
        Handlers.Add(handler, order);
    }

    public abstract void Apply(Harmony harmony);
}