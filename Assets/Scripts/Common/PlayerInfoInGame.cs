using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SerializedClassForJson;
using GameId;

/// <summary>
/// 备注：
/// 需要审核玩家是否连线判定的地方：
/// 获得新物品时，刷新物品时，获得人物属性时
/// </summary>
public class PlayerInfoInGame : MonoBehaviour
{
    #region 相关变量
    /// <summary>
    /// 玩家装备数量的最大值
    /// </summary>
    public static int MAX_EQUIPMENT_IN_PACK = 30;

    /// <summary>
    /// 玩家对应id
    /// </summary>
    public static string Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
        }
    }
    /// <summary>
    /// 玩家对应用户名
    /// </summary>
    public static string NickName
    {
        get
        {
            return username;
        }

        set
        {
            username = value;
        }
    }
    /// <summary>
    /// 玩家密码
    /// </summary>
    public string Temp_pw
    {
        get
        {
            return temp_pw;
        }

        set
        {
            temp_pw = value;
        }
    }
    public static int Level
    {
        get
        {
            return m_level;
        }

        set
        {
            m_level = value;
        }
    }
    public static long Exp
    {
        get
        {
            return m_exp;
        }

        set
        {
            m_exp = value;
        }
    }

    public static long NextExp
    {
        get
        {
            return m_nextExp;
        }

        set
        {
            m_nextExp = value;
        }
    }

    private static string id;
    public static int uid;
    private static string username;
    private string temp_pw;

    private static lint m_money;//星币
    private static lint m_dimenCoin;//次元币（氪金要素
    private static lint m_volume;//共鸣点
    public static int VIP_Level;//玩家vip等级
    public static int SkillPoint;//技能点
    public static string OnlineKey;
    private static int m_level;//等级
    private static long m_exp;//当前经验值
    private static long m_nextExp;

    //private bool autoUpdate = true;
    public static bool DebugMode = false;

    //source指的是原始属性字典，dynamic指的是最终计算所得属性字典
    //
    static AttributeCollection sourceAttrs = new AttributeCollection();
    static AttributeCollection dynamicAttrs = new AttributeCollection();
    #endregion
    static PlayerInfoInGame()
    {
#if UNITY_EDITOR
        DebugMode = true;
#endif
    }
    private static List<ChipData> now_Chips = new List<ChipData>();
    private static List<ItemBase> now_Items = new List<ItemBase>();
    private static List<int> design_Ids = new List<int>();
    private static int design_NowEquipped = Designations.NOTHING;

    public static PlayerInfoInGame Instance { get; set; }

    /// <summary>
    /// 最近阶段更新，当前拥有的ItemBase的集合。
    /// </summary>
    public static List<ItemBase> CurrentItems
    {
        get
        {
            return now_Items;
        }

        set
        {
            now_Items = value;
        }
    }

    public static List<int> Design_Ids
    {
        get
        {
            return design_Ids;
        }

        set
        {
            design_Ids = value;
        }
    }
    /// <summary>
    /// 最近阶段更新，当前拥有的ChipData的集合。
    /// </summary>
    public static List<ChipData> CurrentChips
    {
        get
        {
            return now_Chips;
        }

        set
        {
            now_Chips = value;
        }
    }

    public static int Design_NowEquipped
    {
        get
        {
            return design_NowEquipped;
        }

        set
        {
            design_NowEquipped = value;
        }
    }

    private static string UPDATE_PLAYERINFO_FILE_PATH = "scripts/player/getPlayerInfo.php";

    void Awake()
    {
#if UNITY_EDITOR
        Id = "admin";
        NickName = "管理员";
        uid = 1;
        if (OnlineKey == null || OnlineKey == "")
        {
            OnlineKey = "4ad9b0374bebaa8fb4060e491501bc17";
            GlobalSettings.SetEncryptPerset(OnlineKey);
            //Debug.Log(GlobalSettings.GetEncryptKey());
        }
        //EnemyDataManager.InitAllEnemiesInList();
#endif
        Instance = this;
    }
    void Start()
    {
        /*
        for (int i = 0; i < AttributeCollection.attrNames.Count; i++)
        {
            Debug.Log(AttributeCollection.attrNames[i].Name);
        }
        */

        StartCoroutine(AutoUpdatePlayerInfo());
    }

#if UNITY_EDITOR
    private void Update()
    {
        //制作装备前缀说明
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("start");
            //BattleInstanceManager.GenerateAllFiles();
            //EquipmentFactory.CreatePrefixReadingFile();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ConTipsManager.AddMessage("测试", "成功了");
        }

    }
