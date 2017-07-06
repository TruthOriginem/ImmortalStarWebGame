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

    public static int m_money;//星币
    public static int m_dimenCoin;//次元币（氪金要素
    public static int VIP_Level;//玩家vip等级
    public static int SkillPoint;//技能点
    public static string OnlineKey;
    private static int m_level;//等级
    private static long m_exp;//当前经验值
    private static long m_nextExp;

    //private bool autoUpdate = true;
    public static bool DebugMode = false;

    //s开头的为没有其他加成的原始属性（基础属性）

    private float s_hprate;//回复生命速率，具体回复应该由服务器控制
    private float s_mprate;//回复能量速率


    //source指的是原始属性字典，dynamic指的是最终计算所得属性字典
    //
    static Dictionary<PROPERTY_TYPE, IProperty> sourcePropertyDic = new Dictionary<PROPERTY_TYPE, IProperty>();
    static Dictionary<PROPERTY_TYPE, IProperty> dynamicPropertyDic = new Dictionary<PROPERTY_TYPE, IProperty>();
    static PlayerInfoInGame()
    {
        sourcePropertyDic.Add(PROPERTY_TYPE.MHP, new PropMhp());
        sourcePropertyDic.Add(PROPERTY_TYPE.MMP, new PropMmp());
        sourcePropertyDic.Add(PROPERTY_TYPE.ATK, new PropAtk());
        sourcePropertyDic.Add(PROPERTY_TYPE.DEF, new PropDef());
        sourcePropertyDic.Add(PROPERTY_TYPE.LOG, new PropLog());
        sourcePropertyDic.Add(PROPERTY_TYPE.LCK, new PropLck());
        sourcePropertyDic.Add(PROPERTY_TYPE.CRI, new PropCri());
        dynamicPropertyDic.Add(PROPERTY_TYPE.MHP, new PropMhp());
        dynamicPropertyDic.Add(PROPERTY_TYPE.MMP, new PropMmp());
        dynamicPropertyDic.Add(PROPERTY_TYPE.ATK, new PropAtk());
        dynamicPropertyDic.Add(PROPERTY_TYPE.DEF, new PropDef());
        dynamicPropertyDic.Add(PROPERTY_TYPE.LOG, new PropLog());
        dynamicPropertyDic.Add(PROPERTY_TYPE.LCK, new PropLck());
        dynamicPropertyDic.Add(PROPERTY_TYPE.CRI, new PropCri());
#if UNITY_EDITOR
        DebugMode = true;
#endif
    }

    /// <summary>
    /// 玩家目前身上拥有的物品
    /// </summary>
    public static List<ItemBase> Now_Items = new List<ItemBase>();
    /// <summary>
    /// 玩家目前所拥有的称号的id
    /// </summary>
    public static List<int> Design_Ids = new List<int>();
    /// <summary>
    /// 玩家当前使用的称号id
    /// </summary>
    public static int Design_NowEquipped = Designations.NOTHING;

    public static PlayerInfoInGame Instance { get; set; }


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
        StartCoroutine(AutoUpdatePlayerInfo());
    }
    private void Update()
    {
#if UNITY_EDITOR
        //制作装备前缀说明
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("start");
            //BattleInstanceManager.GenerateAllFiles();
            //EquipmentFactory.CreatePrefixReadingFile();
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            //ExpeditionManager.Instance.expeditionWindowToggle.isOn = true;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ConTipsManager.AddMessage("测试", "成功了");
        }

