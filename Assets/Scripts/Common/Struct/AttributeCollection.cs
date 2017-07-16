using GameId;
using System;
using System.Collections.Generic;
using System.Reflection;
/// <summary>
/// 属性集合类。
/// </summary>
public class AttributeCollection
{
    private Dictionary<Attr, float> attrValues = new Dictionary<Attr, float>();
    public static List<Attr> attrNames;
    /// <summary>
    /// 利用反射得到属性集合中所有Attr的信息
    /// </summary>
    static AttributeCollection()
    {
        attrNames = new List<Attr>();
        Type type = typeof(GameId.Attrs);
        var infos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var item in infos)
        {
            Attr attr = item.GetValue(null) as Attr;
            attrNames.Add(attr);
        }
    }

    public AttributeCollection()
    {
        for (int i = 0; i < attrNames.Count; i++)
        {
            attrValues.Add
                (attrNames[i], 0);
        }
    }

    public AttributeCollection Clone()
    {
        var newAttrs = new AttributeCollection();
        for (int i = 0; i < attrNames.Count; i++)
        {
            var attr = attrNames[i];
            newAttrs.SetValue(attr, GetValue(attr));
        }
        return newAttrs;
    }
    /*
    public AttributeCollection Create(Dictionary<string,float> dic)
    {

    }
    */
    /// <summary>
    /// 根据属性得到属性值。
    /// </summary>
    /// <param name="attr"></param>
    /// <returns></returns>
    public float GetValue(Attr attr)
    {
        return attrValues.ContainsKey(attr) ? attrValues[attr] : 0;
    }
    public string GetValueToString(Attr attr)
    {
        return GetValue(attr).ToString("0.00");
    }
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
        attrValues[GameId.Attrs.ATK] = tempAttrs.atk;
        attrValues[GameId.Attrs.DEF] = tempAttrs.def;
        attrValues[GameId.Attrs.LOG] = tempAttrs.log;
        attrValues[GameId.Attrs.LCK] = tempAttrs.lck;
        attrValues[GameId.Attrs.CRI] = tempAttrs.cri;
        attrValues[GameId.Attrs.MHP] = tempAttrs.mhp;
        attrValues[GameId.Attrs.MMP] = tempAttrs.mmp;
    }
    public void SetValues(BaseModification tempAttrs)
    {
        attrValues[GameId.Attrs.ATK] = tempAttrs.atkMult;
        attrValues[GameId.Attrs.DEF] = tempAttrs.defMult;
        attrValues[GameId.Attrs.LOG] = tempAttrs.logMult;
        attrValues[GameId.Attrs.LCK] = tempAttrs.lckMult;
        attrValues[GameId.Attrs.CRI] = tempAttrs.criMult;
        attrValues[GameId.Attrs.MHP] = tempAttrs.mhpMult;
        attrValues[GameId.Attrs.MMP] = tempAttrs.mmpMult;
    }
    public void MultValues(BaseModification modifyAttrs)
    {
        attrValues[GameId.Attrs.ATK] *= modifyAttrs.atkMult;
        attrValues[GameId.Attrs.DEF] *= modifyAttrs.defMult;
        attrValues[GameId.Attrs.LOG] *= modifyAttrs.logMult;
        attrValues[GameId.Attrs.LCK] *= modifyAttrs.lckMult;
        attrValues[GameId.Attrs.CRI] *= modifyAttrs.criMult;
        attrValues[GameId.Attrs.MHP] *= modifyAttrs.mhpMult;
        attrValues[GameId.Attrs.MMP] *= modifyAttrs.mmpMult;
    }

    public Dictionary<Attr, float> GetValues()
    {
        return attrValues;
    }
    public static List<Attr> GetAllAttrs()
    {
        return attrNames;
    }
    public static AttributeCollection operator +(AttributeCollection l, AttributeCollection r)
    {
        AttributeCollection newA = new AttributeCollection();
        for (int i = 0; i < attrNames.Count; i++)
        {
            var attr = attrNames[i];
            newA.SetValue(attr, l.GetValue(attr) + r.GetValue(attr));
        }
        return newA;
    }
    public static AttributeCollection operator -(AttributeCollection l, AttributeCollection r)
    {
        AttributeCollection newA = new AttributeCollection();
        for (int i = 0; i < attrNames.Count; i++)
        {
            var attr = attrNames[i];
            newA.SetValue(attr, l.GetValue(attr) - r.GetValue(attr));
        }
        return newA;
    }
}
