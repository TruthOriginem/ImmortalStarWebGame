using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum PROPERTY_TYPE
{
    MHP,
    MMP,
    ATK,
    DEF,
    LOG,
    LCK,
    CRI,
    NULL
}
public interface IProperty
{
    float Value { get; set; }
    string GetName();
    string GetDescription();
    string GetValueToString();
    PROPERTY_TYPE GetPropertyType();
}
public class BaseProperty : IProperty
{
    float value = 0;
    PROPERTY_TYPE type;

    public BaseProperty()
    {
    }
    public BaseProperty(float value)
    {
        this.value = value;
    }
    public BaseProperty(PROPERTY_TYPE type, float value)
    {
        this.value = value;
        this.type = type;
    }

    public float Value
    {
        get
        {
            return this.value;
        }

        set
        {
            this.value = value;
        }
    }

    public string GetValueToString()
    {
        return value.ToString("0.00");
    }

    public virtual string GetDescription()
    {
        return "";
    }

    public virtual string GetName()
    {
        return GetName(type);
    }

    public virtual PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.NULL;
    }

    public static string GetName(PROPERTY_TYPE type)
    {
        switch (type)
        {
            case PROPERTY_TYPE.MHP:
                return "最大生命";
            case PROPERTY_TYPE.MMP:
                return "最大能量";
            case PROPERTY_TYPE.ATK:
                return "攻击";
            case PROPERTY_TYPE.DEF:
                return "防御";
            case PROPERTY_TYPE.LOG:
                return "逻辑";
            case PROPERTY_TYPE.LCK:
                return "幸运";
            case PROPERTY_TYPE.CRI:
                return "暴击倍率";
            case PROPERTY_TYPE.NULL:
                return "无";

        }
        return "";
    }
}
/// <summary>
/// 应与Propertytype相对应。
/// </summary>
[System.Serializable]
public class TempAttribute
{
    public float mhp;
    public float mmp;
    public float atk;
    public float def;
    public float log;
    public float lck;
    public float cri;

    /// <summary>
    /// 将所有属性乘以一个系数
    /// </summary>
    /// <param name="mult"></param>
    public void MultAllProperties(float mult)
    {
        mhp *= mult;
        mmp *= mult;
        atk *= mult;
        def *= mult;
        log *= mult;
        lck *= mult;
        cri *= mult;
    }
}

public class PropMhp : BaseProperty
{
    public PropMhp()
    {

    }
    public PropMhp(float value) : base(value)
    {
    }

    public override string GetName()
    {
        return "最高生命";
    }
    public override PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.MHP;
    }
}
public class PropMmp : BaseProperty
{
    public PropMmp()
    {

    }
    public PropMmp(float value) : base(value)
    {
    }

    public override string GetName()
    {
        return "最高能量";
    }
    public override PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.MMP;
    }
}
public class PropAtk : BaseProperty
{
    public PropAtk()
    {

    }
    public PropAtk(float value) : base(value)
    {
    }

    public override string GetName()
    {
        return "攻击";
    }
    public override PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.ATK;
    }
}
public class PropDef : BaseProperty
{
    public PropDef()
    {

    }
    public PropDef(float value) : base(value)
    {
    }

    public override string GetName()
    {
        return "防御";
    }
    public override PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.DEF;
    }
}
public class PropLog : BaseProperty
{
    public PropLog()
    {

    }
    public PropLog(float value) : base(value)
    {
    }

    public override string GetName()
    {
        return "逻辑";
    }
    public override PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.LOG;
    }
}
public class PropLck : BaseProperty
{
    public PropLck()
    {
    }

    public PropLck(float value) : base(value)
    {
    }

    public override string GetName()
    {
        return "幸运";
    }
    public override PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.LCK;
    }
}
public class PropCri : BaseProperty
{
    public PropCri()
    {
    }

    public PropCri(float value) : base(value)
    {
    }

    public override string GetName()
    {
        return "暴击倍率";
    }
    public override PROPERTY_TYPE GetPropertyType()
    {
        return PROPERTY_TYPE.CRI;
    }
}