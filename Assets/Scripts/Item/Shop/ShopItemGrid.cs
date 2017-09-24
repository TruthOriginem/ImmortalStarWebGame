using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text;
using SerializedClassForJson;
using GameId;
using InterfaceTools;
public class ShopItemGrid : MonoBehaviour
{
    public enum WORTH_TYPE
    {
        MONEY_OR_DIMEN,
        DEEP_SCORE

    }
    private string item_Id;

    public Text itemName;
    public Image itemImage;

    public InputField input;
    private ItemBase linkedItem;
    private WORTH_TYPE worthType;
    private object[] buyParams;

    /// <summary>
    /// 生成时需要设置的东西。
    /// </summary>
    /// <param name="buyParams">购买时的参数，一般[0]为积分</param>
    public ShopItemGrid SetParam(string item_id, WORTH_TYPE type = WORTH_TYPE.MONEY_OR_DIMEN, params object[] buyParams)
    {
        this.item_Id = item_id;
        this.worthType = type;
        this.buyParams = buyParams;
        linkedItem = ItemDataManager.GetItemBase(item_Id);
        if (linkedItem != null)
        {
            itemName.text = linkedItem.name;
            itemImage.sprite = SpriteLibrary.GetSprite(linkedItem.GetIconPath());
        }
        return this;
    }


    /// <summary>
    /// 购买指定的商品
    /// </summary>
    public void BuyLinkedItem()
    {
        if (input.text != null)
        {
            int number;
            bool result = int.TryParse(input.text, out number);
            if (result && number > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("您确定要购买 ");
                sb.Append(linkedItem.name);
                sb.Append(" x ");
                sb.Append(number);
                sb.AppendLine(" 吗？");
                sb.Append("这将消耗你 ");
                switch (worthType)
                {
                    case WORTH_TYPE.MONEY_OR_DIMEN:
                        int price = number * linkedItem.price;
                        int dimen = number * linkedItem.dimen;
                        if (price > 0)
                        {
                            sb.AppendLine("<b>");
                            sb.Append(price);
                            sb.Append("</b>");
                            sb.Append(" 星币");
                        }
                        if (dimen > 0)
                        {
                            sb.AppendLine("<b>");
                            sb.Append(dimen);
                            sb.Append("</b>");
                            sb.Append(" 次元币");
                        }
                        break;
                    case WORTH_TYPE.DEEP_SCORE:
                        int score = number * (int)buyParams[0];
                        sb.AppendLine();
                        sb.AppendFormat("<b>{0}</b> 回溯积分", score);
                        break;
                    default:
                        break;
                }
                MessageBox.Show(sb.ToString(), "确认", (dialogResult) => { StartCoroutine(ConfirmBuy(dialogResult, number)); }, MessageBoxButtons.YesNo);
            }
        }
    }
    /// <summary>
    /// 确认购买的协程
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    IEnumerator ConfirmBuy(DialogResult result, int number)
    {
        if (result == DialogResult.Yes)
        {
            bool meetCondition = false;
            switch (worthType)
            {
                case WORTH_TYPE.MONEY_OR_DIMEN:
                    int price = number * linkedItem.price;
                    int dimen = number * linkedItem.dimen;
                    if (price <= PlayerInfoInGame.GetMoney() && dimen <= PlayerInfoInGame.GetDimenCoin())
                    {
                        meetCondition = true;
                        string[] itemids = { item_Id };
                        lint[] amounts = { number };
                        IIABinds bind = new IIABinds(itemids, amounts);
                        TempPlayerAttribute attr = new TempPlayerAttribute();
                        attr.money -= price;
                        attr.dimenCoin -= dimen;
                        SyncRequest.AppendRequest(Requests.PLAYER_DATA, attr);
                        SyncRequest.AppendRequest(Requests.ITEM_DATA, bind.ToJson());
                        yield return PlayerRequestBundle.RequestSyncUpdate();
                    }
                    break;
                case WORTH_TYPE.DEEP_SCORE:
                    int score = number * (int)buyParams[0];
                    if (score <= DeepMemoryManager.CurrData.score)
                    {
                        meetCondition = true;
                        SyncRequest.AppendRequest(Requests.DEEP_MEMORY_DATA, new TempDeepMemorySyncData { addScore = -score });
                        SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(item_Id, number).ToJson());
                        yield return PlayerRequestBundle.RequestSyncUpdate(false);
                    }
                    break;
                default:
                    break;
            }
            input.text = "";
            if (!meetCondition)
            {
                MessageBox.Show("您的余额不足。");
            }
        }
    }
    public String GetToolTipDescription()
    {
        if (GetLinkedItem() != null)
        {
            ItemBase item = GetLinkedItem();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(item.description);
            sb.AppendLine();
            sb.Append("物品售价：");
            switch (worthType)
            {
                case WORTH_TYPE.MONEY_OR_DIMEN:
                    if (item.price > 0)
                    {
                        sb.AppendLine("<b>");
                        sb.Append(item.price);
                        sb.Append("</b>");

                        sb.Append("星币");
                    }
                    if (item.dimen > 0)
                    {
                        sb.AppendLine("<b>");

                        sb.Append(item.dimen);
                        sb.Append("</b>");

                        sb.Append("次元币");
                    }
                    break;
                case WORTH_TYPE.DEEP_SCORE:
                    int score = (int)buyParams[0];
                    sb.AppendLine();
                    sb.AppendFormat("<b>{0}</b> 回溯积分", score);
                    break;
            }
            return sb.ToString();
        }
        return "";
    }
    public ItemBase GetLinkedItem()
    {
        return linkedItem;
    }

    public void OnDespawned()
    {
        input.text = "";
    }
}