#endif
    #region 人物状态相关

    /// <summary>
    /// 向数据库请求刷新目前人物属性，会先刷新身上道具(装备，道具)
    /// <para>将会刷新原始字典，动态字典</para>
    /// </summary>
    public Coroutine RequestUpdatePlayerInfo()
    {
        return StartCoroutine(UpdatePlayerInfo());
    }

    /// <summary>
    /// autoUpdate为true时自动更新人物状态，每一分钟更新一次
    /// </summary>
    /// <returns></returns>
    IEnumerator AutoUpdatePlayerInfo()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }
    public static Coroutine OnlyUpdatePlayerAttrs()
    {
        return Instance.StartCoroutine(Instance._OnlyUpdatePlayerAttrs());
    }

    IEnumerator _OnlyUpdatePlayerAttrs()
    {
        //更新人物状态
        TempPlayerAttribute tempPlayerAttri = null;
        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("type", 0);
        WWW w = new WWW(CU.ParsePath(UPDATE_PLAYERINFO_FILE_PATH), form);
        CU.ShowConnectingUI();
        yield return w;
        CU.HideConnectingUI();
        if (CU.IsPostSucceed(w))
        {
            string jsonText = w.text;
            tempPlayerAttri = JsonUtility.FromJson<TempPlayerAttribute>(jsonText);

            //如果onlinekey不对，则强制退出游戏
            if (tempPlayerAttri.onlineKey != OnlineKey && !DebugMode)
            {
                MessageBox.Show("您的账号已在别的地方登陆，本地将会下线。", "提示", (result) =>
                {
                    StartCoroutine(GameSceneManager.Instance.ReturnLoginWindow());
                }, MessageBoxButtons.OK);
                yield break;
            }
        }
        else
        {
            Debug.LogWarning("Failed");
            CU.ShowConnectFailed();
            CU.HideConnectingUI();
            yield break;
        }
        yield return ComputePlayerInfo(tempPlayerAttri);
    }
    /// <summary>
    /// 计算当前人物信息。
    /// </summary>
    IEnumerator ComputePlayerInfo(TempPlayerAttribute tempPlayerAttr)
    {
        //普通属性
        Exp = tempPlayerAttr.exp;
        NextExp = tempPlayerAttr.nextExp;
        Level = tempPlayerAttr.level;
        m_money = tempPlayerAttr.money;
        m_volume = tempPlayerAttr.volume;
        m_dimenCoin = tempPlayerAttr.dimenCoin;
        SkillPoint = tempPlayerAttr.skillPoint;
        VIP_Level = tempPlayerAttr.vipLevel;
        //称号获取
        int[] designArray;
        int designNowEquipped;
        DesignationManager.ParseDesignData(tempPlayerAttr.designData, out designNowEquipped, out designArray);
        Design_NowEquipped = designNowEquipped;
        Design_Ids = new List<int>(designArray);
        //初始化动态词典
        sourceAttrs.SetValues(tempPlayerAttr);
        dynamicAttrs.SetValues(tempPlayerAttr);
        //设置装备动态词典
        for (int i = 0; i < CurrentItems.Count; i++)
        {
            if (CurrentItems[i] is EquipmentBase)
            {
                EquipmentBase equipment = (EquipmentBase)CurrentItems[i];
                if (equipment.IsEquipped)
                {
                    var attrColl = equipment.GetActualAttrs();
                    dynamicAttrs += attrColl;
                }
            }
        }
        //设置称号给予的加成
        CalculateAttrsByDesignation();
        MakePropertyDirty();
        yield return 0;
    }

    /// <summary>
    /// 计算称号给予的属性加成
    /// </summary>
    void CalculateAttrsByDesignation()
    {
        if (Design_NowEquipped == Designations.NOTHING)
        {
            return;
        }
        var data = DesignationManager.GetDesignationData(Design_NowEquipped);
        if (data == null)
        {
            return;
        }
        dynamicAttrs.MultValues(data);
    }
    /// <summary>
    /// 更新人物状态，从数据库拿取人物的原始属性。
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdatePlayerInfo()
    {
        /*更新成就获取
        bool updateDesign = false;
        if (DesignationManager.DesignToGets.Count > 0)
        {
            for (int i = 0; i < DesignationManager.DesignToGets.Count; i++)
            {
                yield return PlayerRequestBundle.RequestAddDesignation(DesignationManager.DesignToGets[i]);
            }
            updateDesign = true;
            DesignationManager.DesignToGets.Clear();
        }*/
        //先更新人物拥有道具
        yield return PlayerRequestBundle.RequestUpdateItemsInPack();
        //更新人物状态
        yield return _OnlyUpdatePlayerAttrs();
        //然后更新人物技能
        yield return PlayerRequestBundle.RequestGetSkillDatas();
        //更新UI下方信息
        MoneyShower.RefreshMoneyShower();
        DesignationManager.CheckPlayerInfo();
        if (GetMoney() > 500000000)
        {
            var tempattr = new TempPlayerAttribute();
            tempattr.money -= 100000000;
            SyncRequest.AppendRequest(Requests.PLAYER_DATA, tempattr);
            IIABinds binds = new IIABinds(Items.MONEY_CHEST, 1);
            SyncRequest.AppendRequest(Requests.ITEM_DATA, binds.ToJson(false));
            yield return PlayerRequestBundle.RequestSyncUpdate();
        }
        //if (updateDesign) OCManager.Refresh();
        //System.GC.Collect();
    }


    /// <summary>
    /// 获取当前（动态）属性（包括了基础属性与装备属性等的加成）
    /// </summary>
    /// <returns></returns>
    public AttributeCollection GetDynamicAttrs()
    {
        return dynamicAttrs;
    }

    public float GetDynamicAttrValue(Attr type)
    {
        return dynamicAttrs.GetValue(type);
    }

    public float GetSourceAttrValue(Attr type)
    {
        return sourceAttrs.GetValue(type);
    }
    #endregion

    /// <summary>
    /// 请求刷新人物的身上道具和装备
    /// </summary>
    public Coroutine RequestUpdatePlayerItems()
    {
        return PlayerRequestBundle.RequestUpdateItemsInPack();
    }

    /// <summary>
    /// 清除玩家客户端上所有物品(下一步就是请求刷新了)
    /// </summary>
    public void ClearAllItems()
    {
        CurrentItems.Clear();
    }
    /// <summary>
    /// 更新PropertyWindow中人物属性。
    /// </summary>
    public static void MakePropertyDirty()
    {
        GameSceneManager.PropertyDirty = true;
    }

    public void TestGainItem(ItemBase item)
    {
        CurrentItems.Add(item);
    }


    /// <summary>
    /// 得到当前的所有装备，是否包含已装备的装备（默认没有仓库内装备，可返回只包含仓库装备）
    /// </summary>
    /// <returns></returns>
    public static List<EquipmentBase> GetAllEquipments(bool equiped, bool onlyIncludeStorage = false)
    {
        List<EquipmentBase> equips = new List<EquipmentBase>();
        foreach (ItemBase item in CurrentItems)
        {
            if (item is EquipmentBase)
            {
                EquipmentBase equip = item as EquipmentBase;
                if (onlyIncludeStorage)
                {
                    if (equip.IsInStorage)
                    {
                        equips.Add(equip);
                    }
                }
                else
                {
                    if (equiped)
                    {
                        if (!equip.IsInStorage) equips.Add(equip);
                    }
                    else if (!equip.IsEquipped)
                    {
                        if (!equip.IsInStorage) equips.Add(equip);
                    }
                }
            }
        }
        return equips;
    }
    /// <summary>
    /// 返回当前装备的数量。
    /// </summary>
    /// <returns></returns>
    public static int GetEquipmentAmount(bool includeStorage = false)
    {
        int amount = 0;
        for (int i = 0; i < CurrentItems.Count; i++)
        {
            if (CurrentItems[i] is EquipmentBase)
            {
                if (includeStorage)
                {
                    amount++;
                }
                else if (!(CurrentItems[i] as EquipmentBase).IsInStorage)
                {
                    amount++;
                }
            }
        }
        return amount;
    }
    /// <summary>
    /// 得到当前的所有非武器的道具
    /// </summary>
    /// <returns></returns>
    public static List<ItemBase> GetAllItems()
    {
        List<ItemBase> items = new List<ItemBase>();
        foreach (ItemBase item in CurrentItems)
        {
            if (!(item is EquipmentBase))
            {
                items.Add(item);
            }
        }
        return items;
    }

    public static int GetMoney()
    {
        return m_money;
    }
    public static int GetDimenCoin()
    {
        return m_dimenCoin;
    }
    /// <summary>
    /// 返回对应vip等级所需要冲的次元壁
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetVipLevelNeedsDimen(int level)
    {
        return 5 * level * level - 5 * level + 10;
    }
    public static Coroutine _StartCoroutine(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }

}
/// <summary>
/// 缓存属性的子类，用于记录玩家信息。
/// </summary>
[System.Serializable]
public class TempPlayerAttribute : BaseAttribute
{
    public int level;
    public int skillPoint;//技能点
    public long money;
    public long dimenCoin;
    public long volume;
    public int vipLevel;//玩家vip等级
    public long exp;
    public long nextExp;
    public string onlineKey;
    public string designData;//玩家称号
}
/// <summary>
/// 记录被推广人/推广人。
/// </summary>
[System.Serializable]
public class TempPromoter
{
    public string id;
    public string nickname;
    public int level;
    public int totalDimen;
    public int promoteDimen;
}