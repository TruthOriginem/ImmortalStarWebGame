using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BatStageBundleData {
    public string id;
    public string bundleDes;
    public string[] preGridIds;
    public BatStageData[] stages;
    public void Sort()
    {
        List<BatStageData> stageList = new List<BatStageData>(stages);
        stageList.Sort(new StageComparer());
        stages = stageList.ToArray();
        for (int i = 0; i < stages.Length; i++)
        {
            var stage = stages[i];
            List<BatInsGridData> grids = new List<BatInsGridData>(stage.grids);
            grids.Sort(new GridComparer());
            stage.grids = grids.ToArray();
        }
    }
}
public class GridComparer : IComparer<BatInsGridData>
{
    public int Compare(BatInsGridData x, BatInsGridData y)
    {
        return x.index.CompareTo(y.index);
    }
}
public class StageComparer : IComparer<BatStageData>
{
    public int Compare(BatStageData x, BatStageData y)
    {
        return x.index.CompareTo(y.index);
    }
}