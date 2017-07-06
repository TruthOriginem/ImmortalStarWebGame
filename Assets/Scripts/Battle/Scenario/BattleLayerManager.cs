using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SerializedClassForJson;

public class BattleLayerManager : MonoBehaviour
{
    public Stack<CanvasGroup> UISceneStack = new Stack<CanvasGroup>();
    public CanvasGroup StartCanvas;
    public static BattleLayerManager Instance { get; set; }


    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 切换UI层，比如从幕到幕副本
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public static void SwitchLayer(CanvasGroup from, CanvasGroup to)
    {
        from.interactable = false;
        from.alpha = 0;
        from.blocksRaycasts = false;
        //from.gameObject.SetActive(false);
        //to.gameObject.SetActive(true);
        to.interactable = true;
        to.alpha = 1;
        to.blocksRaycasts = true;
    }

    /// <summary>
    /// 初始化战斗界面，清除ui堆栈,
    /// </summary>
    public void Init()
    {
        ClearTheUIStack();
        UISceneStack.Push(StartCanvas);
        StartCoroutine(InitCor());
    }
    IEnumerator InitCor()
    {
        yield return BattleInstanceManager.Instance.RefreshAllGrids();
        yield return PlayerRequestBundle.RequestGetRecord<TempLigRecord>();
        TempLigRecord record = PlayerRequestBundle.record as TempLigRecord;
        HangUpManager.isHanging = record.lig_hangTime >= 0;
        ScenarioManager.Instance.InitOrRefreshBattleStageWindow();
    }
    public void Push(CanvasGroup group)
    {
        CanvasGroup top = UISceneStack.Peek();
        UISceneStack.Push(group);
        group.transform.SetAsLastSibling();
        SwitchLayer(top, group);
    }
    public void Push(CanvasGroup group,Action action)
    {
        Push(group);
        action();
    }
    public void Pop()
    {
        CanvasGroup top = UISceneStack.Pop();
        CanvasGroup to = UISceneStack.Peek();
        SwitchLayer(top, to);
        if (to == StartCanvas)
        {
            BattleInstanceGridPool.Instance.ResetAllObjects();
        }
    }
    public void ClearTheUIStack()
    {
        while (UISceneStack.Count > 1)
        {
            Pop();
        }
        UISceneStack.Clear();
    }

}
