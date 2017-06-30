using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkSkillButton : MonoBehaviour
{
    [Tooltip("链接技能的id")]
    public string linkedSkillId;

    public Text linkedSkillName;
    public Image linkedSkillIcon;

    /// <summary>
    /// 等级为0时变为黑色
    /// </summary>
    public Material greyMaterial;

    bool iconDirty = true;

    // Update is called once per frame
    void Update()
    {
        if (linkedSkillId == null) return;
        if (linkedSkillIcon.sprite == null)
        {
            linkedSkillIcon.sprite = SkillDataManager.GetSpriteById(linkedSkillId);
        }
        if (iconDirty)
        {
            BaseSkill skill = GetLinkedSkill();
            linkedSkillName.text = skill.GetName() + "\nLv." + skill.Level;
            if (skill == null)
            {
                return;
            }
            if (skill.Level == 0)
            {
                linkedSkillIcon.material = greyMaterial;
            }
            else
            {
                linkedSkillIcon.material = null;
            }
            iconDirty = false;
        }
    }
    public void InitLinkedSkillId(string id)
    {
        linkedSkillId = id;
        MakeDirty();
    }
    public void MakeDirty()
    {
        iconDirty = true;
    }
    /// <summary>
    /// 获得连接的技能
    /// </summary>
    /// <returns></returns>
    public BaseSkill GetLinkedSkill()
    {
        return SkillDataManager.GetSkillById(linkedSkillId);
    }

    public void RefreshSkillReadingPart()
    {
        SkillReadingPart.Instance.RefreshTargetSkill(this);
    }
}
