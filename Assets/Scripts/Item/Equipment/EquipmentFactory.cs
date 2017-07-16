using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson;
using System.Text;
using System.IO;
using GameId;

public class EquipmentFactory
{
    /// <summary>
    /// 记录是否已经初始化装备工厂了
    /// </summary>
    public static bool IsFactoryInit = false;
    public static Dictionary<int, PrefixEquipMod> Prefix_IdToMod;
    public static Dictionary<int, BaseEquipMod> Base_IdToMod;

    /// <summary>
    /// 初始化工厂的所有信息
    /// </summary>
    public static void InitFactory(TempEquipModCollection collection)
    {
        if (!IsFactoryInit)
        {
            Prefix_IdToMod = new Dictionary<int, PrefixEquipMod>();
            Base_IdToMod = new Dictionary<int, BaseEquipMod>();
            for (int i = 0; i < collection.bases.Length; i++)
            {
                Base_IdToMod.Add(collection.bases[i].id, collection.bases[i]);
            }
            for (int i = 0; i < collection.prefixs.Length; i++)
            {
                Prefix_IdToMod.Add(collection.prefixs[i].id, collection.prefixs[i]);
            }
            IsFactoryInit = true;
        }
    }

    /// <summary>
    /// 通过id得到装备基本名的mod
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static BaseEquipMod GetBaseEquipModById(int id)
    {
        return Base_IdToMod.ContainsKey(id) ? Base_IdToMod[id] : null;
    }
    /// <summary>
    /// 通过id得到装备前缀名的mod
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static PrefixEquipMod GetPrefixEquipModById(int id)
    {
        return Prefix_IdToMod.ContainsKey(id) ? Prefix_IdToMod[id] : null;
    }

    public static EquipmentBase CreateEquipment(TempEquipment temp)
    {
        EquipmentValue values = new EquipmentValue(temp);
        //通过词缀们来构筑description/或者有自定义description
        StringBuilder affixSb = new StringBuilder();
        //通过词缀们来构筑name
        StringBuilder nameSb = new StringBuilder();
        if (temp.description != "")
        {
            affixSb.Append(temp.description);
        }
        BaseEquipMod baseName = GetBaseEquipModById(temp.affixList[temp.affixList.Length - 1]);
        if (temp.name != "")
        {
            nameSb.Append(temp.name);
        }
        else
        {
            for (int i = 0; i < temp.affixList.Length - 1; i++)
            {
                affixSb.Append(GetPrefixEquipModById(temp.affixList[i]).description);
                nameSb.Append(GetPrefixEquipModById(temp.affixList[i]).name);
            }
            affixSb.Append(baseName.description);
            nameSb.Append(baseName.name);
        }
        EquipmentBase result = new EquipmentBase(temp.item_id, nameSb.ToString(), affixSb.ToString(), baseName.iconFileName,
            temp.spb, temp.price, (EQ_QUALITY)temp.eqQuality, (EQ_TYPE)temp.eqType, values);
        result.SetEquipped(temp.isEquipped);
        result.SetIndexInPack(temp.indexInPack);
        result.SetAffixIds(temp.affixList);
        int[] levels = { temp.eha_level, temp.eha_reha, temp.eha_rebuild };
        result.SetEhaLevels(levels);
        return result;
    }

