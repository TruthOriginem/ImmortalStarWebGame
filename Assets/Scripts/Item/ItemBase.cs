using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ItemBase
{
    public string item_id;
    public string name;
    /// <summary>
    /// 物品图标路径
    /// </summary>
    public string iconFileName;
    /// <summary>
    /// 物品描述
    /// </summary>
    public string description;
    public bool canBeSold;//是否可以出售
    public int indexInPack = -1;
    public int price;
    public int dimen;
    public int sort;
    public string[] compItems = null;
    public int[] compAmount = null;
    public int compPrice;

    private ItemCompoundData compData = null;
    private static string ITEM_PATH = "icons/items/";
    private static string ITEM_SUFFIX = ".png";


    public ItemBase(string id, string name, string dec, string icon, int price, int dimen)
    {
        this.item_id = id;
        this.name = name;
        this.iconFileName = icon;
        this.description = dec;
        this.price = price;
        this.dimen = dimen;
    }


    /// <summary>
    /// 生成并指定该道具的合成类，注意，若compItems为空则直接返回null。
    /// </summary>
    /// <returns></returns>
    public ItemCompoundData GenarateCompoundData()
    {
        if ((compItems[0] != "" & compAmount[0] != 0) || compPrice > 0)
        {
            compData = new ItemCompoundData(item_id, compItems, compAmount, compPrice);
            return compData;
        }
        else
        {
            compData = null;
            return null;
        }
    }
    /// <summary>
    /// 获得该道具的合成类，如果不存在则返回null。
    /// </summary>
    /// <returns></returns>
    public ItemCompoundData GetCompoundData()
    {
        return compData == null ? GenarateCompoundData() : compData;
    }
    public int GetIndexInPack()
    {
        return indexInPack;
    }
    public void SetIndexInPack(int index)
    {
        indexInPack = index;
    }

    /// <summary>
    /// 返回该道具的数量
    /// </summary>
    /// <returns></returns>
    virtual public int GetAmount()
    {
        return ItemDataManager.GetItemAmount(this);
    }
    virtual public bool CanBeSold()
    {
        return canBeSold;
    }

    public virtual string GetIconPath()
    {
        return ITEM_PATH + iconFileName + ITEM_SUFFIX;
    }


}
/// <summary>
/// 合成道具拥有的道具合成类。
/// </summary>
public class ItemCompoundData
{
    public string ComedItemId { get; set; }
    Dictionary<string, int> compNeeds = new Dictionary<string, int>();
    public int compPrice;
    bool noItems = false;
    public ItemCompoundData(string item, string[] needItems, int[] amounts, int price)
    {
        if (needItems != null)
        {
            for (int i = 0; i < needItems.Length; i++)
            {
                compNeeds.Add(needItems[i], amounts[i]);
                //Debug.Log(ComedItemId + ":" + items[i] + "/" + amounts[i]);
            }
        }
        else
        {
            noItems = true;
        }
        compPrice = price;
        ComedItemId = item;
    }
    public ItemCompoundData(string item, string needItem, int needAmount, int price) : this(item, new string[] { needItem }, new int[] { needAmount }, price)
    {
    }
    /// <summary>
    /// 直接访问玩家当前道具数量，并得出是否可合成和合成数量。
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool IfCanCompound(out int amount)
    {
        int now = int.MaxValue;
        amount = int.MaxValue;
        foreach (var kv in compNeeds)
        {
            var item_id = kv.Key;
            var need = kv.Value;
            var nowAmount = ItemDataManager.GetItemAmount(item_id);
            if (nowAmount - need >= 0)
            {
                now = nowAmount / need;
                if (now < amount)
                {
                    amount = now;
                }
            }
            else
            {
                amount = 0;
                break;
            }
        }
        if (compPrice > 0)
        {
            now = PlayerInfoInGame.GetMoney() / compPrice;
        }
        amount = now < amount ? now : amount;
        return amount != 0;
    }
    /// <summary>
    /// 返回指定合成数量需要的词典
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public Dictionary<string, lint> GetCompDict(int amount, out TempPlayerAttribute attr)
    {
        var result = new Dictionary<string, lint>();
        foreach (var kv in compNeeds)
        {
            result.Add(kv.Key, -kv.Value * amount);
        }
        result.Add(ComedItemId, amount);
        attr = new TempPlayerAttribute();
        attr.money = 0 - compPrice * amount;
        return result;
    }
    public Dictionary<string, int> GetNeedsDict()
    {
        return compNeeds;
    }

}