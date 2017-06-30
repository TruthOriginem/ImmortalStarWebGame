using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class ShopItemToolTip : MonoBehaviour
{
    private Text title;
    private Text text;
    void Start()
    {
        title = transform.Find("Title").GetComponent<Text>();
        text = transform.Find("Text").GetComponent<Text>();
        ShopItemGridImage.OnMouseEnter = null;
        ShopItemGridImage.OnMouseExit = null;
        ShopItemGridImage.OnMouseEnter += OnMouseEnterShopItemGrid;
        ShopItemGridImage.OnMouseExit += OnMouseExit;
        gameObject.SetActive(false);
    }

    void Update()
    {
        SetPositon();
    }
    void OnMouseEnterShopItemGrid(ShopItemGrid grid)
    {
        if (grid.GetLinkedItem() != null)
        {
            SetPositon();
            gameObject.SetActive(true);
            ItemBase item = grid.GetLinkedItem();
            title.text = item.name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(item.description);
            sb.AppendLine();
            sb.AppendLine("物品售价：");
            if (item.price > 0)
            {
                sb.Append("<b>");
                sb.Append(item.price);
                sb.Append("</b>");

                sb.AppendLine("星币");
            }
            if (item.dimen > 0)
            {
                sb.Append("<b>");

                sb.Append(item.dimen);
                sb.Append("</b>");

                sb.AppendLine("次元币");
            }
            text.text = sb.ToString();
        }
    }
    void SetPositon()
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform, Input.mousePosition, Camera.main, out pos))
        {
            transform.localPosition = pos;
        }
    }
    void OnMouseExit()
    {
        gameObject.SetActive(false);
    }
}
