using GameId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在战斗的某个流程中触发的技能
/// </summary>
public class DuringBattleTriggerSkill : BeforeBattleModiSkill
{
    protected Dictionary<string, float> idToPercentDict = new Dictionary<string, float>();
    public DuringBattleTriggerSkill() : base()
    {

    }
    public virtual float GetPercentModify(string id, int level)
    {
        return idToPercentDict.ContainsKey(id) ? idToPercentDict[id] * level : 0f;
    }
}
/// <summary>
/// 应急护盾技能 
/// </summary>
public class Skill_AnitcriShield : DuringBattleTriggerSkill
{
    public Skill_AnitcriShield(string id) : base()
    {
        this.id = id;
        this.name = "应急护盾";
        this.icon_name = id;
        this.description = "宇宙中到处充斥着出其不意，适当的在要害部位加装应急护盾是极其明智的决定。";
        this.documentation = "战斗中受到敌方暴击时，有50%的概率触发。消耗当前能量值的$,抵消本次暴击$的伤害。";
    }
    public override float GetPercentModify(string id, int level)
    {
        if (id == Infos.CHANCE)
        {
            return 50f;

        }
        else if (id == Infos.COST_CURR_ENERGY_PERCENT)
        {
            //1级15%的消耗，10级12%的消耗，最低5%的消耗
            return 10 * Mathf.Pow(level, -0.1549f) + 5;
        }
        else if (id == Infos.ELIMATE_CRI_HIT_PERCENT)
        {
            return -55 * Mathf.Pow(level, -0.245f) + 75;
        }
        return 0f;
    }
    public override string GetInfoInDocumentation(int level, int index)
    {
        if (index == 0)
        {
            return (GetPercentModify(Infos.COST_CURR_ENERGY_PERCENT, level)).ToString("0.0") + "%";
        }
        if (index == 1)
        {
            return (GetPercentModify(Infos.ELIMATE_CRI_HIT_PERCENT, level)).ToString("0.0") + "%";
        }
        return "";
    }
}
public class Skill_DyingBreak : DuringBattleTriggerSkill
{
    public Skill_DyingBreak(string id) : base()
    {
        this.id = id;
        this.name = "濒死爆发";
        this.icon_name = id;
        this.description = "当你浑身上下的结构都受到创伤时，你想到的不是保存实力或者战略性撤退，而是更加激烈地反击。";
        this.documentation = "当你的生命低于33%时，每次攻击都触发。消耗最大能量值的$,你的攻击将上升最高$(根据已损失生命提升)。";
    }
    public override float GetPercentModify(string id, int level)
    {
        if (id == Infos.NOW_HP_PERCENT)
        {
            return 33f;

        }
        else if (id == Infos.COST_MAX_ENERGY_PERCENT)
        {
            //1级20%的消耗，10级17%的消耗，最低10%的消耗
            return 10 * Mathf.Pow(level, -0.1549f) + 10;
        }
        else if (id == Infos.COST_ENERGY_FLAT)
        {
            return 20f + 10f * level;
        }
        else if (id == Infos.ADD_ATK_PERCENT)
        {
            return 48.5f + 1.5f * level;
        }
        return 0f;
    }
    public override string GetInfoInDocumentation(int level, int index)
    {
        if (index == 0)
        {
            return (GetPercentModify(Infos.COST_MAX_ENERGY_PERCENT, level)).ToString("0.0") + "%+" + (GetPercentModify(Infos.COST_ENERGY_FLAT, level)).ToString("0");
        }
        if (index == 1)
        {
            return (GetPercentModify(Infos.ADD_ATK_PERCENT, level)).ToString("0.0") + "%";
        }
        return "";
    }
}
