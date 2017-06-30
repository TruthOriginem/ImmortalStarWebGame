using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 负责战斗前/中对战斗单位属性进行修正的类。
/// </summary>
public class UnitModifyManager
{
    /// <summary>
    /// 根据拥有的技能与等级进行修改
    /// </summary>
    /// <param name="skillToLevels"></param>
    /// <param name="record"></param>
    public static void ModifyRecordBeforeBattle(Dictionary<BeforeBattleModiSkill, int> skillToLevels, TempPropertyRecord record)
    {
        PROPERTY_TYPE[] types = record.tempProDic.Keys.ToArray();
        foreach (var kv in skillToLevels)
        {
            foreach(var type in types)
            {
                float percent = kv.Key.GetPercentModify(type, kv.Value);
                if (percent != 0f)
                {
                    record.SetValue(type, record.GetValue(type) * (1f + percent * 0.01f));
                }
            }
        }
    }
}
