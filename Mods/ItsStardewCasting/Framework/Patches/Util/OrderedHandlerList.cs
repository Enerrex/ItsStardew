using System.Collections.Generic;
using System.Linq;

namespace ItsStardewCasting.Framework.Patches.Util;

internal sealed class OrderedHandlerList<T>
{
    private readonly List<(int Order, T Handler)> _handlers = [];

    public IEnumerable<T> Items => _handlers.Select(handler => handler.Handler);

    public void Add(T handler, int order = 0)
    {
        _handlers.Add((order, handler));
        _handlers.Sort((a, b) => a.Order.CompareTo(b.Order));
    }
}