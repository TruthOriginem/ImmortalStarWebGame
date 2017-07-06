using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 用于管理所有关卡数据下载、加载的类
/// </summary>
public class ScenarioManager : MonoBehaviour
{
    private static Dictionary<string, BatInsGridData> gridDict;
    private static Dictionary<string, BatStageData> stageDict;
    private static List<BatStageBundleData> bundleDatas;
    private static string PATH = "scripts/battle/datas/loadScenarioDatas.php";
    //private static string STAGE_IMAGE_PATH = "icons/stages/";
    public static ScenarioManager Instance { get; set; }
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {

    }
    public static Coroutine InitAllScenarioData()
    {
        return Instance.StartCoroutine(Instance.DownLoadAllScenario());
    }
    /// <summary>
    /// 进入剧本界面时调用的初始化。
    /// </summary>
    public void InitOrRefreshBattleStageWindow()
    {
        BattleStageBundle.Instance.ChooseBundle(bundleDatas[0]);
    }

    IEnumerator DownLoadAllScenario()
    {
        ConnectUtils.ShowConnectingUI();
        WWW w = new WWW(ConnectUtils.ParsePath(PATH));
        yield return w;
        if (w.isDone && w.text != ConnectUtils.FAILED)
        {
            BatStageBundleData[] bundles = JsonHelper.GetJsonArray<BatStageBundleData>(w.text);
            bundleDatas = new List<BatStageBundleData>(bundles);
            gridDict = new Dictionary<string, BatInsGridData>();
            stageDict = new Dictionary<string, BatStageData>();
            for (int i = 0; i < bundleDatas.Count; i++)
            {
                var bundle = bundleDatas[i];
                bundle.Sort();
                for (int j = 0; j < bundle.stages.Length; j++)
                {
                    //处理stage
                    var stage = bundle.stages[j];
                    for (int q = 0; q < stage.grids.Length; q++)
                    {
                        //处理grid
                        gridDict.Add(stage.grids[q].id, stage.grids[q]);
                        stage.grids[q].SetParentStageData(stage);
                        stage.grids[q].sId = stage.sId;
#if UNITY_EDITOR
                        var grid = stage.grids[q];
                        var dropbundle = grid.eqDrop;
                        foreach (var drop in dropbundle.randInfos)
                        {
                            DebugLoadManager.eqdroplists[(int)drop.type]++;
                        }
#endif
                    }
                    stageDict.Add(stage.sId, stage);
                }
            }
#if UNITY_EDITOR
            StringBuilder sb = new StringBuilder();
            for(int i =0;i<DebugLoadManager.eqdroplists.Count;i++)
            {
                var amount = DebugLoadManager.eqdroplists[i];
                sb.Append(EquipmentBase.GetEqTypeNameByType((EQ_TYPE)i));
                sb.Append("数量: ");
                sb.Append(amount);
                sb.Append(" ; ");
            }
            Debug.Log(sb.ToString());
#endif
        }
        else
        {
            ConnectUtils.HideConnectingUI();
            ConnectUtils.ShowConnectFailed();
            yield break;
        }
        ConnectUtils.HideConnectingUI();
    }
    public List<BatInsGridData> GetAllGridDatas()
    {
        List<BatInsGridData> datas = new List<BatInsGridData>();
        for (int i = 0; i < bundleDatas.Count; i++)
        {
            var bundle = bundleDatas[i];
            for (int j = 0; j < bundle.stages.Length; j++)
            {
                var stage = bundle.stages[j];
                for (int q = 0; q < stage.grids.Length; q++)
                {
                    datas.Add(stage.grids[q]);
                }
            }
        }
        return datas;
    }
    public List<BatStageData> GetAllStageDatas()
    {
        List<BatStageData> datas = new List<BatStageData>();
        for (int i = 0; i < bundleDatas.Count; i++)
        {
            var bundle = bundleDatas[i];
            for (int j = 0; j < bundle.stages.Length; j++)
            {
                var stage = bundle.stages[j];
                datas.Add(stage);

            }
        }
        return datas;
    }
    public static BatInsGridData GetGridDataById(string id)
    {
        return gridDict != null && gridDict.ContainsKey(id) ? gridDict[id] : null;
    }
    public static BatStageData GetStageDataById(string id)
    {
        return stageDict != null && stageDict.ContainsKey(id) ? stageDict[id] : null;
    }
}
