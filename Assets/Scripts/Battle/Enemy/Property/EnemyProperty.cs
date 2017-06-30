using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson;

public class EnemyProperty {
    public List<SerializedIProperty> properties = new List<SerializedIProperty>();
    public int exp;
    public EnemyProperty(){}
    public EnemyProperty(TempEnemyAttribute temp)
    {
        Update(temp);
    }
    /// <summary>
    /// 将temp内的一般属性录入到EnemyProperty，不管其他的内容
    /// </summary>
    /// <param name="temp"></param>
    public void Update(TempEnemyAttribute temp)
    {
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.MHP, temp.mhp));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.MMP, temp.mmp));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.ATK, temp.atk));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.DEF, temp.def));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.LOG, temp.log));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.LCK, temp.lck));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.CRI, temp.cri));
        exp = temp.dropExp;
    }
    public void Update(TempEnemyGrowthAttr temp)
    {
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.MHP, temp.mhp));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.MMP, temp.mmp));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.ATK, temp.atk));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.DEF, temp.def));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.LOG, temp.log));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.LCK, temp.lck));
        properties.Add(new SerializedIProperty(PROPERTY_TYPE.CRI, temp.cri));
        exp = temp.dropExp;
    }
}
