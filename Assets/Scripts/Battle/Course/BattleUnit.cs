using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using GameId;

/// <summary>
/// 战斗单位，用来记录暂时的属性。（如玩家，敌人）
/// </summary>
public class BattleUnit
{
    public enum SIDE
    {
        PLAYER,
        ENEMY
    }
    public bool isDead = false;
    public SIDE side;
    public TempPropertyRecord tempRecord;
    public string name;
    public int level;
    public string id;
    public Dictionary<BaseSkill, int> skillToLevel;

    //private EnemyAttribute attr;

    public BattleUnit(string id, string name, int level, SIDE side, TempPropertyRecord record)
    {
        this.id = id;
        this.level = level;
        this.name = name;
        this.side = side;
        this.tempRecord = record;
    }
    /// <summary>
    /// 设置该单位的技能
    /// </summary>
    /// <param name="skillToLevel"></param>
    public void SetSkills(Dictionary<BaseSkill, int> skillToLevel)
    {
        this.skillToLevel = skillToLevel;
    }
    public int GetSkillLevel(BaseSkill skill)
    {
        return skillToLevel.ContainsKey(skill) ? skillToLevel[skill] : -1;
    }
    /// <summary>
    /// 获得该单位指定id技能的技能等级，若没有该技能则返回-1
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public int GetSkillLevel(string skillId)
    {
        var skill = SkillDataManager.GetSkillById(skillId);
        if (skill == null)
        {
            return -1;
        }
        return skillToLevel.ContainsKey(skill) ? skillToLevel[skill] : -1;
    }
    /// <summary>
    /// 设置该战斗单位的基本怪物信息
    /// </summary>
    public BattleUnit SetAttr(EnemyAttribute attr)
    {
        //this.attr = attr;
        return this;
    }
    /// <summary>
    /// 用来返回给textbox赋值。
    /// </summary>
    /// <returns></returns>
    public string GetPropertyDescription()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("生命值:" + tempRecord.hp.ToString("0.0") + "/" + GetValue(Attrs.MHP));
        sb.AppendLine("能量值:" + tempRecord.mp.ToString("0.0") + "/" + GetValue(Attrs.MMP));
        sb.AppendLine("攻击:" + GetValue(Attrs.ATK));
        sb.AppendLine("防御:" + GetValue(Attrs.DEF));
        sb.AppendLine("逻辑:" + GetValue(Attrs.LOG));
        sb.AppendLine("幸运:" + GetValue(Attrs.LCK));
        sb.AppendLine("暴击倍率:" + GetValue(Attrs.CRI));
        return sb.ToString();
    }
    public string GetValue(Attr type)
    {
        return tempRecord.GetValue(type).ToString("0.0");
    }

    /// <summary>
    /// 给该单位扣血，返回是否死亡
    /// </summary>
    /// <param name="value"></param>
    public bool DecreaseHp(Battle battle, float value)
    {
        tempRecord.hp -= value;
        if (tempRecord.hp <= 0f)
        {
            tempRecord.hp = 0;
            isDead = true;
            if (side == SIDE.ENEMY)
            {
                battle.enemyDie++;
            }
            else
            {
                battle.playerDie++;
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// 扣除能量值
    /// </summary>
    /// <param name="battle"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool DecreaseMp(Battle battle, float value)
    {
        bool decrease = tempRecord.mp >= value;
        if (decrease)
        {
            tempRecord.mp -= value;
        }
        return decrease;
    }
    /// <summary>
    /// 返回带颜色的战斗单位名字
    /// </summary>
    /// <returns></returns>
    public string GetNameBySide()
    {
        if (side == SIDE.ENEMY)
        {
            return "<color=#FF3D3DFF>" + name + "</color>";
        }
        else if (side == SIDE.PLAYER)
        {
            return "<color=#49FF9DFF>" + name + "</color>";
        }
        else
        {
            return name;
        }
    }
    public float GetNowHp()
    {
        return tempRecord.hp;
    }
    public float GetNowMp()
    {
        return tempRecord.mp;
    }
    /// <summary>
    /// 将当前属性记录替换为指定属性记录，主要用于播放回合时使用。
    /// </summary>
    /// <param name="record"></param>
    public void ChangeToRecord(TempPropertyRecord record)
    {
        this.tempRecord = record;
    }
}
