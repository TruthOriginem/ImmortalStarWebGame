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
            gameObject.SetActive(true);
            ItemBase item = grid.GetLinkedItem();
            title.text = item.name;
            text.text = grid.GetToolTipDescription();
        }
    }
    void OnMouseExit()
    {
        gameObject.SetActive(false);
    }
    void SetPositon()
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform, Input.mousePosition, Camera.main, out pos))
        {
            transform.localPosition = pos;
        }
    }
}
