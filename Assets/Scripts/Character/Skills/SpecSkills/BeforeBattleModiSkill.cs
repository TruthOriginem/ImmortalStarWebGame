using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameId;
/// <summary>
/// 战斗之前，对属性有所改变的技能。
/// </summary>
public class BeforeBattleModiSkill : BaseSkill {
    protected Dictionary<Attr,float> percentPerLevelDic = new Dictionary<Attr, float>();
    protected Dictionary<Attr, float> multPerLevelDic = new Dictionary<Attr, float>();

    public BeforeBattleModiSkill() : base() {

    }
    /// <summary>
    /// 得到对于属性的百分比调整(0~100)
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public virtual float GetPercentModify(Attr property,int level)
    {
        return percentPerLevelDic.ContainsKey(property)?percentPerLevelDic[property] * level:0f;
    }
    /// <summary>
    /// 得到对于属性的乘法调整
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public virtual float GetMultModify(Attr property,int level)
    {
        return multPerLevelDic.ContainsKey(property)?multPerLevelDic[property] * level:1f;
    }
}
/// <summary>
/// 机械作成技能 -0
/// </summary>
public class Skill_MachineGenerate : BeforeBattleModiSkill
{
    public Skill_MachineGenerate(string id) : base()
    {
        this.id = id;
        this.name = "机械作成";
        this.icon_name = id;
        this.description = "机械体用于加强自身的基本技能。";
        this.documentation = "在整个战斗中，获得$的防御加成。";
        percentPerLevelDic.Add(Attrs.DEF, 1f);
    }
    public override string GetInfoInDocumentation(int level, int index)
    {
        if (index == 0)
        {
            return (int)(percentPerLevelDic[Attrs.DEF] * level) + "%";
        }
        return "";
    }
}
/// <summary>
/// 熟练攻击技能 -0
/// </summary>
public class Skill_SkillfulAttack : BeforeBattleModiSkill
{
    public Skill_SkillfulAttack(string id) : base()
    {
        this.id = id;
        this.name = "熟练攻击";
        this.icon_name = id;
        this.description = "在任何情况下都能快速做出攻击的反应。";
        this.documentation = "在整个战斗中，获得$的攻击加成，$的逻辑加成。";
        percentPerLevelDic.Add(Attrs.ATK, 0.5f);
        percentPerLevelDic.Add(Attrs.LOG, 0.1f);
    }
    public override string GetInfoInDocumentation(int level, int index)
    {
        if (index == 0)
        {
            return (percentPerLevelDic[Attrs.ATK] * level).ToString("0.0") + "%";
        }
        if (index == 1)
        {
            return (percentPerLevelDic[Attrs.LOG] * level).ToString("0.0") + "%";
        }
        return "";
    }
}
/// <summary>
/// 八荒六合技能 -0
/// </summary>
public class Skill_UniverseThoughts : BeforeBattleModiSkill
{
    public Skill_UniverseThoughts(string id) : base()
    {
        this.id = id;
        this.name = "八荒六合";
        this.icon_name = id;
        this.description = "心如平静，遁入智瞳。机械生命体掌握自身生命与灵魂的冥想方式。 在这个危险的宇宙中，物理上的打击并不可怕，可怕的是心。 ";
        this.documentation = "在整个战斗中，获得$的全属性加成。";
        percentPerLevelDic.Add(Attrs.MHP, 0.15f);
        percentPerLevelDic.Add(Attrs.MMP, 0.15f);
        percentPerLevelDic.Add(Attrs.ATK, 0.15f);
        percentPerLevelDic.Add(Attrs.DEF, 0.15f);
        percentPerLevelDic.Add(Attrs.LOG, 0.15f);
        percentPerLevelDic.Add(Attrs.LCK, 0.15f);
        percentPerLevelDic.Add(Attrs.CRI, 0.15f);
    }
    public override string GetInfoInDocumentation(int level, int index)
    {

        return (percentPerLevelDic[Attrs.ATK] * level).ToString("0.0") + "%";
 
    }
}
/// <summary>
/// 铜墙铁壁技能 -1
/// </summary>
public class Skill_IronWall : BeforeBattleModiSkill
{
    public Skill_IronWall(string id) : base()
    {
        this.id = id;
        this.name = "铜墙铁壁";
        this.icon_name = id;
        this.description = "在一次战斗之前加强整个身躯的防御，修缮所有防具，另你的生命能量处于最大状态。";
        this.documentation = "在整个战斗中，获得$的防御加成与$的生命、能量加成。";
        percentPerLevelDic.Add(Attrs.DEF, 0.5f);
        percentPerLevelDic.Add(Attrs.MHP, 0.2f);
        percentPerLevelDic.Add(Attrs.MMP, 0.2f);
    }
    public override string GetInfoInDocumentation(int level, int index)
    {
        if (index == 0)
        {
            return (percentPerLevelDic[Attrs.DEF] * level).ToString("0.0") + "%";
        }
        if (index == 1)
        {
            return (percentPerLevelDic[Attrs.MHP] * level).ToString("0.0") + "%";
        }
        return "";
    }
}
