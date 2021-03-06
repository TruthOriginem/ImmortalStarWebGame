﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemPack {
    int GetPackId();
    string GetPackName();
    string GetPackDescription();
    string GetPackIconPath();
    lint GetWorthMoney();
    lint GetWorthDimen();
    Dictionary<string, lint> GetItemToAmounts();

    void SetPackLevel(int level);
    int GetPackLevel();
    bool IsPackHasLevel();
    bool IsFree();

    bool HaveAccessToReceive();
    bool CanBeRecievedNow();
}
public enum ItemPackCostType
{
    NORMAL,//ItemPack里道具合计
    FIXED,//无关道具本来花费
    DISCOUNT//根据道具合计打折
}