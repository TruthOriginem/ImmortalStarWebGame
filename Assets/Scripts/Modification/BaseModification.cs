using UnityEngine;
using System.Collections;
using System.Text;

/// <summary>
/// 基本名，词缀等对装备有加成的基类
/// </summary>
[System.Serializable]
public class BaseModification
{
    public int id;
    public string name;
    public string description;
    public float mhpMult;
    public float mmpMult;
    public float atkMult;
    public float defMult;
    public float logMult;
    public float lckMult;
    public float criMult;
    /// <summary>
    /// 获得"+X%最大生命,+X%..."格式
    /// </summary>
    /// <returns></returns>
    public string GetPlusDescription()
    {
        StringBuilder sb = new StringBuilder();
        if (mhpMult != 1f)
        {
            bool greater = mhpMult > 1f;
            string value = ((greater ? mhpMult - 1f : 1f - mhpMult) * 100f).ToString("0.0");
            sb.Append(greater ? "+" : "-");
            sb.Append(value);
            sb.Append("%最大生命,");
        }
        if (mmpMult != 1f)
        {
            bool greater = mmpMult > 1f;
            string value = ((greater ? mmpMult - 1f : 1f - mmpMult) * 100f).ToString("0.0");
            sb.Append(greater ? "+" : "-");
            sb.Append(value);
            sb.Append("%最大能量,");
        }
        if (atkMult != 1f)
        {
            bool greater = atkMult > 1f;
            string value = ((greater ? atkMult - 1f : 1f - atkMult) * 100f).ToString("0.0");
            sb.Append(greater ? "+" : "-");
            sb.Append(value);
            sb.Append("%攻击,");
        }
        if (defMult != 1f)
        {
            bool greater = defMult > 1f;
            string value = ((greater ? defMult - 1f : 1f - defMult) * 100f).ToString("0.0");
            sb.Append(greater ? "+" : "-");
            sb.Append(value);
            sb.Append("%防御,");
        }
        if (logMult != 1f)
        {
            bool greater = logMult > 1f;
            string value = ((greater ? logMult - 1f : 1f - logMult) * 100f).ToString("0.0");
            sb.Append(greater ? "+" : "-");
            sb.Append(value);
            sb.Append("%逻辑,");
        }
        if (lckMult != 1f)
        {
            bool greater = lckMult > 1f;
            string value = ((greater ? lckMult - 1f : 1f - lckMult) * 100f).ToString("0.0");
            sb.Append(greater ? "+" : "-");
            sb.Append(value);
            sb.Append("%幸运,");
        }
        if (criMult != 1f)
        {
            bool greater = criMult > 1f;
            string value = ((greater ? criMult - 1f : 1f - criMult) * 100f).ToString("0.0");
            sb.Append(greater ? "+" : "-");
            sb.Append(value);
            sb.Append("%暴击倍率,");
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
}
[System.Serializable]
public class EquipModification : BaseModification
{
    public int eqQuality;

    public EQ_QUALITY GetQuality()
    {
        return (EQ_QUALITY)eqQuality;
    }
}