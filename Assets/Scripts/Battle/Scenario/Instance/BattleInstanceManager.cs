using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using SerializedClassForJson;
using System.IO;
using System;

/// <summary>
/// 用于管理、收集所有在客户端中的BattleInstanceGrid信息
/// </summary>
public class BattleInstanceManager : MonoBehaviour
{
    private static string GET_INSTANCEGRID_PATH = "scripts/player/instance/getAllGridsInfo.php";

    public static BatStageData NOW_LINKED_STAGE { get; set; }
    public static BattleInstanceManager Instance { get; set; }
    public static bool IsInit = false;
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 刷新所有关卡的情况,并在之后刷新BattleStage的情况。
    /// </summary>
    public Coroutine RefreshAllGrids()
    {
        return StartCoroutine(RefreshAllGridsCor());
    }
    IEnumerator RefreshAllGridsCor()
    {
        ConnectUtils.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        WWW w = new WWW(ConnectUtils.ParsePath(GET_INSTANCEGRID_PATH), form);
        yield return w;
        if (ConnectUtils.IsDownloadCompleted(w) && ConnectUtils.IsPostSucceed(w))
        {
            TempInstanceRecord record = JsonUtility.FromJson<TempInstanceRecord>(w.text);
            TempBattleInGridRecord[] gridInfos = record.gridRecords;
            TempStageExLevelRecord[] stageExs = record.stageRecords;
            //计入临时词典，映射为：关卡id-玩家攻击次数
            Dictionary<string, int> gridIdToAttackTimeColl = new Dictionary<string, int>();
            //用于获得成就-第一次被完成的关卡
            List<BatInsGridData> firstTimeCompletedGridIdList = new List<BatInsGridData>();
            //对一个关卡的最高重复击败次数
            float maxAttackAmount = 0;
            for (int i = 0; i < gridInfos.Length; i++)
            {
                gridIdToAttackTimeColl.Add(gridInfos[i].id, gridInfos[i].tc);
            }
            for (int i = 0; i < stageExs.Length; i++)
            {
                var id = stageExs[i].id;
                var data = ScenarioManager.GetStageDataById(id);
                data.ExtremeLevel = stageExs[i].lv;
            }
            //处理所有关卡
            foreach (var gridData in ScenarioManager.GetAllGridDatas())
            {
                //关卡id
                string id = gridData.id;
                //具体关卡实例
                var grid = gridData;
                grid.Interactive = false;
                grid.SetCompleted(false);
                var limitation = grid.limit;
                //处理关卡的前置关卡
                if (limitation.preGridIds == null || limitation.preGridIds.Length == 0)
                {
                    //如果没有前置关卡，便可以直接通行
                    grid.Interactive = true;
                }
                else
                {
                    //如果有前置关卡，这根据具体情况设置该关卡是否激活
                    bool isInteractable = true;
                    for (int i = 0; i < limitation.preGridIds.Length; i++)
                    {
                        string needId = limitation.preGridIds[i];
                        //如果词典无该关卡键，则说明该关卡未被攻略
                        if (!gridIdToAttackTimeColl.ContainsKey(needId))
                        {
                            isInteractable = false;
                            break;
                        }
                    }
                    grid.Interactive = isInteractable;
                }
                //如果有记录该关卡，则记录该关卡的攻击次数，并且这个关卡被视作解锁。
                if (gridIdToAttackTimeColl.ContainsKey(id))
                {
                    grid.SetAttackCount(gridIdToAttackTimeColl[id]);
                    //如果这个关卡是限制关卡，超过攻击次数也会被禁用
                    if (limitation.attackTimesPerDay != -1 && gridIdToAttackTimeColl[id] >= limitation.attackTimesPerDay)
                    {
                        grid.SetCanNotAttack();
                    }
                    //如果关卡已完成，则判断成就要素1
                    if (IsInit)
                    {
                        firstTimeCompletedGridIdList.Add(grid);
                        if (maxAttackAmount < grid.GetAttackCount())
                        {
                            maxAttackAmount = grid.GetAttackCount();
                        }
                    }
                    grid.SetCompleted(true);
                }

            }
            //所有grid处理完毕后，开始处理Stage
            foreach (var stage in ScenarioManager.GetAllStageDatas())
            {
                string[] gridIds = stage.preGridIds;
                //如果有前置关卡
                if (!EArray.IsNullOrEmpty(gridIds))
                {
                    bool isActable = true;
                    foreach (string id in gridIds)
                    {
                        BatInsGridData grid = ScenarioManager.GetGridDataById(id);
                        //如果前置关卡未解锁，那么该Stage也设置为未解锁
                        if (!grid.IsCurrentCompleted())
                        {
                            if (!grid.IsOnceCompleted())
                            {
                                isActable = false;
                            }
                        }
                    }
                    stage.Unlocked = isActable;
                }
                else
                {
                    stage.Unlocked = true;
                }
                IsInit = true;
                //Debug.Log(stage.stageName);
            }

            DesignationManager.CheckGridsCompleteDesign(firstTimeCompletedGridIdList);
            DesignationManager.CheckMaxAttackAmountDesign(maxAttackAmount);
            DesignationManager.CheckExtremeLevelDesign(stageExs);
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
        }
        ConnectUtils.HideConnectingUI();
    }


    /// <summary>
    /// 直接disable所有instancegrid
    /// </summary>
    public static void DisableAllGrids()
    {/*
        foreach (var kv in Instance.IdToGrids)
        {
            var gridGo = kv.Value.gameObject;
            gridGo.SetActive(false);
        }*/
    }

    public static void GenerateAllFiles()
    {/*
        string path = Application.dataPath + "/bd_uav/";
        Dictionary<string, List<BatInsGridData>> tempgriddicts = new Dictionary<string, List<BatInsGridData>>();
        foreach (var kv in Instance.IdToGrids)
        {
            BatInsGridData data = BatInsGridData.GenerateBIGData(kv.Value);
            if (tempgriddicts.ContainsKey(data.sId))
            {
                var list = tempgriddicts[data.sId];
                list.Add(data);
            }
            else
            {
                var list = new List<BatInsGridData>();
                list.Add(data);
                tempgriddicts.Add(data.sId, list);
            }
        }
        if (!Directory.Exists(path))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            directoryInfo.Create();
        }

        foreach (var stage in Instance.battleStages)
        {
            BatStageData stageData = BatStageData.GenerateBatStageData(stage);
            string stagePath = path + stageData.sId + "/";
            DirectoryInfo directoryInfo = new DirectoryInfo(stagePath);
            directoryInfo.Create();
            foreach (var kv in tempgriddicts)
            {
                var stageId = kv.Key;
                var gridList = kv.Value;
                if (stageId == stageData.sId)
                {
                    foreach (var gridData in gridList)
                    {
                        FileStream fileStream = new FileStream(stagePath + gridData.id + ".json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        StreamWriter sw = new StreamWriter(fileStream);
                        sw.Write(JsonUtility.ToJson(gridData));
                        sw.Close();
                        fileStream.Close();
                    }
                }
                FileStream fs = new FileStream(stagePath + "stage.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw2 = new StreamWriter(fs);
                sw2.Write(JsonUtility.ToJson(stageData));
                sw2.Close();
                fs.Close();
            }
            //tempdicts.Add(stageData, new List<BatInsGridData>());
        }

        */
    }
}
