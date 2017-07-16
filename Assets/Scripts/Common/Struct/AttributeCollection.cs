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
        Type type = typeof(Attrs);
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

    public Dictionary<Attr, float> GetValues()
    {
        return attrValues;
    }
    public static List<Attr> GetAllAttrs()
    {
        return attrNames;
    }
}
