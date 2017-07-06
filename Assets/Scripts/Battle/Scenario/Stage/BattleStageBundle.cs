using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BattleStageBundle : MonoBehaviour
{
    public static BattleStageBundle Instance { get; set; }
    public GameObject stagePrefab;
    public Transform bundle;
    public Text bundleText;
    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 选择当前的剧集，并刷新stage。
    /// </summary>
    /// <param name="data"></param>
    public void ChooseBundle(BatStageBundleData data)
    {
        ClearBundle();
        bundleText.text = data.bundleDes;
        for (int i = 0; i < data.stages.Length; i++)
        {
            var stageData = data.stages[i];
            BattleStage stage = Instantiate(stagePrefab, bundle, false).GetComponentInChildren<BattleStage>();
            stage.stageId = stageData.sId;
            stage.stageName = stageData.GetModifiedName() ;
            stage.stageDescription = stageData.des;
            stage.imageFileName = stageData.imageFileName;
            stage.preGridIds = stageData.preGridIds;
            stage.LinkedStageData = stageData;
            stage.stageButtonText.text = stage.stageName;
            stage.stageLevelUpButton.gameObject.SetActive(stageData.IsClear() ? true : false);
            stage.InitStageImage();
            stage.SetActable(stageData.Unlocked);
        }
    }
    void ClearBundle()
    {
        for (int i = 0; i < bundle.childCount; i++)
        {
            var child = bundle.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
