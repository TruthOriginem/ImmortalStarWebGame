using System.Collections.Generic;
using GameId;
using UnityEngine;
using System.Text;
/// <summary>
/// 处理根据id产生的相关内容
/// </summary>
public partial class ItemPack
{
    const string GREEN_PATH = "icons/itempacks/gp_green.png";
    const string YELLOW_PATH = "icons/itempacks/gp_yellow.png";
    public void InitById()
    {
        switch (packId)
        {
            case ItemPacks.SIGN_IN:
                description = "签到礼包将根据你的等级给予奖励！七天一个周期，1到7级礼包，7级礼包将给你最大的惊喜！";
                iconPath = GREEN_PATH;
                break;
            case ItemPacks.VIP_NORMAL:
                description = "VIP礼包，每天都可以领取。VIP等级越高，领取越多物品。";
                iconPath = YELLOW_PATH;
                break;
            default:
                break;
        }
    }

    public Dictionary<string, lint> GetItemToAmounts()
    {
        int level = GetPackLevel();
        int sqrtPlayerLevel = (int)Mathf.Sqrt(PlayerInfoInGame.Level);
        itemToAmounts.Clear();
        switch (packId)
        {
            case ItemPacks.SIGN_IN:
                itemToAmounts.Add(Items.MONEY, (5000 + 15000 * level) * sqrtPlayerLevel);
                itemToAmounts.Add(Items.SPB_PIECE, (50 + 50 * level) * sqrtPlayerLevel);
                itemToAmounts.Add(Items.MM_TICKET, 15);
                itemToAmounts.Add(Items.CARD_DROP_DOUBLE, 10 + 2 * level);
                itemToAmounts.Add(Items.CARD_EXP_DOUBLE, 20 + 4 * level);
                if (level == 7)
                {
                    itemToAmounts.Add(Items.DIMEN, 5000);
                    itemToAmounts.Add(Items.REHANCE_STONE, 50);
                    itemToAmounts.Add(Items.REHANCE_SPAR, 10);
                }
                break;
            case ItemPacks.VIP_NORMAL:
                itemToAmounts.Add(Items.SPB_PIECE, (150 + 50 * level) * sqrtPlayerLevel);
                itemToAmounts.Add(Items.REHANCE_STONE, 5 + level);
                itemToAmounts.Add(Items.REHANCE_SPAR, level);
                itemToAmounts.Add(Items.CARD_DROP_DOUBLE, 10 + 5 * level);
                itemToAmounts.Add(Items.CARD_EXP_DOUBLE, 10 + 5 * level);
                break;
            default:
                break;
        }
        return itemToAmounts;
    }
    public string GetItemsString()
    {
        StringBuilder sb = new StringBuilder();
        var itas = GetItemToAmounts();
        foreach (var item in itas)
        {
            sb.Append(" · ");
            sb.Append(ItemDataManager.GetItemName(item.Key));
            sb.Append(" x ");
            sb.Append(item.Value);
            sb.AppendLine();
        }
        sb.Remove(sb.Length - 2, 2);
        return sb.ToString();
    }
}
