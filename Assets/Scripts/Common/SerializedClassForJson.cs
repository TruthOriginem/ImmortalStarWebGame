using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
/// <summary>
/// Json相关的类
/// </summary>
namespace SerializedClassForJson
{
    /// <summary>
    /// 用于记录道具和数量两个对应数组的临时实例，也可用于删除指定装备
    /// </summary>
    public class IIABinds
    {
        public string[] item_ids;
        public int[] amounts;
        public string[] equip_ids_to_delete;
        public IIABinds(string[] item_ids, int[] amounts, string[] equip_ids_to_delete = null)
        {
            this.item_ids = item_ids;
            this.amounts = amounts;
            this.equip_ids_to_delete = equip_ids_to_delete;
        }
        public IIABinds(Dictionary<string, int> idsToAmounts, string[] equip_ids_to_delete = null)
        {
            this.item_ids = idsToAmounts.Keys.ToArray();
            this.amounts = idsToAmounts.Values.ToArray();
            this.equip_ids_to_delete = equip_ids_to_delete;

        }
        public static IIABinds Create(Dictionary<string, int> itemIdsToAmounts)
        {
            return new IIABinds(itemIdsToAmounts.Keys.ToArray(), itemIdsToAmounts.Values.ToArray());
        }
        public static IIABinds Create(List<string> equipIds)
        {
            return new IIABinds(null, null, equipIds.ToArray());
        }
        /// <summary>
        /// 将道具id及字符串转化为json字符串，形式为数组,比如[{"item_id":"","amount":0}]
        /// </summary>
        /// <returns></returns>
        public string GenerateJsonString(bool deleteEq)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            if (deleteEq)
            {
                if (equip_ids_to_delete == null || equip_ids_to_delete.Length == 0)
                {
                    return "";
                }
                for (int i = 0; i < equip_ids_to_delete.Length; i++)
                {
                    TempItemInfo itemToAmount = new TempItemInfo();
                    itemToAmount.item_id = equip_ids_to_delete[i];
                    itemToAmount.amount = -1;
                    sb.Append(JsonUtility.ToJson(itemToAmount));
                    if (i < equip_ids_to_delete.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
            }
            else
            {
                if (item_ids == null || item_ids.Length == 0)
                {
                    return "";
                }
                for (int i = 0; i < item_ids.Length; i++)
                {
                    TempItemInfo itemToAmount = new TempItemInfo();
                    itemToAmount.item_id = item_ids[i];
                    itemToAmount.amount = amounts[i];
                    sb.Append(JsonUtility.ToJson(itemToAmount));
                    if (i < item_ids.Length - 1)
                    {
                        sb.Append(",");
                    }
                }

            }
            sb.Append("]");

            return sb.ToString();
        }
    }

    /// <summary>
    /// 缓存属性的子类，用于记录一部分怪物信息。
    /// </summary>
    [System.Serializable]
    public class TempEnemyAttribute : TempAttribute
    {
        public int idkey;
        public string id;
        public string name;
        public string description;
        public string iconName;
        public int dropExp;
        public TempItemDrops[] dropItems;
        public TempEnemyGrowthAttr growAttr;
    }
    [System.Serializable]
    public class TempEnemyGrowthAttr : TempAttribute
    {
        public int dropExp;
    }
    /// <summary>
    /// 道具数量与道具id
    /// </summary>
    [System.Serializable]
    public class TempItemInfo
    {
        public string item_id;
        public int amount;
        public int indexInPack;//包内目录
        public int needLevel;//需要的等级
    }
    [System.Serializable]
    public class TempItemIndex
    {
        public string[] item_ids;
        public string[] equip_ids;
        public int[] item_indexs;
        public int[] equip_indexs;

        public static TempItemIndex Create(Dictionary<ItemBase, int> idToIndexs)
        {
            TempItemIndex temp = new TempItemIndex();
            List<string> item_ids = new List<string>(20);
            List<string> equip_ids = new List<string>(20);
            List<int> item_indexs = new List<int>(20);
            List<int> equip_indexs = new List<int>(20);
            foreach (var kv in idToIndexs)
            {
                if (kv.Key is EquipmentBase)
                {
                    equip_ids.Add(kv.Key.item_id);
                    equip_indexs.Add(kv.Value);
                }
                else
                {
                    item_ids.Add(kv.Key.item_id);
                    item_indexs.Add(kv.Value);
                }
            }
            temp.equip_ids = equip_ids.ToArray();
            temp.equip_indexs = equip_indexs.ToArray();
            temp.item_ids = item_ids.ToArray();
            temp.item_indexs = item_indexs.ToArray();
            return temp;
        }
    }
    /// <summary>
    /// 用于战斗结果结算
    /// </summary>
    [System.Serializable]
    public class TempItemDrops
    {
        public string id;
        public int amount;
        public float chance;//掉落概率
        public int needLevel;//需要等级才掉落
        public float multLevel;//随着怪物等级提高，道具的掉落数量也会提高
    }
    /// <summary>
    /// 记录上一次攻击的关卡及损失的hp和mp，挂机相关事宜
    /// <para>挂机时是不能进行战斗的</para>
    /// <para>上传这个的时候会更新玩家的关卡数据</para>
    /// </summary>
    [System.Serializable]
    public class TempLigRecord
    {
        public string lig_id;
        public float lig_lostHp;//失去的生命
        public float lig_lostMp;//失去的能量
        public float lig_lostTime;//失去的时间
        public int lig_hangTime = -1;//挂机时长
        public bool lig_dontRecord = false;//需不需要记录，往往都是具有关卡限制的不能记录
        //public bool isHanged = false;//此次操作是否是挂机，用于POST而不是GET
        public void SetHangFinish()
        {
            lig_hangTime = -1;
        }
        public void SetHangBegin()
        {
            lig_hangTime = 0;
        }
    }
    /// <summary>
    /// 用于更新/获取装备状态而使用的Temp父类
    /// </summary>
    [System.Serializable]
    public class TempEquipAttr : TempAttribute
    {
        public string item_id;
        public float spb;
        public int eha_level;//强化等级
        public int eha_reha;//再塑等级
        public int eha_rebuild;//重构等级
        public int price;

