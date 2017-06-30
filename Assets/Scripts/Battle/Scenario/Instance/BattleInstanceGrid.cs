using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using UnityEngine.UI;

public class BattleInstanceGrid : MonoBehaviour
{
    [Header("关卡怪物及掉落信息")]
    /// <summary>
    /// 用于存储该格内的怪物信息
    /// </summary>
    public EnemySpawnData enemyContainer;
    /// <summary>
    /// 特殊装备掉落
    /// </summary>
    public EquipmentDropBundle equipDropBundle;

    [Header("关卡客户端信息")]
    public int index;
    /// <summary>
    /// 该关卡的id
    /// </summary>
    public string gridId;
    /// <summary>
    /// 该关卡的名字。
    /// </summary>
    public string gridName;
    /// <summary>
    /// 该关卡的描述。
    /// </summary>
    public string description;
    [Header("关卡其他信息")]
    public InstanceGridLimitation limitation;
    /// <summary>
    /// 该关卡是不是Boss关卡
    /// </summary>
    public bool isBossGrid;

    [Header("关卡预制体信息")]
    /// <summary>
    /// 怪物格子显示的icon
    /// </summary>
    public Image icon;
    public Image border;
    public Button attackButton;
    public Button sweepButton;




    /// <summary>
    /// 用于控制该格是否可以交互
    /// </summary>
    public CanvasGroup gridCanvas;

    private bool isActable = true;
    private int attackCount = 0;
    private bool unlocked = false;//是否被解锁,即通关过一次
    private BatInsGridData linkedGridData;

    public BatInsGridData LinkedGridData
    {
        get
        {
            return linkedGridData;
        }

        set
        {
            linkedGridData = value;

        }
    }

    /*
private void Start()
{
   InitAllEnemies();
   if (BattleInstanceManager.Instance.idsToGrid.ContainsKey(gridId))
   {
       BattleInstanceManager.Instance.idsToGrid.Remove(gridId);
       BattleInstanceManager.Instance.idsToGrid.Add(gridId, this);
   }
   else
   {
       BattleInstanceManager.Instance.idsToGrid.Add(gridId, this);
   }
   transform.FindChild("gridName").GetComponent<Text>().text = gridName;

}
*/
    void Start()
    {
        gameObject.SetActive(false);
    }



    void Update()
    {

        /*
        if (BattleManager.Instance.IsInBattle())
        {
            SetInteractable(false);
        }
        else
        {
            SetInteractable(true);
        }
        */
    }
    /// <summary>
    /// 按下攻击按钮
    /// </summary>
    public void BeginTheBattle(CanvasGroup battlePart)
    {
        if (HangUpManager.Instance.isHanging)
        {
            MessageBox.Show("挂机状态的时候不能战斗！");
            return;
        }
        else
        {
            StartCoroutine(InitBattle(battlePart));
        }
    }
    IEnumerator InitBattle(CanvasGroup battlePart)
    {
        yield return BattleManager.Instance.InitBattle(linkedGridData);
        BattleLayerManager.Instance.Push(battlePart);
        BattleManager.Instance.SetBackInvoke(() => { BattleLayerManager.Instance.Pop(); BattleInstanceGridPool.Instance.SetTargetObjects(BattleInstanceManager.NOW_LINKED_STAGE); });
    }
    public void RefreshIcon()
    {
        if (icon.sprite != null)
        {
            Destroy(icon.sprite);
        }
        for (int i = 0; i < enemyContainer.enemyGroups.Count; i++)
        {
            var group = enemyContainer.enemyGroups[i];
            if (group.enemy.showIcon)
            {
                EnemyAttribute eAttr = EnemyDataManager.AskForEnemyAttribute(group.enemy.id);
                if (eAttr != null)
                {
                    Texture2D texture = eAttr.iconTexture;
                    icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    return;
                }
            }
        }
    }

    public void SetInteractable(bool ifCan)
    {
        if (ifCan)
        {
            if (!isActable)
            {
                gridCanvas.alpha = 1;
                gridCanvas.interactable = true;
                //gridCanvas.blocksRaycasts = true;
                isActable = true;
            }
        }
        else
        {
            if (isActable)
            {
                gridCanvas.alpha = 0.6f;
                gridCanvas.interactable = false;
                //gridCanvas.blocksRaycasts = false;
                isActable = false;
            }
        }
    }
    public void SetAttackCount(int count)
    {
        attackCount = count;
    }
    /// <summary>
    /// 返回客户端记录中对这个关卡的攻击次数。
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public int GetAttackCount()
    {
        return attackCount;
    }
    /// <summary>
    /// 即是否可以继续攻击，
    /// </summary>
    /// <returns></returns>
    public bool GetActable()
    {
        return isActable;
    }
    public bool SetUnlocked(bool unlocked)
    {
        this.unlocked = unlocked;
        return unlocked;
    }
    public bool IsUnlocked()
    {
        return unlocked;
    }
}
/// <summary>
/// 关卡限制
/// </summary>
[Serializable]
public class InstanceGridLimitation
{
    /// <summary>
    /// 开放该格需要的前置关卡id
    /// </summary>
    public string[] preGridIds;
    /// <summary>
    /// 每天能攻击的次数,-1为无限
    /// </summary>
    public int attackTimesPerDay = -1;
}