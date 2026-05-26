using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;

namespace ItsStardewContentManager.Framework.Patches.Dispatch.SDV.Object.Postfix.DrawWorld;



internal sealed class ObjectDrawWorldChannel : PatchChannelBase<ObjectDrawWorldChannel.IHandler>
{
    public interface IHandler
    {
        void DrawIfApplicable
        (
            SObject obj,
            SpriteBatch spriteBatch,
            int x,
            int y,
            float alpha
        );
    }
    
    public override void Apply(Harmony harmony)
    {
        harmony.Patch
        (
            original: AccessTools.Method
            (
                typeof(SObject),
                nameof(SObject.draw),
                [
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float)
                ]
            ),
            postfix: new HarmonyMethod
            (
                typeof(ObjectDrawWorldPatch),
                nameof(ObjectDrawWorldPatch.Postfix)
            )
        );
    }

    internal void Dispatch
    (
        SObject obj,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha
    )
    {
        foreach (var handler in Handlers.Items)
        {
            handler.DrawIfApplicable
            (
                obj,
                spriteBatch,
                x,
                y,
                alpha
            );
        }
    }
}