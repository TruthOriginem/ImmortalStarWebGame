using UnityEngine;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;
using SerializedClassForJson;

public class BattleInstanceTooltip : MonoBehaviour
{
    public Text title;
    public Text description;
    public Text data;

    void Awake()
    {
        BattleInstanceGridImage.onIconEnter = null;
        BattleInstanceGridImage.onIconExit = null;
        BattleInstanceGridImage.onIconEnter += onIconEnter;
        BattleInstanceGridImage.onIconExit += onIconExit;
        gameObject.SetActive(false);
    }

    void Update()
    {
        RefreshPosition();
    }

    /// <summary>
    /// 刷新这个ToolTip的位置
    /// </summary>
    void RefreshPosition()
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform
                                , Input.mousePosition, Camera.main, out position);
        position.x += 20;
        position.y -= 20;
        Rect rect = GetComponent<RectTransform>().rect;
        float overflowX = Input.mousePosition.x + 20 + rect.width - Screen.width;
        float overflowY = Input.mousePosition.y - 20 - rect.height;
        if (overflowX > 0f)
        {
            position.x -= overflowX;
        }
        if (overflowY < 0f)
        {
            position.y -= overflowY;
        }
        transform.localPosition = position;
    }


    void onIconEnter(BattleInstanceGrid grid)
    {
        gameObject.SetActive(true);
        RefreshPosition();
        title.text = grid.isBossGrid ? TextUtils.GetColoredText(grid.gridName, 150f, 100f, 255f, 255f) : grid.gridName;
        description.text = grid.description;
        data.text = CompleteDataContent(grid.LinkedGridData);
        transform.SetAsLastSibling();
    }
    void onIconExit()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示有哪些敌人
    /// </summary>
    public static string CompleteDataContent(BatInsGridData grid)
    {
        if (grid == null)
        {
            return null;
        }
        StringBuilder sb = new StringBuilder();


        var limitation = grid.limit;
        if (limitation.attackTimesPerDay != -1 && grid.GetAttackCount() >= limitation.attackTimesPerDay)
        {
            int amount;
            if (!grid.CanUseResetPowder(out amount))
            {
                return "今日的挑战次数已用完。";
            }
            else
            {
                sb.AppendLine("今日挑战次数已用完，但是可以使用" + amount + "个Boss重置粉末继续挑战。");
                sb.AppendLine();
            }
        }
        else if (!grid.Interactive && limitation.preGridIds != null && limitation.preGridIds.Length != 0)
        {
            sb.AppendLine(TextUtils.GetColoredText("<size=18>解锁该关卡条件：</size>", 255, 30, 30, 255));
            for (int i = 0; i < limitation.preGridIds.Length; i++)
            {
                var limitGrid = ScenarioManager.GetGridDataById(limitation.preGridIds[i]);
                sb.Append(" -通过");
                //设置一下关卡名的颜色，正常为#00DFFFFF
                string gridName = limitGrid.isBoss ? TextUtils.GetColoredText(limitGrid.name, 150f, 100f, 255f, 255f) : "<color=#00DFFFFF>" + limitGrid.name + "</color>";
                sb.AppendLine(gridName);
            }
            sb.Append("注：全部通过才可解锁");
            return sb.ToString();
        }
        if (grid.limit.attackTimesPerDay != -1 && !grid.CanUseResetPowder())
        {
            sb.Append(TextUtils.GetGreenText("今日挑战剩余次数："));
            int maxAttackCount = grid.limit.attackTimesPerDay;
            sb.Append(maxAttackCount - grid.GetAttackCount());
            sb.Append("/");
            sb.AppendLine(maxAttackCount.ToString());
            sb.AppendLine();
        }

        Dictionary<string, float> idsToLeast = new Dictionary<string, float>();
        Dictionary<string, float> idsToLarge = new Dictionary<string, float>();
        int totalExp = 0;
        var actualEnemyGroup = grid.enemys.GetActualData(grid.sId);
        foreach (EnemyGroup group in actualEnemyGroup.enemyGroups)
        {
            EnemyAttribute attr = EnemyDataManager.AskForEnemyAttribute(group.enemy.id);
            if (attr == null)
            {
                return null;
            }
            totalExp += group.amount * (attr.baseP.exp + attr.growth.exp * (group.enemy.Level - 1));
            sb.Append("· ");
            sb.AppendLine(attr.name + " Lv." + group.enemy.Level + " x " + group.amount);
            //遍历指定怪物种类的掉落信息
            for (int i = 0; i < attr.dropItems.Length; i++)
            {
                //每个group里都是一个怪物（相同等级）的几只
                TempItemDrops itemDrop = attr.dropItems[i];
                if (group.enemy.Level >= itemDrop.needLevel)
                {
                    float amount = group.amount * itemDrop.amount * (1f + itemDrop.multLevel * (group.enemy.Level - itemDrop.needLevel));
                    //Debug.Log(amount);
                    if (idsToLarge.ContainsKey(itemDrop.id))
                    {

                        idsToLarge[itemDrop.id] += amount;
                        idsToLeast[itemDrop.id] += amount * itemDrop.chance;
                    }
                    else
                    {

                        idsToLarge.Add(itemDrop.id, amount);
                        idsToLeast.Add(itemDrop.id, amount * itemDrop.chance);
                    }
                }
            }
        }
        totalExp = Mathf.RoundToInt(totalExp * BattleAwardMult.GetExpMult());
        sb.AppendLine();
        sb.AppendLine(TextUtils.GetSizedString("基本掉落", 18));
        sb.Append("▽ ");
        sb.Append(TextUtils.GetExpText("经验: "));
        if (PlayerInfoInGame.VIP_Level >= 1)
        {
            sb.Append(totalExp);
            sb.AppendFormat("<color=#FFAE00FF>(+{0}%)</color>", Mathf.RoundToInt((BattleAwardMult.GetExpMult() - 1f) * 100f));
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine(totalExp + "");
        }
        foreach (var kv in idsToLarge)
        {
            bool isMoney = kv.Key == "money";
            string signal = isMoney ? "▽ " : "▼ ";
            string name = isMoney ? TextUtils.GetMoneyText(ItemDataManager.GetItemName(kv.Key)) : ItemDataManager.GetItemName(kv.Key);
            sb.AppendFormat("{0}{1} : {2} ~ {3}", signal, name, (int)idsToLeast[kv.Key], (int)idsToLarge[kv.Key]);
            if (PlayerInfoInGame.VIP_Level >= 1)
            {
                sb.Append("<color=#FFAE00FF>(+");
                if (isMoney)
                {
                    sb.Append(Mathf.RoundToInt((BattleAwardMult.GetMoneyMult() - 1f) * 100f));
                }
                else
                {
                    sb.Append(Mathf.RoundToInt((BattleAwardMult.GetDropMult() - 1f) * 100f));
                }
                sb.Append("%)</color>");
            }
            sb.AppendLine();
        }
        sb.AppendLine();
        sb.Append(TextUtils.GetSizedString(grid.eqDrop.GetDropInfo(), 13));
        //sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
}