        public void SetPropertiesByValue(EquipmentValue value)
        {
            mhp = value.GetValue(PROPERTY_TYPE.MHP);
            mmp = value.GetValue(PROPERTY_TYPE.MMP);
            atk = value.GetValue(PROPERTY_TYPE.ATK);
            def = value.GetValue(PROPERTY_TYPE.DEF);
            log = value.GetValue(PROPERTY_TYPE.LOG);
            lck = value.GetValue(PROPERTY_TYPE.LCK);
            cri = value.GetValue(PROPERTY_TYPE.CRI);
        }

        /// <summary>
        /// 设置如果强化，那么接下来的强化是普通强化还是再塑、重构。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mode"></param>
        public void CheckAGetEnhanceMode(EquipmentBase source, out EnhanceManager.MODE mode)
        {
            eha_level = source.eha_level + 1;
            eha_reha = source.eha_reha;

            eha_rebuild = source.eha_rebuild;
            if (eha_level > 10)
            {
                eha_level = 0;
                eha_reha++;
                mode = EnhanceManager.MODE.REHANCE;
            }
            else
            {
                mode = EnhanceManager.MODE.ENHANCE;
            }
        }
    }
    /// <summary>
    /// 用来记录来自数据库的武器信息的。
    /// </summary>
    [System.Serializable]
    public class TempEquipment : TempEquipAttr
    {
        public string name;
        public string description;
        public string iconFileName;
        public int[] affixList;//词缀+基本名id
        public int eqQuality;
        public int eqType;
        public int indexInPack = -1;
        public bool isEquipped;
    }
    /// <summary>
    /// 用于生成随机武器的序列化对象，请求数据库专用
    /// </summary>
    [System.Serializable]
    public class TempRandEquipRequest
    {
        public int eqType;//指定的装备类型
        public int eqQuality;//指定的装备品质等级
        public float value;//指定的灵基值
        public int prefixAmount;//词缀的数量

        public static string GenerateJsonArray(TempRandEquipRequest[] requests)
        {
            string json = "[";
            if (requests != null && requests.Length > 0)
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    json += JsonUtility.ToJson(requests[i]);
                    if (i < requests.Length - 1)
                    {
                        json += ",";
                    }
                }
            }
            json += "]";
            return json;
        }
    }
    /// <summary>
    /// 游戏初始化下载的所有跟强化装备有关的数据
    /// </summary>
    [System.Serializable]
    public class TempEquipModCollection
    {
        public PrefixEquipMod[] prefixs;
        public BaseEquipMod[] bases;
    }
    /// <summary>
    /// 技能等级数据
    /// </summary>
    [System.Serializable]
    public class TempSkillData
    {
        public int idkey;
        public int level;
        public bool equipped;
    }
    /// <summary>
    /// 技能等级数据的集合
    /// </summary>
    [System.Serializable]
    public class TempSkills
    {
        public TempSkillData[] datas;
    }
    /// <summary>
    /// 用于记录关卡信息的，具体是否可以攻击取决于数据库的数据。
    /// <para>当数据库里拥有这个关卡的数据时，则说明这个关卡已经可以解锁了
    /// 特殊关卡，比如boss关卡,涉及到限制次数，则在数据库里会有专门记录时间</para>
    /// <para> </para>
    /// </summary>
    [System.Serializable]
    public class TempBattleInGridRecord
    {
        public string id;
        public int tc;//今天攻击的次数,根据grid的限制次数进行禁用
        //public bool ba;//是否可以攻击，数据库里没有这个数据
    }
    /// <summary>
    /// 当前页的玩家排行榜属性，例如星币排行榜，arg1为排名，arg2为昵称，arg3为星币
    /// </summary>
    [System.Serializable]
    public class TempPlayerRankInfo
    {
        public string arg1;
        public string arg2;
        public string arg3;
    }
    [System.Serializable]
    public class TempRankInfoBundle
    {
        public TempPlayerRankInfo[] infos;
        public int totalPage;
    }
    /// <summary>
    /// 记录远征的怪物权重信息
    /// </summary>
    [System.Serializable]
    public class TempEnemyExpeditionInfo
    {
        public string id;//怪物id
        public int[] lvr;//等级区间
        public int[] pr;//权重区间
    }
    [System.Serializable]
    public class TempPlayerExpeditionInfo
    {
        public int nowLightYear;//当前玩家已到达的远征里程数
        public int maxLightYear;//曾经到达过的最大里程数
        public bool ifEscaped;//上次是否逃跑了
    }
}