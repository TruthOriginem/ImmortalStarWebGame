using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameId;

public class EquipmentBase : ItemBase
{
    private EQ_QUALITY quality;
    private EQ_TYPE type;
    private EquipmentValue values;
    private bool isEquipped = false;
    private bool isInStorage = false;
    private float spb;//灵基值
    private int[] prefixMod_ids;//词缀
    private int baseMod_id;//基本名
    private List<ChipData> linkedChips = new List<ChipData>();
    public int eha_level;//强化等级
    public int eha_reha;//再塑等级
    public int eha_rebuild;//重构等级

    private static readonly Dictionary<EQ_TYPE, string> typeToNames = new Dictionary<EQ_TYPE, string>();
    private static readonly Dictionary<EQ_QUALITY, string> qualityToNames = new Dictionary<EQ_QUALITY, string>();

    private static string EQUIPMENT_PATH = "icons/equipments/";
    private static string EQUIPMENT_SUFFIX = ".png";

    public List<ChipData> LinkedChips
    {
        get
        {
            return linkedChips;
        }
    }

    static EquipmentBase()
    {
        typeToNames.Add(EQ_TYPE.CORE, "核心");
        typeToNames.Add(EQ_TYPE.CRAFT, "锻料工艺");
        typeToNames.Add(EQ_TYPE.MAIN_WEAPON, "主武器");
        typeToNames.Add(EQ_TYPE.DEPUTY_WEAPON, "副武器");
        typeToNames.Add(EQ_TYPE.MAIN_ARMOR, "主护具");
        typeToNames.Add(EQ_TYPE.DEPUTY_ARMOR, "副护具");
        qualityToNames.Add(EQ_QUALITY.ROUGH, "粗糙");
        qualityToNames.Add(EQ_QUALITY.NORMAL, "普通");
        qualityToNames.Add(EQ_QUALITY.GOOD, "良好");
        qualityToNames.Add(EQ_QUALITY.SUPERIOR, "精品");
    }


    public EquipmentBase(string id, string name, string dec, string icon, float spb, int price, EQ_QUALITY quality, EQ_TYPE type, EquipmentValue values) : base(id, name, dec, icon, price, 0)
    {
        this.quality = quality;
        this.type = type;
        this.values = values;
        this.spb = spb;
        ItemDataManager.LoadTargetItemIcon(this);
    }

