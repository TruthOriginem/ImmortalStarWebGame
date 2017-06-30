using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleInstanceGridPool : MonoBehaviour
{
    public RectTransform contents;
    public CanvasGroup canvasGroup;
    public Text stageNameText;
    [Header("对象所需边框")]
    public Sprite normalGridBorder;
    public Sprite bossGridBorder;
    public Sprite goldGridBorder;
    [SerializeField]
    private List<BattleInstanceGrid> objectsInPool;
    public static BattleInstanceGridPool Instance { get; set; }


    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 设置池的大小，会导致池子生成/删除一些对象。
    /// </summary>
    /// <param name="size"></param>
    public void SetPoolSize(int size)
    {

    }
    /// <summary>
    /// 禁用池子中所有对象并调整位置
    /// </summary>
    public void ResetAllObjects()
    {
        contents.localPosition = new Vector3(0, 0, 0);
        for (int i = 0; i < objectsInPool.Count; i++)
        {
            var obj = objectsInPool[i];
            obj.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 根据输入的stage情况,设置信息
    /// </summary>
    /// <param name="stageData"></param>
    public void SetTargetObjects(BatStageData stageData)
    {
        ResetAllObjects();
        for (int i = 0; i < stageData.grids.Length; i++)
        {
            var grid = stageData.grids[i];
            SetObjectsData(grid, grid.index);
        }
        stageNameText.text = stageData.name;
    }
    /// <summary>
    /// 设置指定对象的信息。
    /// </summary>
    /// <param name="data"></param>
    /// <param name="index"></param>
    public void SetObjectsData(BatInsGridData data, int index)
    {
        var grid = objectsInPool[index];
        grid.gameObject.SetActive(true);
        grid.enemyContainer = data.enemys;
        grid.equipDropBundle = data.eqDrop;
        grid.gridId = data.id;
        grid.gridName = data.name;
        grid.description = data.des;
        grid.isBossGrid = data.isBoss;
        grid.limitation = data.limit;
        grid.LinkedGridData = data;
        grid.border.sprite = data.isBoss ? bossGridBorder : normalGridBorder;
        if (data.isBoss)
        {
            grid.border.sprite = data.isGold ? goldGridBorder : bossGridBorder;
        }
        grid.transform.Find("gridName").GetComponent<Text>().text = data.name;
        grid.SetUnlocked(data.IsCompleted());
        grid.SetInteractable(data.Interactive);
        grid.RefreshIcon();
    }
}
