using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameId;
/// <summary>
/// 回合类，一回合指的是，玩家方全部行动一遍，敌方全部行动一遍
/// </summary>
public class BattleCourse
{
    /// <summary>
    /// 最终伤害所拥有的浮动。
    /// </summary>
    private static float DAMAGE_FLOAT_PERCENT = 0.1f;
    public enum TAG
    {
        START,
        NORMAL,
        END
    }

    private TAG tag = TAG.NORMAL;

    private Battle battle;
    private int index;
    public float timeToWait;


    public Dictionary<BattleUnit, TempPropertyRecord> changedProperty = new Dictionary<BattleUnit, TempPropertyRecord>();
    /// <summary>
    /// 记录该回合发生了什么事的
    /// </summary>
    public string progressText;

    public BattleCourse(Battle battle, TAG tag, int index)
    {
        this.battle = battle;
        this.tag = tag;
        this.index = index;
    }

    public bool GenerateCourse()
    {
        StringBuilder sb = new StringBuilder();
        if (tag == TAG.START)
        {
            sb.AppendLine("<size=22>战斗开始!</size>");
        }
        else if (tag == TAG.END)
        {
            sb.AppendLine("<size=22>战斗结束!</size>");
        }
        else
        {
            sb.AppendLine("<color=#FFC45BFF><size=20><b>第" + index + "回合</b></size></color>");
            //玩家的回合
            foreach (BattleUnit unit in battle.playerUnits)
            {
                if (unit.isDead) continue;
                int enemyIndex;
                BattleUnit enemy;
                int count = 0;
                do
                {
                    enemyIndex = Random.Range(0, battle.enemyUnits.Count);
                    enemy = battle.enemyUnits[enemyIndex];
                    count++;
                } while (enemy.isDead && count < 20);

                if (count >= 20)
                {
                    break;
                }
                bool dead = Attack(unit, enemy, sb);
                if (dead)
                {
                    sb.AppendLine(enemy.GetNameBySide() + "失去了生命力。");
                }
                if (battle.ShouldEnd())
                {
                    break;
                }
            }
            //怪物们的回合
            if (!battle.ShouldEnd())
            {
                foreach (BattleUnit unit in battle.enemyUnits)
                {
                    if (unit.isDead) continue;
                    int enemyIndex;
                    BattleUnit enemy;
                    int count = 0;
                    do
                    {
                        enemyIndex = Random.Range(0, battle.playerUnits.Count);
                        enemy = battle.playerUnits[enemyIndex];
                        count++;
                    } while (enemy.isDead && count < 20);
                    if (count >= 20)
                    {
                        break;
                    }

                    bool dead = Attack(unit, enemy, sb);
                    if (dead)
                    {
                        sb.AppendLine(enemy.GetNameBySide() + "失去了生命力。");
                    }
                    if (battle.ShouldEnd())
                    {
                        break;
                    }
                }
            }
        }
        timeToWait = Random.Range(1.5f, 2f);
        progressText = sb.ToString();
        SaveUnitChange();
        return battle.ShouldEnd();
    }
    /// <summary>
    /// 将当前状态替换为本回合的状态，并返回本回合的流程文字
    /// </summary>
    /// <returns></returns>
    public string PlayCourse()
    {
        foreach (var kv in changedProperty)
        {
            kv.Key.tempRecord = kv.Value;
        }
        return progressText;
    }
    /// <summary>
    /// 做出普通攻击行动，返回这次攻击是否造成to的死亡
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="sb"></param>
    /// <returns></returns>
    bool Attack(BattleUnit from, BattleUnit to, StringBuilder sb)
    {
        //获取属性
        float f_atk = GetValue(from, Attrs.ATK);
        float f_log = GetValue(from, Attrs.LOG);
        float f_lck = GetValue(from, Attrs.LCK);
        float f_cri = GetValue(from, Attrs.CRI);
        float f_mhp = GetValue(from, Attrs.MHP);
        float f_mmp = GetValue(from, Attrs.MMP);
        float t_log = GetValue(to, Attrs.LOG);
        float t_lck = GetValue(to, Attrs.LCK);
        float t_def = GetValue(to, Attrs.DEF);
        //属性buff
        #region 属性Buff
        if (from.GetSkillLevel(Skills.DYING_BREAK) > 0)
        {
            //Debug.Log("触发！");
            var skill = SkillDataManager.GetSkillById(Skills.DYING_BREAK) as Skill_DyingBreak;
            int level = from.GetSkillLevel(Skills.DYING_BREAK);
            if (from.GetNowHp() * 100 < f_mhp * skill.GetPercentModify(Infos.NOW_HP_PERCENT, level))
            {
                float mp = skill.GetPercentModify(Infos.COST_MAX_ENERGY_PERCENT, level) * 0.01f * f_mmp + skill.GetPercentModify(Infos.COST_ENERGY_FLAT, level);
                //Debug.Log(mp);
                if (from.DecreaseMp(battle, mp))
                {
                    f_atk += f_atk * 0.01f * skill.GetPercentModify(Infos.ADD_ATK_PERCENT, level);
                    sb.Append(from.GetNameBySide());
                    sb.Append("濒死爆发！这个回合里攻击上升为");
                    sb.Append(TextUtils.GetColoredText(f_atk.ToString("0"),Color.red));
                    sb.AppendLine("!");
                }
            }

        }
        #endregion
        //弱点击破
        bool weakBreak = GetWeakBreak(ref t_def, f_log, t_log);
        //得到最终伤害
        float damage = GetDamage(f_atk, t_def);
        bool criticalHit = GetCriticalHit(ref damage, f_lck, t_lck, f_cri);
        #region 对暴击后伤害产生影响
        if (criticalHit)
        {
            if (to.GetSkillLevel(Skills.ANTICRI_SHIELD) > 0)
            {
                int level = to.GetSkillLevel(Skills.ANTICRI_SHIELD);
                var skill = SkillDataManager.GetSkillById(Skills.ANTICRI_SHIELD) as DuringBattleTriggerSkill;
                if (Random.value * 100 < skill.GetPercentModify(Infos.CHANCE, level))
                {
                    float takeMp = to.tempRecord.mp * (skill.GetPercentModify(Infos.COST_CURR_ENERGY_PERCENT, level) / 100f);
                    if (to.DecreaseMp(battle, takeMp))
                    {
                        float elimateDamage = damage * (skill.GetPercentModify(Infos.ELIMATE_CRI_HIT_PERCENT, level) / 100f);
                        damage -= elimateDamage;
                        sb.Append(to.GetNameBySide());
                        sb.Append("触发了");
                        sb.Append(TextUtils.GetYellowText(skill.GetName()));
                        sb.Append(",抵消了来自");
                        sb.Append(from.GetNameBySide());
                        sb.Append(elimateDamage.ToString("0.0"));
                        sb.AppendLine("伤害！");
                    }
                }
            }
        }
        #endregion
        //做出浮动
        damage *= 1f + Random.Range(-DAMAGE_FLOAT_PERCENT, DAMAGE_FLOAT_PERCENT);
        //结算伤害和输出文字
        sb.Append(from.GetNameBySide());
        sb.Append("对");
        sb.Append(to.GetNameBySide());
        sb.Append("造成了");
        sb.Append(criticalHit ? "<color=#" + ColorUtility.ToHtmlStringRGBA(new Color(1f, 0.9f, 0, 1f)) + ">" : "");
        sb.Append(ValueToString(damage));
        sb.Append(weakBreak ? "<b>弱点击破</b>" : "");
        sb.Append(criticalHit ? "<b>暴击</b>" : "");
        sb.Append("伤害");
        sb.Append(criticalHit ? "</color>" : "");
        sb.AppendLine("!");
        return to.DecreaseHp(battle, damage); ;
    }