    public void AddChip(ChipData chip)
    {
        linkedChips.Add(chip);
    }
    /// <summary>
    /// 返回该装备拥有的芯片的说明。
    /// </summary>
    /// <returns></returns>
    public string GetChipDescription()
    {
        if (linkedChips.Count == 0)
        {
            return "";
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<color=cyan>安装芯片：</color>");
        for (int i = 0; i < linkedChips.Count; i++)
        {
            sb.AppendLine(linkedChips[i].GetFullName(true));
        }
        return sb.ToString();
    }
    /// <summary>
    /// 设置武器是否被装备。
    /// 如果被装备，那他在进行人物属性刷新时会被计入属性统计。
    /// </summary>
    /// <param name="ifEquipped"></param>
    public void SetEquipped(bool ifEquipped)
    {
        isEquipped = ifEquipped;
    }
    /// <summary>
    /// 返回武器是否被装备的布尔值。
    /// </summary>
    /// <returns></returns>
    public bool IsEquipped
    {
        get
        {
            return isEquipped;
        }
    }
    /// <summary>
    /// 是否储存在仓库里。
    /// </summary>
    public bool IsInStorage
    {
        get
        {
            return isInStorage;
        }

        set
        {
            isInStorage = value;
        }
    }

    /// <summary>
    /// 获得该武器芯片装卸上限。
    /// </summary>
    /// <returns></returns>
    public int GetMaxChipDeployment()
    {
        int qualityLevel = (int)quality;
        return 1 + qualityLevel / 2;
    }
    /// <summary>
    /// 设置装备的词缀、基本名
    /// </summary>
    /// <param name="mod_id"></param>
    public void SetAffixIds(int[] affix_ids)
    {
        List<int> affixList = affix_ids.ToList();
        baseMod_id = affixList[affixList.Count - 1];
        affixList.RemoveAt(affixList.Count - 1);
        prefixMod_ids = affixList.ToArray();
    }
    /// <summary>
    /// 设置装备的强化等级
    /// </summary>
    /// <param name="levels"></param>
    public void SetEhaLevels(int[] levels)
    {
        eha_level = levels[0];
        eha_reha = levels[1];
        eha_rebuild = levels[2];
    }
    /// <summary>
    /// 得到装备基本名的id
    /// </summary>
    /// <returns></returns>
    public int GetBaseModId()
    {
        return baseMod_id;
    }

    /// <summary>
    /// 返回装备属性类
    /// </summary>
    /// <returns></returns>
    public EquipmentValue GetAttrs()
    {
        return values;
    }
    /// <summary>
    /// 返回装备受到一切加成后的属性。
    /// </summary>
    /// <returns></returns>
    public AttributeCollection GetActualAttrs()
    {
        var srcAttrs = values.values.Clone();
        for (int i = 0; i < linkedChips.Count; i++)
        {
            var chip = linkedChips[i];
            if (chip.IsEffect(Effects.ATTR))
            {
                srcAttrs += srcAttrs * chip.GetAttrColl();
            }
        }
        return srcAttrs;
    }
    /// <summary>
    /// 返回数量，武器固定为1
    /// </summary>
    /// <returns></returns>
    public override int GetAmount()
    {
        return 1;
    }
    /// <summary>
    /// 获得指定属性的数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetAttrValue(Attr type)
    {
        return values.GetValue(type);
    }

    public EQ_TYPE GetEquipmentType()
    {
        return type;
    }
    public EQ_QUALITY GetEquipmentQuality()
    {
        return quality;
    }
    /// <summary>
    /// 返回武器灵基
    /// </summary>
    /// <returns></returns>
    public float GetSpb()
    {
        return spb;
    }
    /// <summary>
    /// 返回带名字的richtext名,有颜色
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return GetName(name, quality);
    }
    /// <summary>
    /// 返回带名字的richtext名,有颜色
    /// </summary>
    /// <returns></returns>
    public static string GetName(string name, EQ_QUALITY quality)
    {
        StringBuilder sb = new StringBuilder();
        switch (quality)
        {
            case EQ_QUALITY.ROUGH:
                sb.Append("<color=#747474FF>");
                break;
            case EQ_QUALITY.NORMAL:
                sb.Append("<color=#D0D0D0FF>");
                break;
            case EQ_QUALITY.GOOD:
                sb.Append("<color=#5AFF7DFF>");
                break;
            case EQ_QUALITY.SUPERIOR:
                sb.Append("<color=#00ABFFFF>");
                break;
            default:
                sb.Append("<color=#FFFFFFFF>");
                break;
        }
        sb.Append(name);
        sb.Append("</color>");
        return sb.ToString();
    }
    /// <summary>
    /// 返回装备名字(后有 +0和对应的品质颜色)
    /// </summary>
    /// <returns></returns>
    public string GetModifyiedName()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(GetName());
        //后面为+x,目前+10+10封顶
        if (eha_level > 0)
        {
            if (eha_level == 10 && eha_reha == 10)
            {
                sb.Append(" <color=#FFD700FF>★</color>");
            }
            else
            {
                sb.Append(" +");
                sb.Append(eha_level);

            }
        }
        return sb.ToString();
    }
    /// <summary>
    /// 返回装备的再塑次数，如无则为null
    /// </summary>
    /// <returns></returns>
    public string GetRehanceString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<color=#26A86DFF>");
        if (eha_reha > 0)
        {
            sb.Append("[再塑 +");
            sb.Append(eha_reha);
            sb.Append("]");
        }
        else
        {
            sb.Append("[未再塑]");
        }
        sb.Append("</color>");
        return sb.ToString();
    }
    /// <summary>
    /// 返回的数为强化等级 + 再塑等级*11，即强化0级，再塑3的合计等级为33
    /// </summary>
    /// <returns></returns>
    public int GetAllocatedLevel()
    {
        return eha_level + eha_reha * 11;
    }
    /// <summary>
    /// 返回装备类型的名字
    /// </summary>
    /// <returns></returns>
    public string GetEqTypeName()
    {
        if (typeToNames.ContainsKey(type))
        {
            return typeToNames[type];
        }
        else
        {
            return "";
        }
    }

    public override string GetIconPath()
    {
        return EQUIPMENT_PATH + iconFileName + EQUIPMENT_SUFFIX;
    }
    public static string GetEqTypeNameByType(EQ_TYPE type)
    {
        return typeToNames.ContainsKey(type) ? typeToNames[type] : "";
    }
    public static string GetEqQualityNameByType(EQ_QUALITY quality)
    {
        return qualityToNames.ContainsKey(quality) ? qualityToNames[quality] : "";
    }
    public override bool CanBeSold()
    {
        return LinkedChips.Count == 0;
    }
    /// <summary>
    /// 通过再塑次数和强化等级获得真实等级。
    /// </summary>
    /// <param name="reha"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetRealLevel(int reha, int level)
    {
        return reha * 10 + level;
    }
}
/// <summary>
/// 装备品质
/// </summary>
public enum EQ_QUALITY
{
    ROUGH,//粗糙
    NORMAL,//普通
    GOOD,//良好
    SUPERIOR//精品
}
/// <summary>
/// 装备类型
/// </summary>
public enum EQ_TYPE
{
    CORE,//核心
    CRAFT,//工艺
    MAIN_WEAPON,//主武器
    DEPUTY_WEAPON,//副武器
    MAIN_ARMOR,//主防具
    DEPUTY_ARMOR,//副防具
    NOT_EQUIPMENT//非武器
}

/// <summary>
/// 武器拥有的属性
/// </summary>
public class EquipmentValue
{
    public AttributeCollection values = new AttributeCollection();
    public EquipmentValue(BaseAttribute attrs)
    {
        values.SetValues(attrs);
    }
    public float GetValue(Attr type)
    {
        return values.GetValue(type);
    }
}