using GameId;
using System;
using System.Collections.Generic;
using System.Reflection;
/// <summary>
/// 属性集合类。初始值全部为0。
/// </summary>
public class AttributeCollection
{
    private Dictionary<Attr, float> attrValues = new Dictionary<Attr, float>();
    private static readonly List<Attr> attrCls = new List<Attr>();//所有属性静态类
    /// <summary>
    /// 利用反射得到属性集合中所有Attr的信息
    /// </summary>
    static AttributeCollection()
    {
        Type type = typeof(Attrs);
        var infos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var item in infos)
        {
            Attr attr = item.GetValue(null) as Attr;
            attrCls.Add(attr);
        }
    }

    public AttributeCollection()
    {
        for (int i = 0; i < attrCls.Count; i++)
        {
            attrValues.Add
                (attrCls[i], 0);
        }
    }
    /// <summary>
    /// 克隆当前属性集合。
    /// </summary>
    /// <returns></returns>
    public AttributeCollection Clone()
    {
        var newAttrs = new AttributeCollection();
        for (int i = 0; i < attrCls.Count; i++)
        {
            var attr = attrCls[i];
            newAttrs[attr] = this[attr];
        }
        return newAttrs;
    }
    /*
    public AttributeCollection Create(Dictionary<string,float> dic)
    {

    }
    */
    /// <summary>
    /// 根据Attr对象Key获取/修改对应float的Value值。
    /// </summary>
    /// <param name="attr">指定Attr属性</param>
    /// <returns></returns>
    public float this[Attr attr]
    {
        get
        {
            return GetValue(attr);
        }
        set
        {
            SetValue(attr, value);
        }
    }
    /// <summary>
    /// 根据指定属性得到属性值。
    /// </summary>
    /// <param name="attr"></param>
    /// <returns></returns>
    public float GetValue(Attr attr)
    {
        return attrValues.ContainsKey(attr) ? attrValues[attr] : 0;
    }
    /// <summary>
    /// 返回指定属性值字符串(保留小数点两位)
    /// </summary>
    /// <param name="attr"></param>
    /// <returns></returns>
    public string GetValueToString(Attr attr)
    {
        return GetValue(attr).ToString("0.00");
    }
    /// <summary>
    /// 设置指定属性。
    /// </summary>
    /// <param name="attr"></param>
    /// <param name="value"></param>
    public void SetValue(Attr attr, float value)
    {
        attrValues[attr] = value;
    }
    /// <summary>
    /// 根据序列化类BaseAttribute设置当前属性集合各项属性的值。
    /// </summary>
    /// <param name="tempAttrs"></param>
    public void SetValues(BaseAttribute tempAttrs)
    {
        attrValues[Attrs.ATK] = tempAttrs.atk;
        attrValues[Attrs.DEF] = tempAttrs.def;
        attrValues[Attrs.LOG] = tempAttrs.log;
        attrValues[Attrs.LCK] = tempAttrs.lck;
        attrValues[Attrs.CRI] = tempAttrs.cri;
        attrValues[Attrs.MHP] = tempAttrs.mhp;
        attrValues[Attrs.MMP] = tempAttrs.mmp;
    }
    public void SetValues(BaseModification tempAttrs)
    {
        attrValues[Attrs.ATK] = tempAttrs.atkMult;
        attrValues[Attrs.DEF] = tempAttrs.defMult;
        attrValues[Attrs.LOG] = tempAttrs.logMult;
        attrValues[Attrs.LCK] = tempAttrs.lckMult;
        attrValues[Attrs.CRI] = tempAttrs.criMult;
        attrValues[Attrs.MHP] = tempAttrs.mhpMult;
        attrValues[Attrs.MMP] = tempAttrs.mmpMult;
    }
    /// <summary>
    /// 将所有属性与指定BaseModification一一相乘
    /// </summary>
    /// <param name="modifyAttrs"></param>
    public void MultValues(BaseModification modifyAttrs)
    {
        attrValues[Attrs.ATK] *= modifyAttrs.atkMult;
        attrValues[Attrs.DEF] *= modifyAttrs.defMult;
        attrValues[Attrs.LOG] *= modifyAttrs.logMult;
        attrValues[Attrs.LCK] *= modifyAttrs.lckMult;
        attrValues[Attrs.CRI] *= modifyAttrs.criMult;
        attrValues[Attrs.MHP] *= modifyAttrs.mhpMult;
        attrValues[Attrs.MMP] *= modifyAttrs.mmpMult;
    }
    public void AddValues(float added)
    {
        foreach (var key in attrCls)
        {
            attrValues[key] += added;
        }
    }
    public void MultValues(float multi)
    {
        foreach (var key in attrCls)
        {
            attrValues[key] *= multi;
        }
    }

    /// <summary>
    /// 获得这个AttrColl中数值最大的属性。
    /// </summary>
    /// <returns></returns>
    public Attr GetMaxValueAttr()
    {
        float max = float.MinValue;
        Attr attr = Attrs.MHP;
        foreach (var kv in attrValues)
        {
            if (kv.Value > max)
            {
                max = kv.Value;
                attr = kv.Key;
            }
        }
        return attr;
    }
    /// <summary>
    /// 返回属性-值 的字典。
    /// </summary>
    /// <returns></returns>
    public Dictionary<Attr, float> GetValues()
    {
        return attrValues;
    }
    /// <summary>
    /// 返回所有属性的静态成员。
    /// </summary>
    /// <returns></returns>
    public static List<Attr> GetAllAttributes()
    {
        return attrCls;
    }
    public static AttributeCollection operator +(AttributeCollection l, AttributeCollection r)
    {
        AttributeCollection newA = new AttributeCollection();
        for (int i = 0; i < attrCls.Count; i++)
        {
            var attr = attrCls[i];
            newA[attr] = l.GetValue(attr) + r.GetValue(attr);
        }
        return newA;
    }
    public static AttributeCollection operator -(AttributeCollection l, AttributeCollection r)
    {
        AttributeCollection newA = new AttributeCollection();
        for (int i = 0; i < attrCls.Count; i++)
        {
            var attr = attrCls[i];
            newA[attr] = l.GetValue(attr) - r.GetValue(attr);
        }
        return newA;
    }
    public static AttributeCollection operator *(AttributeCollection l, AttributeCollection r)
    {
        AttributeCollection newA = new AttributeCollection();
        for (int i = 0; i < attrCls.Count; i++)
        {
            var attr = attrCls[i];
            newA[attr] = l.GetValue(attr) * r.GetValue(attr);
        }
        return newA;
    }
}
