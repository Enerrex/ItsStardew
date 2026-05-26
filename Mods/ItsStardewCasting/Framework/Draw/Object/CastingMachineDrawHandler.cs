// using System.Collections.Generic;
// using HarmonyLib;
// using ItsStardewContentManager.Framework;
// using ItsStardewContentManager.Framework.Patches.Dispatch.SDV.Object.Postfix.DrawWorld;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using StardewModdingAPI;
// using StardewModdingAPI.Events;
// using StardewValley;
// using StardewValley.GameData.BigCraftables;
// using StardewValley.GameData.Objects;
// using SharedContentManager = ItsStardewContentManager.ContentManager;
// using SObject = StardewValley.Object;
//
// namespace ItsStardewContentManager.Framework.Draw.Object;
//
// public class CastingMachineDrawHandler : ObjectDrawWorldChannel.IHandler
// {
//     private const int SourceWidth = 16;
//     private const int SourceHeight = 32;
//     private const float Scale = 4f;
//     private const float OverlayLayerOffset = 0.000001f;
//
//     private readonly Texture2D _moldOverlayTexture;
//     private readonly string _castingMachineQualifiedItemId;
//
//     public CastingMachineDrawHandler(Texture2D moldOverlayTexture, string castingMachineQualifiedItemId)
//     {
//         _moldOverlayTexture = moldOverlayTexture;
//         _castingMachineQualifiedItemId = castingMachineQualifiedItemId;
//     }
//
//     public void DrawIfApplicable
//     (
//         SObject obj,
//         SpriteBatch spriteBatch,
//         int x,
//         int y,
//         float alpha
//     )
//     {
//         if (obj.QualifiedItemId != _castingMachineQualifiedItemId)
//         {
//             return;
//         }
//
//         if (moldsOverlaySheet is null || castingMachineSheet is null)
//         {
//             return;
//         }
//
//         Vector2 drawPosition = GetMachineDrawPosition(machine);
//
//         if (IsMachineActive(machine))
//         {
//             Rectangle activeMachineRect =
//                 new
//                 (
//                     CastingMachineActiveSpriteIndex * OverlayTileWidth,
//                     0,
//                     OverlayTileWidth,
//                     OverlayTileHeight
//                 );
//
//             spriteBatch.Draw
//             (
//                 castingMachineSheet,
//                 drawPosition,
//                 activeMachineRect,
//                 Color.White,
//                 0f,
//                 Vector2.Zero,
//                 WorldDrawScale,
//                 SpriteEffects.None,
//                 1f
//             );
//         }
//
//         int overlayIndex = GetOverlaySpriteIndex(machine);
//         Rectangle sourceRect =
//             new
//             (
//                 overlayIndex * OverlayTileWidth,
//                 0,
//                 OverlayTileWidth,
//                 OverlayTileHeight
//             );
//
//         spriteBatch.Draw
//         (
//             moldsOverlaySheet,
//             drawPosition,
//             sourceRect,
//             Color.White,
//             0f,
//             Vector2.Zero,
//             WorldDrawScale,
//             SpriteEffects.None,
//             1f
//         );
//     }
// }