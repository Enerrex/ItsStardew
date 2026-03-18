using System;
using StardewValley;

namespace Common.Strings;

/// <summary>
/// Stardew Valley qualified item ID helpers.
/// 
/// Examples:
///   SdvQualifiedId.Build(ItemKind.Object, "gold_ring")         -> "(O)gold_ring"
///   SdvQualifiedId.Object("gold_ring")                         -> "(O)gold_ring"
///   SdvQualifiedId.Build(ItemKind.BigCraftable, "CastingMachine")
/// </summary>
public static class SdvQualifiedId
{
    public enum ItemKind
    {
        Object,
        BigCraftable,
        Boots,
        Furniture,
        Hat,
        Mannequin,
        Pants,
        Shirt,
        Tool,
        Trinket,
        Wallpaper,
        Flooring,
        Weapon,
    }

    public static string Build(ItemKind kind, string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be null or empty.", nameof(itemId));

        return GetTypePrefix(kind) + itemId;
    }

    public static string GetTypePrefix(ItemKind kind)
    {
        return kind switch
        {
            ItemKind.Object => ItemRegistry.type_object,
            ItemKind.BigCraftable => ItemRegistry.type_bigCraftable,
            ItemKind.Boots => ItemRegistry.type_boots,
            ItemKind.Furniture => ItemRegistry.type_furniture,
            ItemKind.Hat => ItemRegistry.type_hat,
            ItemKind.Mannequin => ItemRegistry.type_mannequin,
            ItemKind.Pants => ItemRegistry.type_pants,
            ItemKind.Shirt => ItemRegistry.type_shirt,
            ItemKind.Tool => ItemRegistry.type_tool,
            ItemKind.Trinket => ItemRegistry.type_trinket,
            ItemKind.Wallpaper => ItemRegistry.type_wallpaper,
            ItemKind.Flooring => ItemRegistry.type_floorpaper,
            ItemKind.Weapon => ItemRegistry.type_weapon,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown item kind.")
        };
    }

    public static bool IsQualified(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        int closeParen = itemId.IndexOf(')');
        return itemId.Length >= 4
            && itemId[0] == '('
            && closeParen > 1;
    }

    public static string QualifyAsObjectIfNeeded(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be null or empty.", nameof(itemId));

        return IsQualified(itemId) ? itemId : Object(itemId);
    }

    public static bool TryParseTypePrefix(string qualifiedId, out ItemKind kind)
    {
        kind = default;

        if (!IsQualified(qualifiedId))
            return false;

        int closeParen = qualifiedId.IndexOf(')');
        string prefix = qualifiedId[..(closeParen + 1)];

        if (prefix == ItemRegistry.type_object)
        {
            kind = ItemKind.Object;
            return true;
        }

        if (prefix == ItemRegistry.type_bigCraftable)
        {
            kind = ItemKind.BigCraftable;
            return true;
        }

        if (prefix == ItemRegistry.type_boots)
        {
            kind = ItemKind.Boots;
            return true;
        }

        if (prefix == ItemRegistry.type_furniture)
        {
            kind = ItemKind.Furniture;
            return true;
        }

        if (prefix == ItemRegistry.type_hat)
        {
            kind = ItemKind.Hat;
            return true;
        }

        if (prefix == ItemRegistry.type_mannequin)
        {
            kind = ItemKind.Mannequin;
            return true;
        }

        if (prefix == ItemRegistry.type_pants)
        {
            kind = ItemKind.Pants;
            return true;
        }

        if (prefix == ItemRegistry.type_shirt)
        {
            kind = ItemKind.Shirt;
            return true;
        }

        if (prefix == ItemRegistry.type_tool)
        {
            kind = ItemKind.Tool;
            return true;
        }

        if (prefix == ItemRegistry.type_trinket)
        {
            kind = ItemKind.Trinket;
            return true;
        }

        if (prefix == ItemRegistry.type_wallpaper)
        {
            kind = ItemKind.Wallpaper;
            return true;
        }

        if (prefix == ItemRegistry.type_floorpaper)
        {
            kind = ItemKind.Flooring;
            return true;
        }

        if (prefix == ItemRegistry.type_weapon)
        {
            kind = ItemKind.Weapon;
            return true;
        }

        return false;
    }

    // Convenience methods
    public static string Object(string itemId) => Build(ItemKind.Object, itemId);
    public static string BigCraftable(string itemId) => Build(ItemKind.BigCraftable, itemId);
    public static string Boots(string itemId) => Build(ItemKind.Boots, itemId);
    public static string Furniture(string itemId) => Build(ItemKind.Furniture, itemId);
    public static string Hat(string itemId) => Build(ItemKind.Hat, itemId);
    public static string Mannequin(string itemId) => Build(ItemKind.Mannequin, itemId);
    public static string Pants(string itemId) => Build(ItemKind.Pants, itemId);
    public static string Shirt(string itemId) => Build(ItemKind.Shirt, itemId);
    public static string Tool(string itemId) => Build(ItemKind.Tool, itemId);
    public static string Trinket(string itemId) => Build(ItemKind.Trinket, itemId);
    public static string Wallpaper(string itemId) => Build(ItemKind.Wallpaper, itemId);
    public static string Flooring(string itemId) => Build(ItemKind.Flooring, itemId);
    public static string Weapon(string itemId) => Build(ItemKind.Weapon, itemId);
}