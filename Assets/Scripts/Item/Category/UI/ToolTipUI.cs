using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class ToolTipUI : MonoBehaviour
{
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text content;

    private ItemBase linkedItem;//目标
    private EquipmentBase comparedEq = null;//用来比较的item
    public ItemUI linkedItemUI { get; set; }
    /// <summary>
    /// 使提示框指向ItemBase的一个对象。如果不是装备，不存在对比的装备的话，cItem填null。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cItem"></param>
    public void LinkItem(ItemBase item, EquipmentBase cItem)
    {
        linkedItem = item;
        comparedEq = cItem;
        UpdateItem();
    }
    public void UpdateItem()
    {
        title.text = linkedItem.name;
        StringBuilder contentSb = new StringBuilder();
        if (linkedItem is EquipmentBase)
        {

            EquipmentBase equipment = (EquipmentBase)linkedItem;
            title.text = equipment.GetModifyiedName();
            contentSb.AppendLine(equipment.GetRehanceString());
            contentSb.AppendLine("<size=18><b>" + equipment.GetEqTypeName() + "</b></size>");
            contentSb.AppendLine(TextUtils.GetSpbText("灵基 -- " + equipment.GetSpb().ToString("0.0")));
            contentSb.AppendLine(linkedItem.description);
            contentSb.AppendLine();
            EquipmentValue value = equipment.GetProperties();

            if (comparedEq != null)
            {
                if (comparedEq == linkedItem)
                {
                    foreach (var kv in value.values)
                    {
                        IProperty property = kv.Value;
                        if (property.Value != 0f)
                        {
                            contentSb.AppendLine(property.GetName() + " " + property.GetValueToString());
                        }
                    }
                }
                else
                {
                    EquipmentValue cValue = comparedEq.GetProperties();
                    foreach (var kv in value.values)
                    {
                        float pro = kv.Value.Value;
                        float Cpro = cValue.values[kv.Key].Value;
                        if (!(pro == 0f && Cpro == 0f))
                        {
                            contentSb.AppendLine(kv.Value.GetName() + " " + kv.Value.GetValueToString() + GetCompareString(pro, Cpro));
                        }
                    }
                }

            }
            else
            {
                foreach (var kv in value.values)
                {
                    IProperty property = kv.Value;
                    if (property.Value != 0f)
                    {
                        contentSb.AppendLine(property.GetName() + " " + property.GetValueToString() + GetCompareString(property.Value, 0f));
                    }
                }
            }

            contentSb.AppendLine("\n<color=#808080ff><size=10>按右键以装备或卸除</size></color>");
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(linkedItem.description);
            sb.AppendLine();
            sb.Append("<color=#FFED00FF>持有数量</color>：" + linkedItem.GetAmount());
            contentSb.AppendLine(sb.ToString());
        }
        contentSb.AppendLine();
        if (linkedItem.CanBeSold())
        {
            contentSb.Append("<size=10>物品售价:" + linkedItem.price/2);
            contentSb.Append(linkedItem.GetAmount() > 1 ? " * " + linkedItem.GetAmount() + " = " + linkedItem.GetAmount() * linkedItem.price/2 : "");
            contentSb.Append("</size>");
        }
        else
        {
            contentSb.Append("<color=red>不可出售</color>");
        }
        content.text = contentSb.ToString();
    }
    public ItemBase getLinkedItem()
    {
        return linkedItem;
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void SetLocalPosition(Vector2 position)
    {
        transform.localPosition = position;
    }

    string GetCompareString(float value, float comValue)
    {
        if (value > comValue)
        {
            return "<color=#00ff00ff>(+" + (value - comValue).ToString("0.0") + ")</color>";
        }
        else if (value < comValue)
        {
            return "<color=#ff0000ff>(" + (value - comValue).ToString("0.0") + ")</color>";
        }
        else
        {
            return "(+0)";
        }
    }
    /// <summary>
    /// 加入后换一行
    /// </summary>
    /// <param name="text"></param>
    void AddTextToContent(string text)
    {
        content.text += text + "\n";
    }
}
