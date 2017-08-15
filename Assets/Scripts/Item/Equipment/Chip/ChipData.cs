using GameId;
using SerializedClassForJson.Chip;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// TempChipRawData的封装类
/// </summary>
public class ChipData
{
    private TempChipRawData rawData;

    private AttributeCollection effect_Attrs;
    private const float BASE_IMPROVE_EACH_LEVEL = 0.1f;
    private string deconame = "";
    private static readonly Dictionary<Attr, string> ATTR_MAIN_CHIP_DECONAMES = new Dictionary<Attr, string>();
    static ChipData()
    {
        ATTR_MAIN_CHIP_DECONAMES.Add(Attrs.MHP, "架构");
        ATTR_MAIN_CHIP_DECONAMES.Add(Attrs.MMP, "新核");
        ATTR_MAIN_CHIP_DECONAMES.Add(Attrs.ATK, "猛攻");
        ATTR_MAIN_CHIP_DECONAMES.Add(Attrs.DEF, "壁垒");
        ATTR_MAIN_CHIP_DECONAMES.Add(Attrs.LOG, "睿智");
        ATTR_MAIN_CHIP_DECONAMES.Add(Attrs.LCK, "强运");
        ATTR_MAIN_CHIP_DECONAMES.Add(Attrs.CRI, "满溢");
    }
    public ChipData(TempChipRawData raw)
    {
        rawData = raw;
        ParseRaw();
    }
    /// <summary>
    /// 解析服务器传递的字符串。
    /// </summary>
    void ParseRaw()
    {
        var raw = rawData;
        switch (raw.effectId)
        {
            case Effects.ATTR:
                var eParams = JsonUtility.FromJson<TempChipEffectParamsAttr>(raw.effectParams);
                effect_Attrs = new AttributeCollection();
                effect_Attrs.SetValues(eParams);
                effect_Attrs.MultValues(GetRarityModifiedMult() * (1f + eParams.rand));
                effect_Attrs.MultValues(1f + BASE_IMPROVE_EACH_LEVEL * rawData.level);
                SetDecoName();
                break;
            default:
                break;
        }
    }
    public bool IsEffect(int effect)
    {
        return rawData.effectId == effect;
    }
    public bool IsEquipped()
    {
        return rawData.equip_id != -1;
    }
    public int GetEquippedId()
    {
        return rawData.equip_id;
    }
    /// <summary>
    /// 进行下次升级所需要的杀敌数。
    /// </summary>
    /// <returns></returns>
    public lint GetNextLevelNeedKillAmounts()
    {
        return 5 * (lint)Mathf.Pow(2, rawData.level);
    }
    public int GetId()
    {
        return rawData.id;
    }
    /// <summary>
    /// 返回是否可以升级。
    /// </summary>
    /// <returns></returns>
    public bool CanUpgrade()
    {
        return GetNextLevelNeedKillAmounts() <= rawData.killAmounts;
    }
    public int GetLevel()
    {
        return rawData.level;
    }
    public lint GetKillAmounts()
    {
        return rawData.killAmounts;
    }
    public lint GetNextLevelNeedSpbPieces()
    {
        return Mathf.RoundToInt(1000f * Mathf.Pow(2, rawData.level) * (0.75f + 0.25f * rawData.starRarity));
    }
    /// <summary>
    /// 返回Effect == ATTR时的属性集合。（全是基数为0的浮点数）
    /// </summary>
    /// <returns></returns>
    public AttributeCollection GetAttrColl()
    {
        return effect_Attrs;
    }
    /// <summary>
    /// 获得该芯片的完整名字。
    /// </summary>
    /// <param name="containLevel"></param>
    /// <returns></returns>
    public string GetFullName(bool containLevel = false)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("<color={0}>{1} T{2} {3}芯片</color>", TextUtils.GetRarityColorCode(rawData.starRarity), rawData.originName, rawData.starRarity, deconame);
        if (containLevel) { sb.AppendLine(); sb.AppendFormat(" Lv.{0}", rawData.level); }
        return sb.ToString();
    }
    /// <summary>
    /// 获取特殊效果，当前杀敌数和总计杀敌数。
    /// </summary>
    /// <returns></returns>
    public string GetInfo1()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("特殊效果：");
        sb.Append("    ");
        switch (rawData.effectId)
        {
            case Effects.ATTR:
                sb.Append("装备属性加成");
                break;
            default:
                break;
        }
        sb.AppendLine();
        sb.AppendLine("当前杀敌数：");
        sb.AppendFormat("   <size=12>{0}(/{1})</size>", rawData.killAmounts, GetNextLevelNeedKillAmounts());
        sb.AppendLine();
        sb.AppendLine("总计杀敌数：");
        sb.AppendFormat("   <size=12>{0}</size>", rawData.totalKillAmounts);
        return sb.ToString();
    }
    public string GetInfo2()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("具体加成：");
        switch (rawData.effectId)
        {
            case Effects.ATTR:
                var attrs = effect_Attrs.GetValues();
                foreach (var kv in attrs)
                {
                    if (kv.Value == 0) continue;
                    sb.AppendFormat("   ·{0} + {1}%", kv.Key.Name, (kv.Value * 100).ToString("f2"));
                    sb.AppendLine();
                }
                break;
            default:
                break;
        }
        return sb.ToString();
    }
    /// <summary>
    /// 根据星级获得额外加成系数。
    /// </summary>
    /// <returns></returns>
    float GetRarityModifiedMult()
    {
        int rarity = rawData.starRarity;
        float multi = 1f;
        if (rarity <= 4)
        {
            multi = 0.9f + 0.1f * rarity;
        }
        //else if (rarity <= 7)
        else
        {
            multi = 1f + 0.25f * (rarity - 3);
        }
        return multi;
    }
    void SetDecoName()
    {
        Attr maxAttr = effect_Attrs.GetMaxValueAttr();
        deconame = ATTR_MAIN_CHIP_DECONAMES[maxAttr];
    }
    public int GetRaritySpriteIndex()
    {
        return rawData.starRarity - 1;
    }

    /*
    public EquipmentBase GetLinkedEquipment()
    {

    }
    */
}
namespace SerializedClassForJson.Chip
{
    [System.Serializable]
    public class TempChipRawData
    {
        public int id;
        public string player_id;
        public string originName;
        public int equip_id;
        public int starRarity;
        public int level;
        public long killAmounts;
        public long totalKillAmounts;
        public int effectId;
        public string effectParams;
    }
    [System.Serializable]
    public class TempChipRandGenData
    {
        public int handler = 0;
        public int starRarity;
        public int effectId;
        public string effectGenInfo;
        public int addOthersKillAmounts;
    }
    [System.Serializable]
    public class TempChipRelativeData
    {
        public int handler = 1;
        public int chipId;
        public bool deleteIt = false;
        public string setEquipId;
        public int addedLevel;
        public long addedKillAmounts;
    }
    [System.Serializable]
    public class TempChipEffectParamsAttr : BaseAttribute
    {
        public float rand;//随机因子(-0.02~0.02)
    }
}
