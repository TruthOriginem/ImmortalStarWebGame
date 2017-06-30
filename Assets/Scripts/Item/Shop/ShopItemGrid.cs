using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text;
using SerializedClassForJson;

public class ShopItemGrid : MonoBehaviour
{
    public string item_Id;

    public Text itemName;
    public Image itemImage;

    public InputField input;
    private ItemBase linkedItem;

    private bool isDirty = true;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isDirty)
        {
            linkedItem = ItemDataManager.GetItemBase(item_Id);
            if (linkedItem != null)
            {
                itemName.text = linkedItem.name;
                itemImage.sprite = ItemModal.GetIconByPath(linkedItem.GetIconPath());
                if (itemImage.sprite != null)
                {
                    isDirty = false;
                }
            }
        }
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
                int price = number * linkedItem.price;
                int dimen = number * linkedItem.dimen;
                StringBuilder sb = new StringBuilder();
                sb.Append("您确定要购买 ");
                sb.Append(linkedItem.name);
                sb.Append(" x ");
                sb.Append(number);
                sb.Append(" 吗？");
                sb.AppendLine();
                sb.Append("这将消耗你 ");
                if (price > 0)
                {
                    sb.Append("<b>");
                    sb.Append(price);
                    sb.Append("</b>");
                    sb.Append(" 星币");
                }
                if(dimen > 0)
                {
                    sb.Append("<b>");
                    sb.Append(dimen);
                    sb.Append("</b>");
                    sb.Append(" 次元币");
                }
                sb.Append("。");
                MessageBox.Show(sb.ToString(), "确认", (dialogResult) => { StartCoroutine(ConfirmBuy(dialogResult, number, price,dimen)); }, MessageBoxButtons.YesNo);
            }
        }
    }
    /// <summary>
    /// 确认购买的协程
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    IEnumerator ConfirmBuy(DialogResult result, int number, int price,int dimen)
    {
        if (result == DialogResult.Yes)
        {
            if (price <= PlayerInfoInGame.GetMoney() && dimen<=PlayerInfoInGame.GetDimenCoin())
            {
                string[] itemids = { item_Id };
                int[] amounts = { number };
                IIABinds bind = new IIABinds(itemids, amounts);
                TempPlayerAttribute attr = new TempPlayerAttribute();
                attr.money -= price;
                attr.dimenCoin -= dimen;
                yield return PlayerRequestBundle.RequestUpdateRecord<UnityEngine.Object>(null, bind,attr);
                input.text = "";
                PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
            }
            else
            {
                MessageBox.Show("您的余额不足。");
            }
        }
    }
    public ItemBase GetLinkedItem()
    {
        return linkedItem;
    }
}
