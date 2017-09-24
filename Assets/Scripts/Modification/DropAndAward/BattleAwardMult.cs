using UnityEngine;
using System.Collections;

/// <summary>
/// 战斗/挂机结算时
/// </summary>
public class BattleAwardMult{
    public static float GetMoneyMult(int level = -1)
    {
        if (level == -1) level = PlayerInfoInGame.VIP_Level;
        return 1f + level * 0.05f;
    }
    public static float GetExpMult(int level = -1)
    {
        if (level == -1) level = PlayerInfoInGame.VIP_Level;
        return 1f + level * 0.05f;
    }
    public static float GetDropMult(int level = -1)
    {
        if (level == -1) level = PlayerInfoInGame.VIP_Level;
        return 1f + level * 0.02f;
    }
    /// <summary>
    /// 远征掉落倍率
    /// </summary>
    /// <returns></returns>
    public static float GetExpeditionDropMult(int level = -1)
    {
        if (level == -1) level = PlayerInfoInGame.VIP_Level;
        return 1f + level * 0.01f;
    }
}
