﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameId;

/// <summary>
/// 战斗类，当玩家正式攻击目标怪物，并打开战斗界面时实例化。
/// 实例化的时候便开始生成回合
/// </summary>
public class Battle
{
    /// <summary>
    /// 回合类
    /// </summary>
    public List<BattleCourse> courses = new List<BattleCourse>();
    /// <summary>
    /// 我方单位
    /// </summary>
    public List<BattleUnit> playerUnits = new List<BattleUnit>();
    /// <summary>
    /// 敌方单位
    /// </summary>
    public List<BattleUnit> enemyUnits = new List<BattleUnit>();

    public EnemySpawnData enemySpawnData;

    public int enemyDie = 0;
    public int playerDie = 0;

    /// <summary>
    /// 用来记录是否有重复名字的
    /// </summary>
    private Dictionary<string, int> enemyCount = new Dictionary<string, int>();


    public Battle(object enemyData)
    {
        if (enemyData is EnemySpawnData)
        {
            enemySpawnData = enemyData as EnemySpawnData;
            GenerateWholeBattle();
        }
        if (enemyData is MachineMatchManager)
        {
            InitMachineMatchPVPUnits(MachineMatchManager.CurrentTarget);
        }
        if (enemyData is DeepMemoryManager)
        {
            var manager = enemyData as DeepMemoryManager;
            InitDeepMemoryUnits(manager.GetEnemyAttrByDepth(), manager.GetEnemyLevel());
        }
    }
    public Battle(EnemySpawnData data)
    {
        enemySpawnData = data;
        GenerateWholeBattle();
    }

    private void GenerateWholeBattle()
    {
        InitAllUnits();
    }
    /// <summary>
    /// 初始化所有单位，包括玩家，敌人
    /// </summary>
    private void InitAllUnits()
    {
        InitPlayerUnit();
        foreach (EnemyGroup group in enemySpawnData.enemyGroups)
        {
            for (int i = 0; i < group.amount; i++)
            {
                BaseEnemy baseEnemy = group.enemy;
                EnemyAttribute attr = EnemyDataManager.AskForEnemyAttribute(baseEnemy.id);
                string name = attr.name;
                name += "(Lv." + baseEnemy.Level + ")";
                string index = "";
                if (enemyCount.ContainsKey(name))
                {
                    index = "(" + (++enemyCount[name]) + ")";
                }
                else
                {
                    enemyCount.Add(name, 1);
                }
                name += index;
                var dic = EnemyDataManager.GenerateAttrs(attr, baseEnemy.Level);
                TempPropertyRecord record = new TempPropertyRecord(dic);
                var enemy = new BattleUnit(attr.id, name, baseEnemy.Level, BattleUnit.SIDE.ENEMY, record);
                enemy.SetSkills(new Dictionary<BaseSkill, int>());
                enemyUnits.Add(enemy);
            }
        }
    }
    private void InitMachineMatchPVPUnits(PlayerUnit enemy)
    {
        InitPlayerUnit();
        enemyUnits.Add(enemy.CreateBattleUnit(BattleUnit.SIDE.ENEMY));
    }
    private void InitDeepMemoryUnits(EnemyAttribute attr, lint level)
    {
        InitPlayerUnit();
        string name = attr.name;
        name += "(Lv." + level + ")";
        var dic = EnemyDataManager.GenerateAttrs(attr, level);
        TempPropertyRecord record = new TempPropertyRecord(dic);
        var enemy = new BattleUnit(attr.id, name, level, BattleUnit.SIDE.ENEMY, record);
        enemy.SetSkills(new Dictionary<BaseSkill, int>());
        enemyUnits.Add(enemy);
    }

    public void InitPlayerUnit()
    {
        TempPropertyRecord playerRecord = new TempPropertyRecord(PlayerInfoInGame.Instance.GetDynamicAttrs());
        //战前技能属性调整
        UnitModifyManager.ModifyRecordBeforeBattle(SkillDataManager.GetPlayersBeforeBattleSkill(), playerRecord);
        BattleUnit playerUnit = new BattleUnit(PlayerInfoInGame.Id, PlayerInfoInGame.NickName, PlayerInfoInGame.Level, BattleUnit.SIDE.PLAYER, playerRecord);
        playerRecord.hp = playerRecord.GetValue(Attrs.MHP);
        playerRecord.mp = playerRecord.GetValue(Attrs.MMP);
        playerUnit.SetSkills(SkillDataManager.GetPlayersDuringBattleSkill());
        playerUnits.Add(playerUnit);
    }
    /// <summary>
    /// 如果死的敌人=所有敌人或者死的玩家=所有玩家则表示应该结束了。
    /// </summary>
    /// <returns></returns>
    public bool ShouldEnd()
    {
        return enemyDie == enemyUnits.Count || playerDie == playerUnits.Count;
    }
    /// <summary>
    /// 判断玩家是否胜利。判断依据是：玩家死亡数量小于总数，敌人死亡数量=敌人原本数量。
    /// </summary>
    /// <returns></returns>
    public bool IsWin()
    {
        return playerDie < playerUnits.Count && enemyDie == enemyUnits.Count;
    }
}
