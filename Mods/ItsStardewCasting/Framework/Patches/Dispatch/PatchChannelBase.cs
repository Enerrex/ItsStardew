using HarmonyLib;
using ItsStardewContentManager.Framework.Patches.Api;
using ItsStardewContentManager.Framework.Patches.Util;

namespace ItsStardewContentManager.Framework.Patches.Dispatch;

internal abstract class PatchChannelBase<T> : IPatchChannel
{
    protected readonly OrderedHandlerList<T> Handlers = new();

    public void Add(T handler, int order = 0)
    {
        Handlers.Add(handler, order);
    }

    public abstract void Apply(Harmony harmony);
}