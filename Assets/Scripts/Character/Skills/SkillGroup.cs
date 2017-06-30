using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SkillGroup : MonoBehaviour
{
    public SkillGroupLimitation groupLimitation;
    public Text groupName;
    /// <summary>
    /// 所有拥有LinkSkillButton的子物体的技能id集合
    /// </summary>
    private List<string> groupMemberIds = new List<string>();
    private CanvasGroup canvas;

    private string originGroupName;
    private bool isDirty = true;

    //开始的初始化
    void Start()
    {
        originGroupName = groupName.text;
        //将所有子物体的相关技能按钮获取，并添加入表
        LinkSkillButton[] buttons = GetComponentsInChildren<LinkSkillButton>(true);
        foreach (var button in buttons)
        {
            groupMemberIds.Add(button.linkedSkillId);
        }
        //设置当前画布
        canvas = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        //数据脏则刷新一下数据
        if (isDirty)
        {
            //如果有前置group则需要满足前置group升级一定技能等级才能开启该层的设定。
            if (groupLimitation.preGroup != null)
            {
                var group = groupLimitation.preGroup;
                int needPoints = groupLimitation.needSkillPoint;
                bool able = group.GetTotalSkillLevels() >= needPoints;
                if (able)
                {
                    groupName.text = originGroupName;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(originGroupName);
                    sb.Append(" (");
                    sb.Append(TextUtils.GetColoredText(groupLimitation.preGroup.originGroupName,Color.cyan));
                    sb.Append("技能等级合计");
                    sb.Append(TextUtils.GetColoredText(groupLimitation.needSkillPoint.ToString(), Color.cyan));
                    sb.Append("即可解锁)");
                    groupName.text = sb.ToString();
                }
                SetInteractable(able);
            }
            else
            {
                SetInteractable(true);
            }
            isDirty = false;
        }
    }
    public void MakeDirty()
    {
        isDirty = true;
    }
    /// <summary>
    /// 获得这个技能组所有技能的技能等级合计值（玩家的）
    /// </summary>
    /// <returns></returns>
    public int GetTotalSkillLevels()
    {
        int points = 0;
        foreach (var id in groupMemberIds)
        {
            BaseSkill skill = SkillDataManager.GetSkillById(id);
            points += skill.Level;
        }
        return points;
    }
    public void SetInteractable(bool able)
    {
        canvas.alpha = able ? 1f : 0.8f;
        canvas.interactable = able;
    }
}
[System.Serializable]
public class SkillGroupLimitation
{
    public SkillGroup preGroup = null;//前置技能组
    public int needSkillPoint = 0;//前置技能组的需要点的技能点
}