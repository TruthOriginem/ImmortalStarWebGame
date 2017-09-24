using GameId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 用于存储关卡信息的序列化类
/// </summary>
[System.Serializable]
public class BatInsGridData
{
    public int index;
    /// <summary>
    /// 该关卡隶属剧本的id
    /// </summary>
    public string sId;
    /// <summary>
    /// 该关卡的id
    /// </summary>
    public string id;
    /// <summary>
    /// 该关卡的名字。
    /// </summary>
    public string name;
    /// <summary>
    /// 该关卡的描述。
    /// </summary>
    public string des;
    /// <summary>
    /// 该关卡是不是Boss关卡
    /// </summary>
    public bool isBoss;
    public bool isGold = false;
    public InstanceGridLimitation limit;
    public EnemySpawnData enemys;
    public EquipmentDropBundle eqDrop;

    private int attackCount;
    private bool isCompleted = false;
    private bool interactive;

    private static Dictionary<BatInsGridData, BatStageData> gridsToStage = new Dictionary<BatInsGridData, BatStageData>();
    //private BatStageData parentStageData;

    /// <summary>
    /// 设置是否可交互，当挑战次数用完就不能交互，前置关卡均未解锁不能交互。
    /// </summary>
    public bool Interactive
    {
        get
        {
            return interactive;
        }

        set
        {
            interactive = value;
        }
    }
    /// <summary>
    /// 隶属的StageData
    /// </summary>
    public BatStageData GetParentStageData()
    {
        return gridsToStage[this];
    }
    public void SetParentStageData(BatStageData data)
    {
        if (gridsToStage.ContainsKey(this))
        {
            gridsToStage[this] = data;
        }
        else
        {
            gridsToStage.Add(this, data);
        }
    }

    public void SetAttackCount(int count)
    {
        attackCount = count;
    }
    /// <summary>
    /// 返回客户端记录中对这个关卡的攻击次数。
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public int GetAttackCount()
    {
        return attackCount;
    }
    /// <summary>
    /// 设置该关卡不能被攻击。
    /// </summary>
    public void SetCanNotAttack()
    {
        interactive = false;
    }
    /// <summary>
    /// 当该关卡被顺利攻破，则会加入数据库词条并视作已完成。
    /// </summary>
    /// <param name="isCompleted"></param>
    /// <returns></returns>
    public bool SetCompleted(bool isCompleted)
    {
        this.isCompleted = isCompleted;
        return isCompleted;
    }
    /// <summary>
    /// 返回该关卡是否已完成(成功攻破)。当该关卡被顺利攻破，则会加入数据库词条并视作已完成。
    /// 需要注意，这不包括极限等级。
    /// </summary>
    /// <returns></returns>
    public bool IsCurrentCompleted()
    {
        return isCompleted;
    }
    /// <summary>
    /// 进行过极限重置的地区里，该关卡是一定完成过的。
    /// </summary>
    /// <returns></returns>
    public bool IsOnceCompleted()
    {
        return GetParentStageData().ExtremeLevel > 0;
    }
    /// <summary>
    /// 返回是否可以用重置粉末进行额外挑战
    /// </summary>
    /// <returns></returns>
    public bool CanUseResetPowder()
    {
        int amount;
        return CanUseResetPowder(out amount);
    }
    /// <summary>
    /// 返回是否可以用重置粉末进行额外挑战，并且需要几个粉末。
    /// <para>如果返回false，则说明该关卡不是boss/挑战次数无限/未达到挑战次数上限</para>
    /// </summary>
    /// <returns></returns>
    public bool CanUseResetPowder(out int amount)
    {
        amount = 0;
        if (!isBoss) return false;
        int attackCount = GetAttackCount();
        if (limit.attackTimesPerDay == -1 || (limit.attackTimesPerDay > attackCount)) return false;
        //攻击关卡溢出次数
        int overflowCount = attackCount - limit.attackTimesPerDay;
        //每溢出两次，所需粉末数量+1
        amount = overflowCount / 2 + 1;
        return true;
    }

}
