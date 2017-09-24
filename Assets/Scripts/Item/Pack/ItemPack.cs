using GameId;
using System.Collections.Generic;
using System;

public partial class ItemPack : IItemPack
{
    ItemPacks packId;
    int packLevel = 0;
    int packMaxLevel = -1;
    lint worthDimen = 0;
    lint worthMoney = 0;
    string name;
    string description;
    string iconPath;
    bool canRecieved = false;
    bool haveAccessToRecieve = true;
    Dictionary<string, lint> itemToAmounts = new Dictionary<string, lint>();

    private ItemPack(ItemPacks id, string name, int level = 0)
    {
        packId = id;
        packLevel = level;
        this.name = name;
        InitById();
    }

    public static ItemPack Generate(ItemPacks id, string name)
    {
        return new ItemPack(id, name);
    }
    public ItemPack Generate(ItemPacks id, string name, int level)
    {
        return new ItemPack(id, name, level);
    }

    public int GetPackId()
    {
        return (int)packId;
    }

    public ItemPacks GetPackEnum()
    {
        return packId;
    }

    public int GetPackLevel()
    {
        return packLevel;
    }

    public int GetPackMaxLevel()
    {
        return packMaxLevel;
    }

    public lint GetWorthDimen()
    {
        return worthDimen;
    }

    public lint GetWorthMoney()
    {
        return worthMoney;
    }

    public bool IsPackHasLevel()
    {
        return packMaxLevel > -1;
    }

    public void SetPackLevel(int level)
    {
        packLevel = level;
    }

    public bool IsFree()
    {
        return worthMoney <= 0 && worthDimen <= 0;
    }

    public string GetPackName()
    {
        return name;
    }
    /// <summary>
    /// 设置是否满足领取条件。
    /// </summary>
    /// <param name="able"></param>
    public void SetCanRecieve(bool able)
    {
        canRecieved = able;
    }
    /// <summary>
    /// 设置领取权限。
    /// </summary>
    /// <param name="access"></param>
    public void SetAccess(bool access)
    {
        haveAccessToRecieve = access;
    }

    public bool CanBeRecievedNow()
    {
        return canRecieved;
    }

    public string GetPackDescription()
    {
        return description;
    }

    public string GetPackIconPath()
    {
        return iconPath;
    }

    public bool HaveAccessToReceive()
    {
        return haveAccessToRecieve ;
    }
}