    /// <summary>
    /// 从指定战斗单位的临时数据中获得指定属性
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    float GetValue(BattleUnit unit, Attr type)
    {
        return unit.tempRecord.GetValue(type);
    }
    /// <summary>
    /// 获得直接攻击将会造成的伤害
    /// </summary>
    /// <param name="atk"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    float GetDamage(float atk, float def)
    {
        //return (atk * 1) / (def / 100 + 1);
        return (atk * atk) / (def + atk) * 0.5f;
    }
    /// <summary>
    /// 返回是否目标方被弱点击破，如果击破的话目标方的防御力会/2
    /// </summary>
    /// <param name="t_def"></param>
    /// <param name="f_log"></param>
    /// <param name="t_log"></param>
    /// <returns></returns>
    bool GetWeakBreak(ref float t_def, float f_log, float t_log)
    {
        float possi = 1.1f * f_log + 0.9f * t_log;
        possi /= f_log + t_log;
        possi -= 0.95f;
        possi *= 3f;
        possi = possi < 0.1f ? 0.1f : possi;
        //如果弱点击破，那么使得目标方防御/2
        if (Random.value < possi)
        {
            t_def *= 0.5f;
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 返回是否产生暴击。这将在最后阶段进行伤害结算。
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="f_lck"></param>
    /// <param name="t_lck"></param>
    /// <param name="f_cri"></param>
    /// <returns></returns>
    bool GetCriticalHit(ref float damage, float f_lck, float t_lck, float f_cri)
    {
        float criPoss = Mathf.Sqrt(f_lck) / Mathf.Sqrt(f_lck + t_lck) - 0.5f;
        criPoss = criPoss < 0.1f ? 0.1f : criPoss;
        if (Random.value < criPoss)
        {
            damage *= f_cri;
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 一方攻击另一方时计算的的逻辑百分比增伤。
    /// </summary>
    /// <param name="f_log">攻击方的逻辑</param>
    /// <param name="t_log">防守方的逻辑</param>
    /// <returns></returns>
    float GetLogDamagePercent(float f_log, float t_log)
    {
        return ((f_log - (f_log - t_log) * 0.3f) / (f_log + t_log) - 0.49f) * 4f;
    }


    string ValueToString(float value)
    {
        return value.ToString("0.0");
    }
    /// <summary>
    /// 将当前的情况保存下来。
    /// </summary>
    void SaveUnitChange()
    {
        foreach (BattleUnit unit in battle.playerUnits)
        {
            changedProperty.Add(unit, unit.tempRecord.Clone());
        }
        foreach (BattleUnit unit in battle.enemyUnits)
        {
            changedProperty.Add(unit, unit.tempRecord.Clone());
        }
    }

}
