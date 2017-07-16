using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class BatStageData
{
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
    private int extremeLevel;
    public const float MULT_PER_EXLEVEL = 1.25f;
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
    /// 极限等级
    /// </summary>
    public int ExtremeLevel
    {
        get
        {
            return extremeLevel;
        }

        set
        {
            extremeLevel = value;
        }
    }
    public float GetMultiExLevelFactor()
    {
        float resultf = 1f;
        int ex = extremeLevel;
        while (ex-- > 0)
        {
            resultf *= MULT_PER_EXLEVEL;
        }
        return resultf;
    }
    /// <summary>
    /// 包含极限等级，通关等修饰的地区名。
    /// </summary>
    /// <returns></returns>
    public string GetModifiedName()
    {
        StringBuilder sb = new StringBuilder(name);
        if (extremeLevel > 0)
        {
            sb.Append("<size=18>(");
            sb.Append("极限<color=red>");
            sb.Append(extremeLevel);
            sb.Append("</color>)</size>");
        }
        if (IsClear())
        {
            sb.Append(TextUtils.GetYellowText("<size=12>★</size>"));
        }
        return sb.ToString();
    }
    /// <summary>
    /// 返回玩家当前Stage的所有关卡是否都已完成。(极限升级后重启时，关卡的完成状态会重启)
    /// </summary>
    /// <returns></returns>
    public bool IsClear()
    {
        bool isComplete = true;
        for (int i = 0; i < grids.Length; i++)
        {
            if (!grids[i].IsCurrentCompleted()) isComplete = false;
        }
        return isComplete;
    }
    /// <summary>
    /// Stage是否已经被完成过。如果有极限等级则返回True。
    /// </summary>
    /// <returns></returns>
    public bool IsCompleted()
    {
        if (extremeLevel > 0)
        {
            return true;
        }
        return IsClear();
    }
    /*
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
    */
}
