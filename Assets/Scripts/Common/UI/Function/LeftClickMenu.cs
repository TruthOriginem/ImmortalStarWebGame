using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SerializedClassForJson;
using GameId;
using System.Text;

public class LeftClickMenu : MonoBehaviour
{
    public static LeftClickMenu Instance { get; set; }
    public CanvasGroup canvas;
    [Header("按钮")]
    public Button sellButton;
    public Button dropButton;
    public Button destoryButton;

    private bool isShowing = false;

    public static bool GetMenuShowing()
    {
        return Instance.isShowing;
    }
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BaseGridUI.onGridLeftClick = null;
        BaseGridUI.onGridLeftClick += OnGridClickInCategory;
        gameObject.SetActive(false);

    }

    void Update()
    {
        if (isShowing)
        {
            canvas.alpha = Mathf.Lerp(canvas.alpha, 1f, Time.deltaTime * 20f);
        }
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;

            List<RaycastResult> list = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, list);
            foreach (var result in list)
            {
                if (result.gameObject == gameObject)
                {
                    return;
                }
            }
            Disable();
        }
    }
    /// <summary>
    /// 在背包内触发,出售时价格减半
    /// </summary>
    /// <param name="gridTransfrom"></param>
    public void OnGridClickInCategory(Transform gridTransfrom)
    {
        if (ItemModal.GetItemUI(gridTransfrom.name) != null)
        {
            ItemBase item = ItemModal.GetItemUI(gridTransfrom.name).GetLinkedItem();
            List<Button> buttons = new List<Button>();
            buttons.Add(item.CanBeSold() ? sellButton : dropButton);
            if (item is EquipmentBase)
            {
                buttons.Add(destoryButton);
            }
            ClickInit(buttons.ToArray());
            //    Debug.Log(item.GetAmount());
            sellButton.onClick.AddListener(() =>
            {
                if (item.GetAmount() == 1)
                {
                    MessageBox.Show("是否确认此操作？您将得到" + item.price / 2 + "星币。", "温馨提示", (result2) =>
                     {
                         if (result2 == DialogResult.Yes)
                         {
                             PlayerInfoInGame.Instance.StartCoroutine(OnGridClickInCategoryCor(item, 1, item.price / 2));
                         }
                     }, MessageBoxButtons.YesNo);
                }
                else
                {
                    InputIntBox.Show("您要出售多少" + item.name + "?", "提示", item.GetAmount(), (result, value) =>
                     {
                         if (result == DialogResult.OK && value > 0)
                         {
                             MessageBox.Show("是否确认此操作？您将得到" + item.price * value / 2 + "星币。", "温馨提示", (result2) =>
                                 {
                                     if (result2 == DialogResult.Yes)
                                     {
                                         PlayerInfoInGame.Instance.StartCoroutine(OnGridClickInCategoryCor(item, value, item.price / 2));
                                     }
                                 }, MessageBoxButtons.YesNo);
                         }
                     }, MessageBoxButtons.OKCancel);
                }
                Disable();
            });
            dropButton.onClick.AddListener(() =>
            {
                InputIntBox.Show("您要丢弃多少" + item.name + "?", "提示", item.GetAmount(), (result, value) =>
                {
                    if (result == DialogResult.OK && value > 0)
                    {
                        MessageBox.Show("是否确认此操作？", "温馨提示", (result2) =>
                        {
                            if (result2 == DialogResult.Yes)
                            {
                                PlayerInfoInGame.Instance.StartCoroutine(OnGridClickInCategoryCor(item, value, 0));
                            }
                        }, MessageBoxButtons.YesNo);
                    }
                }, MessageBoxButtons.OKCancel);
                Disable();
            });
            destoryButton.onClick.AddListener(() =>
            {
                EquipmentBase equip = item as EquipmentBase;
                int pieceAmount = GetSpbPiecesAmount((int)equip.GetSpb());
                StringBuilder sb = new StringBuilder();
                sb.Append("请问您要销毁这个装备吗？销毁此装备将获得");
                sb.Append(pieceAmount);
                sb.Append("个");
                sb.Append(TextUtils.GetSpbText("原灵碎片。"));
                MessageBox.Show(sb.ToString(), "温馨提示", (result) =>
                  {
                      if (result == DialogResult.Yes)
                      {
                          IIABinds binds = new IIABinds(new string[] { Items.SPB_PIECE }, new Currency[] { pieceAmount }, new string[] { equip.item_id });
                          PlayerInfoInGame.Instance.StartCoroutine(OnGridClickInCategoryCor(binds));
                      }
                  }, MessageBoxButtons.YesNo);
                Disable();
            });
        }
    }
    IEnumerator OnGridClickInCategoryCor(ItemBase item, int amount, int price)
    {
        TempPlayerAttribute attr = new TempPlayerAttribute();
        attr.money = price * amount;
        if (item is EquipmentBase)
        {
            yield return PlayerRequestBundle.RequestUpdateRecord<Object>(null, new IIABinds(null, null, new string[] { item.item_id }), price <= 0 ? null : attr, null);
        }
        else
        {
            yield return PlayerRequestBundle.RequestUpdateRecord<Object>(null, new IIABinds(new string[] { item.item_id }, new Currency[] { -amount }), price <= 0 ? null : attr, null);
        }
        yield return CategoryManager.Instance.RequestLoad();
    }
    IEnumerator OnGridClickInCategoryCor(IIABinds binds)
    {
        TempPlayerAttribute attr = new TempPlayerAttribute();
        yield return PlayerRequestBundle.RequestUpdateRecord<Object>(null, binds, null, null);
        yield return CategoryManager.Instance.RequestLoad();
    }
    /// <summary>
    /// 原灵碎片的兑换公式
    /// </summary>
    /// <param name="spb"></param>
    /// <returns></returns>
    int GetSpbPiecesAmount(int spb)
    {
        return spb >= 10 ? spb / 10 : 1;
    }
    /// <summary>
    /// 点击相应东西后会触发的内容
    /// </summary>
    /// <param name="buttonsToEnable"></param>
    void ClickInit(Button[] buttonsToEnable)
    {
        Enable();
        SetButtonsActive(buttonsToEnable);
        SetPosition();
    }
    void SetPosition()
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform, Input.mousePosition, Camera.main, out pos))
        {
            pos.x += 10;
            pos.y -= 10;
            transform.localPosition = pos;
        }
        transform.SetAsLastSibling();
    }
    /// <summary>
    /// 用于决定左键菜单显示哪些按钮
    /// </summary>
    /// <param name="buttons"></param>
    void SetButtonsActive(Button[] buttons)
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < allButtons.Length; i++)
        {
            allButtons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].onClick.RemoveAllListeners();
        }
    }

    void Disable()
    {
        canvas.alpha = 0f;
        isShowing = false;
        gameObject.SetActive(false);
    }
    void Enable()
    {
        isShowing = true;
        gameObject.SetActive(true);
    }
}
