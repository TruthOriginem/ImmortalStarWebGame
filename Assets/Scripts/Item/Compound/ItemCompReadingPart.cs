using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 合成面板右半边，负责合成调用
/// </summary>
public class ItemCompReadingPart : MonoBehaviour
{
    [Header("UI内容上半")]
    public Image itemIconImage;
    public Text itemNameText;
    public Text itemDesText;
    public Text itemNeedText;
    [Header("UI内容下半")]
    public Text compAmount;
    public Slider compSlider;
    public Button compButton;
    public Text maxValueText;

    public static ItemCompReadingPart Instance { get; set; }

    private ItemCompoundData linkedCompoundData;
    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 整体更新合成说明界面。
    /// </summary>
    /// <param name="data"></param>
    public void RefreshUI(ItemCompoundData data)
    {
        if (data == null)
        {
            return;
        }
        linkedCompoundData = data;
        ItemBase item = ItemDataManager.GetItemBase(data.ComedItemId);
        itemIconImage.sprite = SpriteLibrary.GetSprite(item.GetIconPath());
        itemNameText.text = item.name;
        itemDesText.text = item.description;
        int amount;
        compButton.interactable = data.IfCanCompound(out amount);
        compSlider.maxValue = amount;
        maxValueText.text = amount.ToString();
        compSlider.minValue = 0;
        compSlider.value = amount > 0 ? 1 : 0;
        RefreshNeedsUI();
    }
    /// <summary>
    /// Slider变更时调用，更新itemNeedText
    /// </summary>
    public void RefreshNeedsUI()
    {
        int amount = GetAmountToComp();
        compAmount.text = "合成数量:" + amount;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(TextUtils.GetYellowText("单个需求："));
        foreach (var kv in linkedCompoundData.GetNeedsDict())
        {
            string name = ItemDataManager.GetItemName(kv.Key);
            sb.Append(name);
            sb.Append(" x ");
            sb.Append(kv.Value);
            int diff = ItemDataManager.GetItemAmount(kv.Key) - kv.Value;
            sb.Append(diff >= 0 ? "(剩余:" : "(缺少:");
            sb.Append(Mathf.Abs(diff));
            sb.Append(")");
            sb.AppendLine();
        }
        if (linkedCompoundData.compPrice > 0)
        {
            sb.Append("星币 x ");
            sb.Append(linkedCompoundData.compPrice);
            sb.AppendLine();

        }
        if (amount > 1)
        {
            sb.AppendLine("--------------");
            sb.AppendLine(TextUtils.GetYellowText("当前多个需求:"));
            foreach (var kv in linkedCompoundData.GetNeedsDict())
            {
                string name = ItemDataManager.GetItemName(kv.Key);
                sb.Append(name);
                sb.Append(" x ");
                sb.Append(kv.Value * amount);
                int diff = ItemDataManager.GetItemAmount(kv.Key) - kv.Value * amount;
                sb.Append(diff >= 0 ? "(剩余:" : "(缺少:");
                sb.Append(Mathf.Abs(diff));
                sb.Append(")");
                sb.AppendLine();
            }
            if (linkedCompoundData.compPrice > 0)
            {
                sb.Append("星币 x ");
                sb.Append(linkedCompoundData.compPrice * amount);
            }
        }
        itemNeedText.text = sb.ToString();
        compButton.interactable = amount > 0;
    }

    /// <summary>
    /// 合成按钮调用。
    /// </summary>
    public void StartComp()
    {
        int amount = GetAmountToComp();
        int maxAmount;
        bool canComp = linkedCompoundData.IfCanCompound(out maxAmount);
        if (!canComp || amount > maxAmount)
        {
            RefreshUI(linkedCompoundData);
            return;
        }
        StringBuilder sb = new StringBuilder();
        sb.Append("请问您要合成");
        sb.Append(amount);
        sb.Append("个");
        sb.Append(ItemDataManager.GetItemName(linkedCompoundData.ComedItemId));
        sb.Append("吗？");
        MessageBox.Show(sb.ToString(), "温馨提示", (result) =>
          {
              if (result == DialogResult.Yes)
              {
                  StartCoroutine(_Comp());
              }
          }, MessageBoxButtons.YesNo);
    }
    IEnumerator _Comp()
    {
        //首先更新一波玩家道具
        yield return PlayerInfoInGame.Instance.RequestUpdatePlayerItems();
        //判断是否可以合成，如果有问题则当做网络问题
        int amount = GetAmountToComp();
        int maxAmount;
        linkedCompoundData.IfCanCompound(out maxAmount);
        if (amount > maxAmount)
        {
            CU.ShowConnectFailed();
            RefreshUI(linkedCompoundData);
            yield break;
        }
        //开始上传结果
        TempPlayerAttribute attr;
        Dictionary<string, lint> dict = linkedCompoundData.GetCompDict(amount, out attr);
        CU.ShowConnectingUI();
        yield return RequestBundle.RequestUpdateIIA(new IIABinds(dict), attr);
        ItemCompManager.Instance.InitOrRefresh(false);
        RefreshUI(linkedCompoundData);
        CU.HideConnectingUI();
    }
    /// <summary>
    /// 当前滑条指定的合成数量。
    /// </summary>
    /// <returns></returns>
    int GetAmountToComp()
    {
        return Mathf.RoundToInt(compSlider.value);
    }
}
