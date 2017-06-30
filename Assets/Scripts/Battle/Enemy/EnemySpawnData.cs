using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 怪物的生成数据，里面包含敌组（敌人种类，数量）。
/// </summary>
[System.Serializable]
public class EnemySpawnData{
    public List<EnemyGroup> enemyGroups;
}
