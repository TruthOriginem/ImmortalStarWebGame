using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSceneManager : MonoBehaviour
{
    public Text LeftSkillPointText;
    public Transform SkillMainScene;
    public Transform SkillEquipViewContent;//玩家装备技能的改变
    public static SkillSceneManager Instance { get; set; }

    private bool isDirty = true;
    public void Init()
    {
        MakeDirty();
    }
    /// <summary>
    /// 使得所有LinkSkillButton、SkillGroup的数据变脏
    /// </summary>
    public void MakeDirty()
    {
        isDirty = true;
        LinkSkillButton[] buttons = SkillMainScene.GetComponentsInChildren<LinkSkillButton>();
        SkillGroup[] groups = SkillMainScene.GetComponentsInChildren<SkillGroup>();
        foreach (LinkSkillButton button in buttons)
        {
            if (button.transform.parent == SkillEquipViewContent)
            {
                //如果在玩家装备栏，则删除，等待下次生成
                GameObject.Destroy(button.gameObject);
            }
            else
            {
                if (button.GetLinkedSkill().Equipped)
                {
                    Instantiate(button.gameObject, SkillEquipViewContent);
                }
                button.MakeDirty();
            }
        }
        foreach (var group  in groups)
        {
            group.MakeDirty();
        }
    }

    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDirty)
        {
            LeftSkillPointText.text = PlayerInfoInGame.SkillPoint.ToString();
            isDirty = false;
        }
    }
}
