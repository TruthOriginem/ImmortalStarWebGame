using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 机械擂台的成员类，用于显示信息。（一共4名）
/// </summary>
public class MMUIMember : MonoBehaviour
{
    public Text memberName;
    public Text memberDesign;
    public Text memberLevel;
    public Text memberRank;
    public Button challButton;
    private PlayerUnit linkedPlayerUnit;

    public PlayerUnit LinkedPlayerUnit
    {
        get
        {
            return linkedPlayerUnit;
        }

        set
        {
            linkedPlayerUnit = value;
            if (value == null)
            {
                memberName.text = "无";
                memberLevel.text = "";
                memberRank.text = "";
                memberDesign.text = "";
                challButton.interactable = false;
            }
            else
            {
                memberName.text = value.nickname;
                memberLevel.text = string.Format("等级：{0}", value.level);
                memberRank.text = string.Format("排名：{0}", value.challRank);
                memberDesign.text = DesignationManager.GetDesignationName(value.designId);
                challButton.interactable = true;
                challButton.onClick.RemoveAllListeners();
                challButton.onClick.AddListener(() => { MachineMatchManager.Instance.InitTargetPlayerUnitBattle(this); });
            }
        }
    }
    public void Freeze()
    {
        challButton.interactable = false;
    }
}
