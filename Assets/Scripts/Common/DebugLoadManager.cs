#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 正常游戏中并不会使用该类。 
/// </summary>
public class DebugLoadManager : MonoBehaviour
{
    public static List<int> eqdroplists = new List<int>(6) { 0, 0, 0, 0, 0, 0 };
    void Start()
    {
        StartCoroutine(StartInit());
    }
    /// <summary>
    /// Debug阶段初始化
    /// </summary>
    /// <returns></returns>
	IEnumerator StartInit()
    {
        yield return StartCoroutine(ModDataManager.InitEquipmentFactory());
        yield return DesignationManager.InitDesignationDatas();
        yield return ScenarioManager.InitAllScenarioData();
        yield return EnemyDataManager.InitAllEnemiesInList();
        yield return ItemDataManager.InitAllItems();
        yield return EnemyExpeditionManager.InitEnemyExpeditionInfos();
    }
    void Update()
    {

    }
}
#endif