    /// <summary>
    /// 前缀解释生成文件
    /// </summary>
    public static void CreatePrefixReadingFile()
    {
        FileStream fs = new FileStream("装备前缀解释.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs);
        List<StringBuilder> sbLists = new List<StringBuilder>();
        for (int i = 0; i < 4; i++)
        {
            sbLists.Add(new StringBuilder().AppendLine(EquipmentBase.GetEqQualityNameByType((EQ_QUALITY)i)));
        }
        foreach (var prefix in Prefix_IdToMod.Values)
        {
            var sb = sbLists[prefix.eqQuality];
            sb.Append(prefix.name);
            sb.Append(",生命");
            sb.Append(prefix.mhpMult);
            sb.Append("倍补正");
            sb.Append(",能量");
            sb.Append(prefix.mmpMult);
            sb.Append("倍补正");
            sb.Append(",攻击");
            sb.Append(prefix.atkMult);
            sb.Append("倍补正");
            sb.Append(",防御");
            sb.Append(prefix.defMult);
            sb.Append("倍补正");
            sb.Append(",逻辑");
            sb.Append(prefix.logMult);
            sb.Append("倍补正");
            sb.Append(",幸运");
            sb.Append(prefix.lckMult);
            sb.Append("倍补正");
            sb.Append(",暴击倍率");
            sb.Append(prefix.criMult);
            sb.AppendLine("倍补正");
        }
        for (int i = 0; i < sbLists.Count; i++)
        {
            sw.WriteLine(sbLists[i].ToString());
        }
        sw.Close();
        fs.Close();
    }
}
/// <summary>
/// 前缀
/// </summary>
[System.Serializable]
public class PrefixEquipMod : EquipModification
{
    public float priceMult;
}
/// <summary>
/// 装备基本名，基本名将会决定装备强化需求
/// </summary>
[System.Serializable]
public class BaseEquipMod : EquipModification
{
    public int eqType;
    public int price;
    public string iconFileName;
    public string[] eham_ids;//强化需要道具
    public int[] eham_needLevel;//强化需要道具的需要等级
    public int[] eham_stopLevel;//强化需要道具的暂停需求等级
    public int[] eham_ba;//道具基本数量
    public float[] eham_apl;//每级增加数量

    private List<EhaDemand> demands;//即分析各个道具的强化需求

    public List<EhaDemand> InitEhaDemand()
    {
        demands = new List<EhaDemand>();
        for (int i = 0; i < eham_ids.Length; i++)
        {
            demands.Add(new EhaDemand(eham_ids[i], eham_needLevel[i], eham_stopLevel[i], eham_ba[i], eham_apl[i]));
        }
        return demands;
    }
    public List<EhaDemand> GetDemands()
    {
        if (demands == null)
        {
            return InitEhaDemand();
        }
        else
        {
            return demands;
        }
    }
    /// <summary>
    /// 根据装备目前等级状态生成基本名的道具需求
    /// <para>例如每级再塑需要等级*1的再塑秘石,装备等级1级以上，再塑等级超过包括5时，每级需要2*（等级-4）</para>
    /// </summary>
    /// <param name="equip"></param>
    /// <param name="generateType"></param>
    /// <returns></returns>
    public List<EhaDemand> GenerateDemandsByEquip(EquipmentBase equip)
    {
        List<EhaDemand> demands = new List<EhaDemand>(GetDemands());
        //List<EhaDemand> demands = new List<EhaDemand>();
        //demands.Add(new EhaDemand("iron_normal", 1, 1, 1, 0));
        if (equip.eha_level == 10)
        {
            demands.Add(new EhaDemand(Items.REHANCE_STONE, equip.eha_reha + 1));
            if ((int)equip.GetEquipmentQuality() >= 1 && equip.eha_reha >= 5)
            {
                demands.Add(new EhaDemand(Items.REHANCE_SPAR, (equip.eha_reha - 4) * 2));
            }
        }
        return demands;
    }

    /// <summary>
    /// 强化道具需求
    /// </summary>
    public class EhaDemand
    {
        public string item_id;
        public int needLevel;
        public int stopLevel;
        public int baseAmount;
        public float amountPerLevel;
        public EhaDemand(string item_id, int needLevel, int stopLevel, int baseAmount, float amountPerLevel)
        {
            this.item_id = item_id;
            this.needLevel = needLevel;
            this.stopLevel = stopLevel;
            this.baseAmount = baseAmount;
            this.amountPerLevel = amountPerLevel;
        }
        public EhaDemand(string item_id, int baseAmount) : this(item_id, 1, 0, baseAmount, 0)
        {

        }
        /// <summary>
        /// 当前这个强化要求是否满足
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        public bool CanDemandCompleted(EquipmentBase equipment)
        {
            return needLevel <= equipment.GetAllocatedLevel() && (stopLevel > equipment.GetAllocatedLevel() || stopLevel <= 0);
        }
    }
    public EQ_TYPE GetEqType()
    {
        return (EQ_TYPE)eqType;
    }
}
