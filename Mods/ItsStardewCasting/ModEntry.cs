using System.Collections.Generic;
using HarmonyLib;
using ItsStardewCasting.Framework.ContentManager;
using ItsStardewCasting.Framework.ContentManager.Constants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;

namespace ItsStardewCasting;

public sealed class ModEntry : Mod
{
    private const string CastingMachineStateIdle = "idle";
    private const string CastingMachineStateWorking = "working";
    private const string CastingMachineStateReady = "ready";
    private const string GoldBarQualifiedItemId = "(O)336";
    private const string GoldRingQualifiedItemId = "(O)Gold_Ring_Artisan";
    private const int OverlayTileWidth = 16;
    private const int OverlayTileHeight = 32;
    private const float WorldDrawScale = 4f;
    private const int ProcessingSeconds = 10;

    private const int CastingMachineSpriteIndex = 0;
    private const int CastingMachineActiveSpriteIndex = 1;
    private const int RingMoldSpriteIndex = 1;

    private Harmony? _harmony;
    private CastingContentManager? _castingMachineContentManager;
    private Texture2D? _castingMachineSheet;
    private Texture2D? _moldsOverlaySheet;
    private Texture2D? _moldsInventorySheet;
    private static ModDataKeys? _modKeys;
    internal static string InstalledMoldKey { get; private set; } = string.Empty;

    public override void Entry(IModHelper helper)
    {
        _modKeys = new ModDataKeys(ModManifest.UniqueID);
        InstalledMoldKey = _modKeys.InstalledMold;

        _castingMachineContentManager = CastingContentManager.FromMod(this, helper);


        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Display.RenderedWorld += OnRenderedWorld;
        helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;

        _harmony = new Harmony(ModManifest.UniqueID);
        _harmony.PatchAll(typeof(ModEntry).Assembly);

        Monitor.Log
        (
            "Content manager initialized successfully.",
            LogLevel.Info
        );
    }

    internal static bool TryHandleCastingMachineDropIn
    (
        Object machine,
        Item dropInItem,
        bool probe,
        Farmer who,
        bool returnFalseIfItemConsumed,
        out bool result
    )
    {
        result = false;

        if (!IsCastingMachine(machine))
        {
            return false;
        }

        if (IsRingMold(dropInItem))
        {
            return HandleMoldInsert(machine, dropInItem, probe, who, returnFalseIfItemConsumed, out result);
        }

        if (IsGoldBar(dropInItem))
        {
            return HandleGoldBarInsert(machine, dropInItem, probe, who, returnFalseIfItemConsumed, out result);
        }

        return false;
    }

