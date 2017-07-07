using UnityEngine;
using System.Collections;

/// <summary>
/// 战斗/挂机结算时
/// </summary>
public class BattleAwardMult{
    public static float GetMoneyMult()
    {
        return 1f + PlayerInfoInGame.VIP_Level * 0.05f;
    }
    public static float GetExpMult()
    {
        return 1f + PlayerInfoInGame.VIP_Level * 0.05f;
    }
    public static float GetDropMult()
    {
        return 1f + PlayerInfoInGame.VIP_Level * 0.02f;
    }
    /// <summary>
    /// 远征掉落倍率
    /// </summary>
    /// <returns></returns>
    public static float GetExpeditionDropMult()
    {
        return 1f + PlayerInfoInGame.VIP_Level * 0.01f;
    }
}
