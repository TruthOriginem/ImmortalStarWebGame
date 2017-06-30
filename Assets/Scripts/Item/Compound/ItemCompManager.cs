using GameId;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCompManager : MonoBehaviour
{

    public static ItemCompManager Instance { get; set; }
    // Use this for initialization
    void Awake()
    {
        Instance = this;
    }
    public void InitOrRefresh(bool refreshPlayer)
    {
        StartCoroutine(RefreshCor(refreshPlayer));
    }
    IEnumerator RefreshCor(bool refreshPlayer)
    {
        if (refreshPlayer)
        {
            //更新
            yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        }
        List<ItemCompoundData> compDatas = new List<ItemCompoundData>();
        foreach (var kv in ItemDataManager.idsToItems)
        {
            var data = kv.Value.GetCompoundData();
            if (data != null)
            {
                compDatas.Add(data);
            }
        }
        AddSpecItemCompoundData(compDatas);
        compDatas.Sort(new CompComparer.BaseAmountComparer());
        ItemCompUIPool.Instance.UpdatePoolByList(compDatas);
    }
    /// <summary>
    /// 一些特定的合成公式
    /// </summary>
    /// <param name="datas"></param>
    void AddSpecItemCompoundData(List<ItemCompoundData> datas)
    {
        datas.Add(new ItemCompoundData(Items.CARD_EXP_DOUBLE, new[] { Items.STAR_ASH }, new[] { 3 }, 500));
        datas.Add(new ItemCompoundData(Items.CARD_DROP_DOUBLE, new[] { Items.DEEP_GOOD }, new[] { 1 }, 500));
        datas.Add(new ItemCompoundData(Items.MONEY_CHEST, null, null, 100000000));
        datas.Add(new ItemCompoundData("crystal_cuprum", new[] { "iron_fiery", "oil_low" }, new[] { 10, 6 }, 100));
        datas.Add(new ItemCompoundData("crystal_lasurite", new[] { "iron_azure", "oil_normal" }, new[] { 10, 2 }, 100));
    }
}
namespace CompComparer
{
    /// <summary>
    /// 根据道具可合成数量排序，如果相同则以
    /// </summary>
    public class BaseAmountComparer : IComparer<ItemCompoundData>
    {
        public int Compare(ItemCompoundData x, ItemCompoundData y)
        {
            int xa;
            bool xb = x.IfCanCompound(out xa);
            int ya;
            bool yb = y.IfCanCompound(out ya);
            if (xb && yb)
            {
                if (xa < ya)
                {
                    return 1;
                }
                else if (xa > ya)
                {
                    return -1;
                }
                else
                {
                    //此处需要改进
                    return x.ComedItemId.CompareTo(y.ComedItemId);
                }
            }
            else
            {
                if (xb)
                {
                    return -1;
                }
                else if (yb)
                {
                    return 1;
                }
                else
                {
                    return x.ComedItemId.CompareTo(y.ComedItemId);
                }
            }
        }
    }
}
