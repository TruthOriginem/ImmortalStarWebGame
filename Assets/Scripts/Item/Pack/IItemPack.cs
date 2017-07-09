using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemPack {
    string GetPackId();
    int GetWorthMoney();
    int GetWorthDimen();
    Dictionary<string, int> GetItemToAmounts();

    void SetPackLevel(int level);
    int GetPackLevel();
    int GetPackMaxLevel();

}
public enum ItemPackCostType
{
    NORMAL,//ItemPack里道具合计
    FIXED,//无关道具本来花费
    DISCOUNT//根据道具合计打折
}