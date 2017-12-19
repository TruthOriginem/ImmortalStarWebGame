using GameId;
using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemUseMethods
{
    /// <summary>
    /// 返回迭代器，根据道具id使用道具。
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="backToCategory"></param>
    /// <returns></returns>
    public static IEnumerator UseItem(string itemId, bool backToCategory = true)
    {
        //默认是消耗类道具
        bool shouldConsumed = true;
        //需要事先添加、减少的道具
        var itemBinding = new Dictionary<string, lint>();
        switch (itemId)
        {
            //上锁的芯片箱
            case Items.BOX_CHIP:
                {
                    var chip = ChipManager.GenerateRandomDataByTargetTier(3);
                    SyncRequest.AppendRequest(Requests.CHIP_HANDLER_DATA, chip);
                    break;
                }
            default:
                break;
        }
        //如果是消耗类，则消耗+1
        if (shouldConsumed)
        {
            itemBinding.Add(itemId, -1);
        }
        SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(itemBinding).ToJson());
        //更新啦，如果需要重新回到背包界面，那么将刷新它
        yield return RequestBundle.RequestSyncUpdate(false);
        if (backToCategory) CategoryManager.Instance.RequestLoad();
    }

}
