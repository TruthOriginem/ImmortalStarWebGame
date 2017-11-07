using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

/// <summary>
/// 管理技能阅读说明部分
/// </summary>
public class SkillReadingPart : MonoBehaviour
{
    private BaseSkill skill;//当前的技能
    public Text skillNameAndLevel;//技能名字+等级
    public Text skillDescription;//技能说明
    public Text skillDocumentation;//技能文档
    public Image skillIcon;//技能图标

    public Text skillUpgradeNeededText;//提示所需技能点
    public Button skillUpgradeButton;//技能升级按钮
    public Button skillEquipButton;//技能装备按钮
    public Text skillEquipText;

    public static SkillReadingPart Instance { get; set; }
    void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 把说明改为指定技能
    /// </summary>
    public void RefreshTargetSkill(LinkSkillButton linkSkill)
    {
        skill = linkSkill.GetLinkedSkill();
        RefreshTargetSkill();
        skillIcon.sprite = linkSkill.linkedSkillIcon.sprite;
        SkillSceneManager.Instance.MakeDirty();
    }
    public void RefreshTargetSkill()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(skill.GetName());
        sb.AppendLine();
        sb.Append("Lv.");
        sb.Append(skill.Level);
        sb.Append("(最高");
        sb.Append(TextUtils.GetColoredText(skill.GetLimitedLevel().ToString(), Color.red));
        sb.Append("级)");
        skillUpgradeNeededText.text = "升级所需技能点:" + TextUtils.GetYellowText(skill.GetNeedSP().ToString());
        skillNameAndLevel.text = sb.ToString();
        skillDescription.text = skill.GetDescription();
        skillDocumentation.text = skill.GetDocumentation(skill.Level);
    }

    void Update()
    {
        if (skill != null)
        {
            bool interactable = false;
            if (skill.Level > 0)
            {
                interactable = true;
                if (skill.Equipped)
                {
                    skillEquipText.text = "卸载技能";
                }
                else
                {
                    interactable = SkillDataManager.GetNowEquippedSkillAmount() < SkillDataManager.GetMaxEquippedSkillAmount();
                    skillEquipText.text = "装备技能";
                }
            }
            else
            {
                interactable = false;
            }
            skillEquipButton.interactable = interactable;
            //设置是否可以升级
            skillUpgradeButton.interactable = skill.CanBeUpgraded();
        }
    }

    public void UpgradeTargetSkill()
    {
        if (skill == null)
        {
            return;
        }
        MessageBox.Show("您确定要升级这个技能？你需要花费" + skill.GetNeedSP() + "点技能点。", "提示", (result) =>
            {
                if (result == DialogResult.Yes)
                {
                    skill.Level++;
                    RequestBundle.RequestUpdateSkills(-skill.GetNeedSP());
                }
            }, MessageBoxButtons.YesNo);

    }
    public void EquipTargetSkill()
    {
        if (skill == null)
        {
            return;
        }
        skill.Equipped = skill.Equipped ? false : true;
        RequestBundle.RequestUpdateSkills(0);
    }

}
