using GameId;
using SerializedClassForJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPackManager : MonoBehaviour
{
    const string GET_BUNDLE_PATH = "scripts/player/item/itempack/getGiftPackDatas.php";
    static Dictionary<ItemPacks, ItemPack> packs = new Dictionary<ItemPacks, ItemPack>();
    static TempItemPackBundle bundle;
    public static ItemPackManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;
        packs.Add(ItemPacks.SIGN_IN, ItemPack.Generate(ItemPacks.SIGN_IN, "签到礼包"));
        packs.Add(ItemPacks.VIP_NORMAL, ItemPack.Generate(ItemPacks.VIP_NORMAL, "VIP每日礼包"));
    }
    public static ItemPack GetItemPack(ItemPacks idenum)
    {
        return packs.ContainsKey(idenum) ? packs[idenum] : null;
    }
    public static ItemPack GetItemPack(int id)
    {
        ItemPacks idenum = (ItemPacks)id;
        return packs.ContainsKey(idenum) ? packs[idenum] : null;
    }
    public static Coroutine RequestUpdateIPBundle()
    {
        return Instance.StartCoroutine(_GetIPBundle());
    }
    static IEnumerator _GetIPBundle()
    {
        WWWForm form = new WWWForm();
        form.AddField(Requests.ID, PlayerInfoInGame.Id);
        WWW w = new WWW(ConnectUtils.ParsePath(GET_BUNDLE_PATH), form);
        ConnectUtils.ShowConnectingUI();
        yield return w;
        ConnectUtils.HideConnectingUI();
        if (ConnectUtils.IsPostSucceed(w))
        {
            bundle = JsonUtility.FromJson<TempItemPackBundle>(w.text);
            ParseBundle();
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
        }
    }
    /// <summary>
    /// 解析Bundle，并对静态pack进行分析
    /// </summary>
    static void ParseBundle()
    {
        ItemPack pack;
        DateTime packDT;
        int packId;
        int diffDay;
        int packLevel;
        bool canRecieved;
        DateTime serverDT = DateTime.Parse(bundle.serverDateTime);
        DateTime serverNoSpecDT = serverDT.Date;
        //0 签到礼包
        pack = packs[ItemPacks.SIGN_IN];
        packId = pack.GetPackId();
        pack.SetAccess(true);
        packDT = GetIPLastRecieveTime(packId).Date;
        diffDay = (serverNoSpecDT - packDT).Days;
        canRecieved = diffDay > 0;
        pack.SetCanRecieve(canRecieved);
        //如果未领取，即要领取的是当前等级+1的，如果已领取，便是当前等级
        packLevel = GetIPRecieveTimes(packId) + (canRecieved ? 1 : 0);
        packLevel = packLevel > 7 ? 1 : packLevel;
        packLevel = diffDay < 2 ? packLevel : 1;
        pack.SetPackLevel(packLevel);
        //1 VIP普通礼包
        pack = packs[ItemPacks.VIP_NORMAL];
        packId = pack.GetPackId();
        packDT = GetIPLastRecieveTime(packId).Date;
        diffDay = (serverNoSpecDT - packDT).Days;
        canRecieved = diffDay > 0;
        pack.SetCanRecieve(canRecieved);
        packLevel = PlayerInfoInGame.VIP_Level;
        pack.SetAccess(packLevel > 0);
        pack.SetPackLevel(packLevel);
    }


    #region 处理服务器数据
    /// <summary>
    /// 获得当前该礼品包领取了几次。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static int GetIPRecieveTimes(int id)
    {
        if (id >= bundle.getTimes.Length)
        {
            return 0;
        }
        else
        {
            return bundle.getTimes[id];
        }
    }
    /// <summary>
    /// 获得指定礼包上一次领取的时间。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static DateTime GetIPLastRecieveTime(int id)
    {
        if (id >= bundle.getTimes.Length)
        {
            return DateTime.MinValue;
        }
        else
        {
            return DateTime.Parse(bundle.lastGetDateTime[id]);
        }
    }
    /// <summary>
    /// 上次从服务器取数据时服务器的时间。
    /// </summary>
    /// <returns></returns>
    public static string GetIPLatestServerTime()
    {
        return bundle.serverDateTime;
    }
    /// <summary>
    /// 获得旧服务器时间-指定礼包上次获取时间的TimeSpan
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static TimeSpan GetTimeSpanBetweenLastALastest(int id)
    {
        return DateTime.Parse(bundle.serverDateTime) - GetIPLastRecieveTime(id);
    }
    #endregion
}
namespace SerializedClassForJson
{
    /// <summary>
    /// 从服务器获得玩家的礼包获取状态。
    /// </summary>
    [System.Serializable]
    public class TempItemPackBundle
    {
        public int[] getTimes;
        public string[] lastGetDateTime;
        public string serverDateTime;
    }
    [System.Serializable]
    public class TempGPUpdate
    {
        public int targetId;
        public int targetLevel;
    }
}