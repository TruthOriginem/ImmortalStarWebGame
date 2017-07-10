using UnityEngine;
using System.Collections;
using System.Text;


public class BaseSkill
{
    public static int TOTAL_IDKEYS = -1;
    protected string id;//技能id 
    protected int idkey;////唯一的技能序号
    protected string name;//技能名字
    protected string icon_name;//技能图标名字，相关图片从SkillDataManager获取
    protected string description;//技能描述
    protected string documentation;//技能档案
    protected int level = 0;//目前登陆玩家的技能等级
    protected bool equipped = false;//目前登陆玩家是否装备了这个技能

    public int Level
    {
        get
        {
            return level;
        }
        set
        {
            level = value;
        }
    }
    public bool Equipped
    {
        get
        {
            return equipped;
        }
        set
        {
            equipped = value;
        }
    }

    public BaseSkill()
    {
        this.id = "";
        this.name = "";
        this.icon_name = "";
        this.description = "";
        this.documentation = "";
        idkey = ++TOTAL_IDKEYS;
    }
    public string GetId()
    {
        return id;
    }

    public string GetName()
    {
        return name;
    }
    public string GetIconName()
    {
        return icon_name;
    }
    public string GetIconPath()
    {
        return "icons/skills/" + GetIconName() + ".png";
    }
    public string GetDescription()
    {
        return description;
    }
    /// <summary>
    /// 根据玩家等级获得当前所能到达的最高等级。
    /// </summary>
    /// <returns></returns>
    public int GetLimitedLevel()
    {
        return PlayerInfoInGame.Level / 7 + 1;
    }
    /// <summary>
    /// 获得当前升级该技能所需要的技能点。
    /// 0~9需要1点升级,10~19需要2点。
    /// </summary>
    /// <returns></returns>
    public int GetNeedSP()
    {
        return 1 + level / 10;
    }
    /// <summary>
    /// 该技能目前能否被升级
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public bool CanBeUpgraded()
    {
        return PlayerInfoInGame.SkillPoint >= GetNeedSP() && Level < GetLimitedLevel();
    }
    /// <summary>
    /// 获得相应完整的技能档案
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public string GetDocumentation(int now_level)
    {
        string[] docus = documentation.Split('$');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < docus.Length; i++)
        {
            sb.Append(docus[i]);
            if (i == docus.Length - 1) break;
            if (now_level == 0)
            {
                sb.Append("(1级:");
                sb.Append(TextUtils.GetGreenText(GetInfoInDocumentation(1, i)));
                sb.Append(")");
            }
            else
            {
                sb.Append(TextUtils.GetYellowText(GetInfoInDocumentation(now_level, i)));
                sb.Append("(下级:");
                sb.Append(TextUtils.GetGreenText(GetInfoInDocumentation(now_level + 1, i)));
                sb.Append(")");
            }

        }
        return sb.ToString();
    }
    /// <summary>
    /// 获得相应位置，相应等级的技能说明,以$为分割字符串
    /// </summary>
    /// <param name="level"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual string GetInfoInDocumentation(int level, int index)
    {
        return "";
    }
}
