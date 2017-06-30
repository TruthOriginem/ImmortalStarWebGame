using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BatStageData{
    public int index;
    public string sId;
    /// <summary>
    /// 图片文件的名字
    /// </summary>
    public string imageFileName;
    public string name;
    public string des;
    public string[] preGridIds;
    public BatInsGridData[] grids;
    private bool isActable = true;

    /// <summary>
    /// 是否可以交互(解锁)。
    /// </summary>
    public bool Unlocked
    {
        get
        {
            return isActable;
        }

        set
        {
            isActable = value;
        }
    }
    /// <summary>
    /// 是否已经通关，需要确保玩家刷新了关卡内容。
    /// </summary>
    /// <returns></returns>
    public bool IsClear()
    {
        return grids[grids.Length - 1].IsCompleted();
    }

    public static BatStageData GenerateBatStageData(BattleStage stage)
    {
        BatStageData data = new BatStageData();
        data.sId = stage.stageId;
        data.imageFileName = stage.imageFileName;
        data.name = stage.stageName;
        data.des = stage.stageDescription;
        data.preGridIds = stage.preGridIds;
        return data;
    }
}