    internal static bool TryCollectReadyOutput(Object machine, Farmer who, bool justCheckingForActivity, out bool result)
    {
        result = false;

        if (!IsCastingMachine(machine) || GetMachineState(machine) != CastingMachineStateReady)
        {
            return false;
        }

        if (justCheckingForActivity)
        {
            result = true;
            return true;
        }

        Item output = ItemRegistry.Create(GoldRingQualifiedItemId);
        Item? leftover = who.addItemToInventory(output);
        if (leftover is not null)
        {
            Game1.createItemDebris(leftover, machine.TileLocation * 64f, -1, machine.Location);
        }

        ClearProcessing(machine);
        result = true;
        return true;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(CastingAssetNames.CastingMachineTexture))
        {
            RegisterTextureAsset
            (
                e,
                _castingMachineSheet,
                CastingAssetNames.CastingMachineTexture
            );
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo(CastingAssetNames.MoldsOverlayTexture))
        {
            RegisterTextureAsset
            (
                e,
                _moldsOverlaySheet,
                CastingAssetNames.MoldsOverlayTexture
            );
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo(CastingAssetNames.MoldsInventoryTexture))
        {
            RegisterTextureAsset
            (
                e,
                _moldsInventorySheet,
                CastingAssetNames.MoldsInventoryTexture
            );
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(InsertBigCraftableData);
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(InsertObjectData);
        }
    }

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (Game1.currentLocation is null || _moldsOverlaySheet is null)
        {
            return;
        }

        foreach (KeyValuePair<Vector2, Object> pair in Game1.currentLocation.objects.Pairs)
        {
            Object machine = pair.Value;
            if (!IsCastingMachine(machine))
            {
                continue;
            }

            DrawCastingMachineOverlay(e.SpriteBatch, machine);
        }
    }

    private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }

        foreach (GameLocation location in Game1.locations)
        {
            foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
            {
                Object machine = pair.Value;
                if (!IsCastingMachine(machine))
                {
                    continue;
                }

                TickMachine(machine);
            }
        }
    }

    private void RegisterTextureAsset(AssetRequestedEventArgs e, Texture2D? texture, string assetName)
    {
        if (texture is null)
        {
            Monitor.Log
            (
                $"Skipping asset '{assetName}' because the texture was not loaded.",
                LogLevel.Warn
            );
            return;
        }

        e.LoadFrom
        (
            () => texture,
            AssetLoadPriority.Exclusive
        );
    }

    private void DrawCastingMachineOverlay(SpriteBatch spriteBatch, Object machine)
    {
        if (_moldsOverlaySheet is null || _castingMachineSheet is null)
        {
            return;
        }

        Vector2 drawPosition = GetMachineDrawPosition(machine);

        if (IsMachineActive(machine))
        {
            Rectangle activeMachineRect = new(CastingMachineActiveSpriteIndex * OverlayTileWidth, 0, OverlayTileWidth, OverlayTileHeight);

            spriteBatch.Draw
            (
                _castingMachineSheet,
                drawPosition,
                activeMachineRect,
                Color.White,
                0f,
                Vector2.Zero,
                WorldDrawScale,
                SpriteEffects.None,
                1f
            );
        }

        int overlayIndex = GetOverlaySpriteIndex(machine);
        Rectangle sourceRect = new(overlayIndex * OverlayTileWidth, 0, OverlayTileWidth, OverlayTileHeight);
        
        spriteBatch.Draw
        (
            _moldsOverlaySheet,
            drawPosition,
            sourceRect,
            Color.White,
            0f,
            Vector2.Zero,
            WorldDrawScale,
            SpriteEffects.None,
            1f
        );
    }

    private static Vector2 GetMachineDrawPosition(Object machine)
    {
        Vector2 worldPosition =
            new
            (
                machine.TileLocation.X * 64f,
                (machine.TileLocation.Y - 1f) * 64f
            );
        return Game1.GlobalToLocal(worldPosition);
    }

    private static void InsertBigCraftableData(IAssetData asset)
    {
        IDictionary<string, BigCraftableData> data = asset.AsDictionary<string, BigCraftableData>().Data;

        data[CastingItemIds.CastingMachine] =
            new BigCraftableData
            {
                Name = "Casting Machine",
                DisplayName = "Casting Machine",
                Description = "A machine for casting metal bars into molds.",
                Price = 2500,
                CanBePlacedIndoors = true,
                CanBePlacedOutdoors = true,
                Texture = CastingAssetNames.CastingMachineTexture,
                SpriteIndex = CastingMachineSpriteIndex
            };
    }

    private static void InsertObjectData(IAssetData asset)
    {
        IDictionary<string, ObjectData> data = asset.AsDictionary<string, ObjectData>().Data;

            data[CastingItemIds.RingMold] =
            new ObjectData
            {
                Name = "Ring Mold",
                DisplayName = "Ring Mold",
                Description = "A mold used to cast a gold ring.",
                Type = "Basic",
                Category = -8,
                Price = 250,
                Texture = CastingAssetNames.MoldsInventoryTexture,
                SpriteIndex = RingMoldSpriteIndex
            };
    }

    private static bool IsCastingMachine(Item item)
    {
        return item.QualifiedItemId == CastingItemIds.QualifiedCastingMachine;
    }

    private static bool IsRingMold(Item item)
    {
        return item.QualifiedItemId == CastingItemIds.QualifiedRingMold;
    }

    private static bool IsGoldBar(Item item)
    {
        return item.QualifiedItemId == GoldBarQualifiedItemId;
    }

    private static bool HandleMoldInsert(Object machine, Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, out bool result)
    {
        result = false;

        if (machine.modData.ContainsKey(InstalledMoldKey))
        {
            return true;
        }

        if (probe)
        {
            result = true;
            return true;
        }

        machine.modData[InstalledMoldKey] = CastingItemIds.RingMold;
        machine.modData[_modKeys!.MachineState] = CastingMachineStateIdle;
        who.Items.ReduceId(dropInItem.QualifiedItemId, 1);
        result = !returnFalseIfItemConsumed;
        return true;
    }

    private static bool HandleGoldBarInsert(Object machine, Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, out bool result)
    {
        result = false;

        if (!machine.modData.TryGetValue(InstalledMoldKey, out string? installedMold) || installedMold != CastingItemIds.RingMold)
        {
            return true;
        }

        if (GetMachineState(machine) != CastingMachineStateIdle)
        {
            return true;
        }

        if (probe)
        {
            result = true;
            return true;
        }

        SetMachineState(machine, CastingMachineStateWorking);
        SetProcessingSecondsRemaining(machine, ProcessingSeconds);
        who.Items.ReduceId(dropInItem.QualifiedItemId, 1);
        result = !returnFalseIfItemConsumed;
        return true;
    }

    private void TickMachine(Object machine)
    {
        if (GetMachineState(machine) != CastingMachineStateWorking)
        {
            return;
        }

        int remaining = GetProcessingSecondsRemaining(machine);
        if (remaining <= 0)
        {
            SetMachineState(machine, CastingMachineStateReady);
            return;
        }

        remaining--;
        if (remaining <= 0)
        {
            SetProcessingSecondsRemaining(machine, 0);
            SetMachineState(machine, CastingMachineStateReady);
        }
        else
        {
            SetProcessingSecondsRemaining(machine, remaining);
        }
    }

    private static bool HandleReadyOutput(Object machine, Farmer who, bool justCheckingForActivity, out bool result)
    {
        result = false;

        if (GetMachineState(machine) != CastingMachineStateReady)
        {
            return false;
        }

        if (justCheckingForActivity)
        {
            result = true;
            return true;
        }

        Item output = ItemRegistry.Create(GoldRingQualifiedItemId);
        Item? leftover = who.addItemToInventory(output);
        if (leftover is not null)
        {
            Game1.createItemDebris(leftover, machine.TileLocation * 64f, -1, machine.Location);
        }

        ClearProcessing(machine);
        result = true;
        return true;
    }

    private static void ClearProcessing(Object machine)
    {
        machine.modData.Remove(_modKeys!.MachineState);
        machine.modData.Remove(_modKeys!.ProcessingSecondsRemaining);
    }

    private static void SetMachineState(Object machine, string state)
    {
        machine.modData[_modKeys!.MachineState] = state;
    }

    private static string GetMachineState(Object machine)
    {
        return machine.modData.TryGetValue(_modKeys!.MachineState, out string? state) ? state : CastingMachineStateIdle;
    }

    private static void SetProcessingSecondsRemaining(Object machine, int seconds)
    {
        machine.modData[_modKeys!.ProcessingSecondsRemaining] = seconds.ToString();
    }

    private static int GetProcessingSecondsRemaining(Object machine)
    {
        return machine.modData.TryGetValue(_modKeys!.ProcessingSecondsRemaining, out string? value) && int.TryParse(value, out int seconds)
            ? seconds
            : 0;
    }

    private static bool IsMachineActive(Object machine)
    {
        string state = GetMachineState(machine);
        return state is CastingMachineStateWorking or CastingMachineStateReady;
    }

    private int GetOverlaySpriteIndex(Object machine)
    {
        if (!machine.modData.TryGetValue
            (
                InstalledMoldKey,
                out string? installedMold
            ) ||
            string.IsNullOrWhiteSpace(installedMold))
        {
            return 0;
        }

        if (installedMold == CastingItemIds.RingMold)
        {
            return IsMachineActive(machine) ? 2 : 1;
        }

        return 0;
    }
}
