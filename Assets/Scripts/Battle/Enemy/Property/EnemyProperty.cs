using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson;

public class EnemyProperty {
    public AttributeCollection attrs = new AttributeCollection();
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
        attrs.SetValues(temp);
        exp = temp.dropExp;
    }
    public void Update(TempEnemyGrowthAttr temp)
    {
        attrs.SetValues(temp);
        exp = temp.dropExp;
    }
}
