using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;

namespace ItsStardewContentManager.Framework.Patches.Dispatch.SDV.Object.Postfix.DrawWorld;

internal static class ObjectDrawWorldPatch
{
    public static ObjectDrawWorldChannel? Channel { get; set; }

    internal static void Postfix
    (
        SObject instance,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha
    )
    {
        Channel?.Dispatch
        (
            instance,
            spriteBatch,
            x,
            y,
            alpha
        );
    }
}