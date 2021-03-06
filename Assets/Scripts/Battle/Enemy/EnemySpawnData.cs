﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 怪物的生成数据，里面包含敌组（敌人种类，数量）。
/// </summary>
[System.Serializable]
public class EnemySpawnData
{
    public List<EnemyGroup> enemyGroups;

    /// <summary>
    /// 根据指定地区的极限等级，返回正确的怪物等级
    /// </summary>
    /// <param name="stageId"></param>
    /// <returns></returns>
    public EnemySpawnData GetActualData(string stageId)
    {
        var data = ScenarioManager.GetStageDataById(stageId);
        if (data.ExtremeLevel > 0)
        {
            var spawnData = new EnemySpawnData();
            List<EnemyGroup> newGroups = new List<EnemyGroup>();
            for (int i = 0; i < enemyGroups.Count; i++)
            {
                var group = enemyGroups[i];
                var enemy = new BaseEnemy();
                enemy.id = group.enemy.id;
                enemy.Level = Mathf.RoundToInt(group.enemy.Level * data.GetMultiExLevelFactor());
                enemy.showIcon = group.enemy.showIcon;
                var newGroup = new EnemyGroup();
                newGroup.enemy = enemy;
                newGroup.amount = group.amount;
                newGroups.Add(newGroup);
            }
            spawnData.enemyGroups = newGroups;
            return spawnData;
        }
        else
        {
            return this;
        }
    }
    /// <summary>
    /// 获得该SpawnData生成怪物总数。
    /// </summary>
    /// <returns></returns>
    public int GetEnemiesCount()
    {
        int count = 0;
        for (int i = 0; i < enemyGroups.Count; i++)
        {
            var group = enemyGroups[i];
            count += group.amount;
        }
        return count;
    }
}