#endif
    }
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
        /*
        autoUpdate = true;
        while (autoUpdate)
        {
            yield return new WaitForSeconds(60);
            StartCoroutine(UpdatePlayerInfo());
        }
        */
        //StartCoroutine(UpdatePlayerInfo());
        while (true)
        {
            yield return new WaitForSeconds(60);
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
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
        m_dimenCoin = tempPlayerAttr.dimenCoin;
        SkillPoint = tempPlayerAttr.skillPoint;
        VIP_Level = tempPlayerAttr.vipLevel;
        //称号获取
        int[] designArray;
        DesignationManager.ParseDesignData(tempPlayerAttr.designData, out Design_NowEquipped, out designArray);
        Design_Ids = new List<int>(designArray);

        SetPropertyValue(PROPERTY_TYPE.MHP, tempPlayerAttr.mhp, sourcePropertyDic);
        SetPropertyValue(PROPERTY_TYPE.MMP, tempPlayerAttr.mmp, sourcePropertyDic);
        SetPropertyValue(PROPERTY_TYPE.ATK, tempPlayerAttr.atk, sourcePropertyDic);
        SetPropertyValue(PROPERTY_TYPE.DEF, tempPlayerAttr.def, sourcePropertyDic);
        SetPropertyValue(PROPERTY_TYPE.LOG, tempPlayerAttr.log, sourcePropertyDic);
        SetPropertyValue(PROPERTY_TYPE.LCK, tempPlayerAttr.lck, sourcePropertyDic);
        SetPropertyValue(PROPERTY_TYPE.CRI, tempPlayerAttr.cri, sourcePropertyDic);
        //初始化动态词典
        foreach (var kv in sourcePropertyDic)
        {
            SetPropertyValue(kv.Key, kv.Value.Value, dynamicPropertyDic);
        }
        //设置装备动态词典
        for (int i = 0; i < Now_Items.Count; i++)
        {
            if (Now_Items[i] is EquipmentBase)
            {
                EquipmentBase equipment = (EquipmentBase)Now_Items[i];
                if (equipment.IsEquipped())
                {
                    EquipmentValue value = equipment.GetProperties();
                    foreach (var property in dynamicPropertyDic)
                    {
                        property.Value.Value += value.values[property.Key].Value;
                    }
                }
            }
        }
        //设置称号给予的加成
        CalculatePropertiesByDesignation();
        MakePropertyDirty();
        yield return 0;
    }

    /// <summary>
    /// 计算称号给予的属性加成
    /// </summary>
    void CalculatePropertiesByDesignation()
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
        dynamicPropertyDic[PROPERTY_TYPE.MHP].Value *= data.mhpMult;
        dynamicPropertyDic[PROPERTY_TYPE.MMP].Value *= data.mmpMult;
        dynamicPropertyDic[PROPERTY_TYPE.ATK].Value *= data.atkMult;
        dynamicPropertyDic[PROPERTY_TYPE.DEF].Value *= data.defMult;
        dynamicPropertyDic[PROPERTY_TYPE.LOG].Value *= data.logMult;
        dynamicPropertyDic[PROPERTY_TYPE.LCK].Value *= data.lckMult;
        dynamicPropertyDic[PROPERTY_TYPE.CRI].Value *= data.criMult;
        //SetPropertyValue(PROPERTY_TYPE.ATK)
    }
    /// <summary>
    /// 更新人物状态，从数据库拿取人物的原始属性。
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdatePlayerInfo()
    {
        bool needRefreshMoneyAndItem = false;
        //更新成就获取
        if (DesignationManager.DesignToGets.Count > 0)
        {
            for (int i = 0; i < DesignationManager.DesignToGets.Count; i++)
            {
                yield return PlayerRequestBundle.RequestAddDesignation(DesignationManager.DesignToGets[i]);
            }
            DesignationManager.DesignToGets.Clear();
        }
        //先更新人物拥有道具
        yield return PlayerRequestBundle.RequestUpdateItemsInPack();
        //更新人物状态
        TempPlayerAttribute tempPlayerAttri = null;
        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("type", 0);
        WWW w = new WWW(ConnectUtils.ParsePath(UPDATE_PLAYERINFO_FILE_PATH), form);
        ConnectUtils.ShowConnectingUI();
        yield return w;
        if (ConnectUtils.IsPostSucceed(w))
        {
            string jsonText = w.text;
            tempPlayerAttri = JsonUtility.FromJson<TempPlayerAttribute>(jsonText);
            int money = tempPlayerAttri.money;
            //超过5亿则开始
            if (money >= 500000000)
            {
                needRefreshMoneyAndItem = true;
            }
            //如果onlinekey不对，则强制退出游戏
            if (tempPlayerAttri.onlineKey != OnlineKey && !DebugMode)
            {
                //Debug.LogError(tempPlayerAttri.onlineKey + ":" + OnlineKey);
                //autoUpdate = false;
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
            ConnectUtils.ShowConnectFailed();
            ConnectUtils.HideConnectingUI();
            yield break;
        }
        yield return StartCoroutine(ComputePlayerInfo(tempPlayerAttri));
        //然后更新人物技能
        yield return PlayerRequestBundle.RequestGetSkillDatas();
        MoneyShower.RefreshMoneyShower();
        ConnectUtils.HideConnectingUI();
        if (needRefreshMoneyAndItem)
        {
            var tempattr = new TempPlayerAttribute();
            tempattr.money -= 100000000;
            IIABinds binds = new IIABinds(new string[] { Items.MONEY_CHEST }, new int[] { 1 });
            yield return PlayerRequestBundle.RequestUpdateRecord<Object>(null, binds, tempattr, null);
        }
        //System.GC.Collect();
    }


    /// <summary>
    /// 获取当前（动态）属性（包括了基础属性与装备属性等的加成）
    /// </summary>
    /// <returns></returns>
    public Dictionary<PROPERTY_TYPE, IProperty> GetDynamicProperties()
    {
        return dynamicPropertyDic;
    }

    public float GetDynamicPropertyValue(PROPERTY_TYPE type)
    {
        return dynamicPropertyDic[type].Value;
    }

    public float GetSourcePropertyValue(PROPERTY_TYPE type)
    {
        return sourcePropertyDic[type].Value;
    }
    /// <summary>
    /// 设置属性值。
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <param name="value">数值</param>
    /// <param name="proDic">原始属性/动态属性词典</param>
    void SetPropertyValue(PROPERTY_TYPE type, float value, Dictionary<PROPERTY_TYPE, IProperty> proDic)
    {
        IProperty property = proDic[type];
        property.Value = value;
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
        Now_Items.Clear();
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
        Now_Items.Add(item);
    }


    /// <summary>
    /// 得到当前的所有装备，是否包含已装备的装备
    /// </summary>
    /// <returns></returns>
    public static List<EquipmentBase> GetAllEquipments(bool equiped)
    {
        List<EquipmentBase> equips = new List<EquipmentBase>();
        foreach (ItemBase item in Now_Items)
        {
            if (item is EquipmentBase)
            {
                EquipmentBase equip = item as EquipmentBase;
                if (equiped)
                {
                    equips.Add(item as EquipmentBase);
                }
                else if (!equip.IsEquipped())
                {
                    equips.Add(item as EquipmentBase);
                }
            }
        }
        return equips;
    }
    /// <summary>
    /// 返回当前装备的数量。
    /// </summary>
    /// <returns></returns>
    public static int GetEquipmentAmount()
    {
        int amount = 0;
        for (int i = 0; i < Now_Items.Count; i++)
        {
            if (Now_Items[i] is EquipmentBase)
            {
                amount++;
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
        foreach (ItemBase item in Now_Items)
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
public class TempPlayerAttribute : TempAttribute
{
    public int level;
    public int skillPoint;//技能点
    public int money;
    public int dimenCoin;
    public int vipLevel;//玩家vip等级
    public long exp;
    public long nextExp;
    public float nhp;
    public float nmp;
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