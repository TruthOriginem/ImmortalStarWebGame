using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 每个单位用于短暂记录属性的值，用于战斗的各个回合。
/// </summary>
public class TempPropertyRecord{
    public float hp;
    public float mp;
    public Dictionary<PROPERTY_TYPE, float> tempProDic;

    public TempPropertyRecord(Dictionary<PROPERTY_TYPE, float> targetDic)
    {
        CloneProperty(ref targetDic);
        InitHpAndMp();
    }
    public TempPropertyRecord(Dictionary<PROPERTY_TYPE, IProperty> targetDic)
    {
        CloneProperty(ref targetDic);
        InitHpAndMp();
    }
    public TempPropertyRecord(List<SerializedIProperty> properties)
    {
        tempProDic = new Dictionary<PROPERTY_TYPE, float>();
        foreach(SerializedIProperty pro in properties)
        {
            tempProDic.Add(pro.type, pro.value);
        }
        InitHpAndMp();
    }

    public float GetValue(PROPERTY_TYPE type)
    {
        return tempProDic[type];
    }
    public void SetValue(PROPERTY_TYPE type,float value)
    {
        tempProDic[type] = value;
    }


    void InitHpAndMp()
    {
        hp = GetValue(PROPERTY_TYPE.MHP);
        mp = GetValue(PROPERTY_TYPE.MMP);
    }

    /// <summary>
    /// 将指定属性字典简单克隆
    /// </summary>
    /// <param name="targetDic"></param>
    void CloneProperty(ref Dictionary<PROPERTY_TYPE, float> targetDic)
    {
        tempProDic = new Dictionary<PROPERTY_TYPE, float>();
        foreach (var kv in targetDic)
        {
            tempProDic.Add(kv.Key, kv.Value);
        }
    }
    void CloneProperty(ref Dictionary<PROPERTY_TYPE, IProperty> targetDic)
    {
        tempProDic = new Dictionary<PROPERTY_TYPE, float>();
        foreach (var kv in targetDic)
        {
            tempProDic.Add(kv.Key, kv.Value.Value);
        }
    }

    /// <summary>
    /// 复制当前的Record
    /// </summary>
    /// <returns></returns>
    public TempPropertyRecord Clone()
    {
        TempPropertyRecord record = new TempPropertyRecord(tempProDic);
        record.hp = hp;
        record.mp = mp;
        return record;
    }
}
