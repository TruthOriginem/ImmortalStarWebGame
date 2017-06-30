using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ItemToCompoundUI : MonoBehaviour
{
    public Text itemNameUIText;

    private ItemCompoundData linkedData;

    /// <summary>
    /// 更新该条目
    /// </summary>
    public void UpdateUI(ItemCompoundData data)
    {
        linkedData = data;
        string name = ItemDataManager.GetItemName(data.ComedItemId);
        int amount;
        bool canComp = data.IfCanCompound(out amount);
        StringBuilder sb = new StringBuilder();
        if (canComp)
        {
            sb.Append(name);
            sb.Append("(");
            sb.Append(amount);
            sb.Append(")");
        }
        else
        {
            sb.Append(TextUtils.GetColoredText(name, Color.gray));
        }
        itemNameUIText.text = sb.ToString();
    }
    public void SelectThis()
    {
        ItemCompReadingPart.Instance.RefreshUI(linkedData);
    }
}
