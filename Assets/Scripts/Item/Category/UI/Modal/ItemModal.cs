using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 用于记录背包位置与道具/装备/技能图标
/// </summary>
public static class ItemModal
{
    /// <summary>
    /// key为Grid的名字，value为对应的ItemUI
    /// </summary>
    public static Dictionary<string, ItemUI> modals = new Dictionary<string, ItemUI>();

    public static Dictionary<string, Sprite> itemIconModals = new Dictionary<string, Sprite>();

    public static void AddItemUI(string gridName, ItemUI itemUI)
    {
        if (modals.ContainsKey(gridName))
        {
            modals.Remove(gridName);
        }
        modals.Add(gridName, itemUI);
    }

    public static void RemoveItemUI(string gridName)
    {
        modals.Remove(gridName);
    }

    public static ItemUI GetItemUI(string gridName)
    {
        if (modals.ContainsKey(gridName))
        {
            return modals[gridName];
        }
        else
        {
            return null;
        }
    }

    public static void Clear()
    {
        modals.Clear();
    }

    public static void AddIconByPath(string path, Sprite icon)
    {
        if (itemIconModals.ContainsKey(path))
        {
            itemIconModals[path] = icon;
        }
        else
        {
            itemIconModals.Add(path, icon);
        }
    }
    public static void RemoveIconByPath(string path)
    {
        if (itemIconModals.ContainsKey(path))
        {
            itemIconModals.Remove(path);
        }
    }
    public static Sprite GetIconByPath(string path)
    {
        if (itemIconModals.ContainsKey(path))
        {
            return itemIconModals[path];
        }
        else
        {
            return null;
        }
    }
}
