using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameId;

/// <summary>
/// 每个单位用于短暂记录属性的值，用于战斗的各个回合。
/// </summary>
public class TempPropertyRecord{
    public float hp;
    public float mp;
    public AttributeCollection attrs = new AttributeCollection();

    public TempPropertyRecord(AttributeCollection nattrs)
    {
        CloneProperty(nattrs);
        InitHpAndMp();
    }

    public float GetValue(Attr type)
    {
        return attrs.GetValue(type);
    }
    public void SetValue(Attr type,float value)
    {
        attrs.SetValue(type,value);
    }


    void InitHpAndMp()
    {
        hp = GetValue(Attrs.MHP);
        mp = GetValue(Attrs.MMP);
    }

    /// <summary>
    /// 将指定属性字典简单克隆
    /// </summary>
    /// <param name="targetDic"></param>
    void CloneProperty(AttributeCollection attrs)
    {
        this.attrs = attrs.Clone();
    }

    /// <summary>
    /// 复制当前的Record
    /// </summary>
    /// <returns></returns>
    public TempPropertyRecord Clone()
    {
        TempPropertyRecord record = new TempPropertyRecord(attrs);
        record.hp = hp;
        record.mp = mp;
        return record;
    }
}
