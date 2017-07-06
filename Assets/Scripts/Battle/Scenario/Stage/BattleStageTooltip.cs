using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class BattleStageTooltip : MonoBehaviour
{
    public Text title;
    public Text content;

    void Awake()
    {
        BattleStage.onToolTipEnter = null;
        BattleStage.onToolTipExit = null;
        BattleStage.onToolTipEnter += OnToolTipEnter;
        BattleStage.onToolTipExit += OnToolTipExit;
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RefreshPosition();
    }

    /// <summary>
    /// 刷新这个ToolTip的位置
    /// </summary>
    void RefreshPosition()
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform
                                , Input.mousePosition, Camera.main, out position);
        position.x += 20;
        position.y -= 20;
        transform.localPosition = position;
    }


    void OnToolTipEnter(BattleStage stage)
    {
        RefreshPosition();
        this.gameObject.SetActive(true);
        StringBuilder nameSb = new StringBuilder();
        nameSb.Append(stage.stageName);
        title.text = nameSb.ToString();
        if (stage.GetActable())
        {
            content.text = stage.stageDescription;
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            if (stage.preGridIds != null && stage.preGridIds.Length > 0)
            {
                foreach (string id in stage.preGridIds)
                {
                    var grid = ScenarioManager.GetGridDataById(id);
                    if (!grid.IsCompleted())
                    {
                        sb.Append("-解锁需要关卡：");
                        sb.Append(TextUtils.GetGreenText(ScenarioManager.GetStageDataById(grid.sId).name));
                        sb.Append("的");
                        sb.AppendLine(TextUtils.GetColoredText(grid.name, 255, 100, 100, 255));
                    }
                }
            }
            content.text = sb.ToString();
        }
    }
    void OnToolTipExit()
    {
        this.gameObject.SetActive(false);
    }
}
