using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SerializedClassForJson;
using System.Text;
using PathologicalGames;
using GameId;

public class CategoryManager : MonoBehaviour
{
    private static CategoryManager _instance;
    public static CategoryManager Instance { get { return _instance; } }

    [SerializeField]
    private PackGridPanelUI gridPanel;
    [SerializeField]
    private DragItemUI dragItemUI;
    [SerializeField]
    private ToolTipUI toolTipUI;
    [SerializeField]
    private SpawnPool spawnPool;
    [SerializeField]
    private Transform itemPrefab;

    private bool isDragingItem = false;
    private bool isToolTipHelping = false;
    /// <summary>
    /// 如果是更新玩家界面。。
    /// </summary>
    private bool isRefreshing = false;

    void Awake()
    {
        _instance = this;

        #region 加入回调函数
        BaseGridUI.onTipPointerEnter = null;
        BaseGridUI.onTipPointerEnter += OnGridEnter;
        BaseGridUI.onTipPointerExit = null;
        BaseGridUI.onTipPointerExit += OnGridExit;
        BaseGridUI.onGridBeginDrag = null;
        BaseGridUI.onGridBeginDrag += Grid_BeginDrag;
        BaseGridUI.onGridEndDrag = null;
        BaseGridUI.onGridEndDrag += Grid_EndDrag;
        BaseGridUI.onGridRightClick = null;
        BaseGridUI.onGridRightClick += OnGridRightClick;


        #endregion
    }

    /// <summary>
    /// 用协程载入背包
    /// </summary>
    public Coroutine RequestLoad()
    {
        return StartCoroutine(Load());
    }
    /// <summary>
    /// RequestLoad()中的协程，模拟数据库载入
    /// </summary>
    IEnumerator Load()
    {
        isDragingItem = false;
        isToolTipHelping = false;
        ItemModal.Clear();
        ClearRecordItemUIs();
        yield return StartCoroutine(RequestUpdateCatogory());
        gridPanel.DeleteAboveRange();
    }

    /// <summary>
    /// 要求更新玩家身上的items
    /// </summary>
    IEnumerator RequestUpdateCatogory()
    {
        if (PlayerInfoInGame.Instance == null)
        {
            yield break;
        }
        yield return PlayerRequestBundle.RequestUpdateItemsInPack();
        List<ItemBase> items = PlayerInfoInGame.CurrentItems;
        if (items.Count == 0)
        {
            yield break;
        }
        isRefreshing = true;
        for (int i = 0; i < items.Count; i++)
        {
            AddItem(items[i]);
        }
        isRefreshing = false;
    }
    void ClearRecordItemUIs()
    {
        gridPanel.ClearAllItemUI();
    }

    void Update()
    {
        //测试

        /*
        if (Input.GetMouseButtonDown(2))
        {

        }
        */
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(toolTipUI.transform.parent.transform as RectTransform
                                , Input.mousePosition, Camera.main, out position);
        if (isDragingItem)
        {
            dragItemUI.SetLocalPosition(position);
            toolTipUI.Hide();
        }
        else if (isToolTipHelping)
        {
            float height = toolTipUI.rectTransform.rect.height + 30;
            position.x -= toolTipUI.rectTransform.rect.width + 30;
            position.y += 20;
            //float width = toolTipUI.GetComponent<RectTransform>().rect.width;
            if (Input.mousePosition.y - height < 0)
            {
                position.y += height - Input.mousePosition.y;
            }
            toolTipUI.Show();
            toolTipUI.SetLocalPosition(position);
        }

    }

    /// <summary>
    /// 往界面里添加道具，每个道具会生成道具ui与道具链接
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    ItemBase AddItem(ItemBase item)
    {
        Transform grid;
        bool isEquipped = false;
        if (item is EquipmentBase)
        {
            if ((item as EquipmentBase).IsInStorage) return null;
            isEquipped = ((EquipmentBase)item).IsEquipped;
        }

        if (item.GetIndexInPack() == -1)
        {
            grid = gridPanel.GetEmptyGrid();
        }
        else
        {
            if (isEquipped)
            {
                grid = gridPanel.GetEmptyGrid(item.GetIndexInPack(), true);
            }
            else
            {
                grid = gridPanel.GetEmptyGrid(item.GetIndexInPack(), false);
                if (grid == null)
                {
                    grid = gridPanel.GetEmptyGrid();
                }

            }
        }
        if (grid == null)
        {
            // Debug.LogWarning("背包已满");
            return null;
        }
        CreateNewItemUI(item, grid);
        return item;
    }

    void RemoveItem(string gridName)
    {
        ItemModal.RemoveItemUI(gridName);
    }

    #region 拖拽回调函数
    void Grid_BeginDrag(Transform grid)
    {
        if (grid.childCount == 0)
        {
            return;
        }
        else
        {
            ItemUI itemUI = ItemModal.GetItemUI(grid.name);
            if (itemUI.GetLinkedItem() == null)
            {
                return;
            }
            ItemBase item = itemUI.GetLinkedItem();
            dragItemUI.Show();
            dragItemUI.LinkItem(item);
            dragItemUI.SetIcon(itemUI.GetIcon());
            isDragingItem = true;
            DespawnItemUI(itemUI.transform);
        }
    }
    void Grid_EndDrag(Transform prevTransform, Transform enterTransform)
    {
        if (!isDragingItem)
        {
            return;
        }
        //Debug.Log(enterTransform.name
        if (enterTransform == null)
        {
            CreateNewItemUI(dragItemUI.GetLinkedItem(), prevTransform);
        }
        else
        {
            if (enterTransform.tag == "Grid")//拖到格子里
            {
                BaseGridUI gridUI = enterTransform.GetComponent<BaseGridUI>();
                if (gridUI == null)
                {
                    Debug.LogWarning("出错了");
                }
                if (enterTransform.childCount == 0)//塞入空格子
                {
                    if (gridUI.GetPermittedType() == EQ_TYPE.NOT_EQUIPMENT)//如果该网格没有装备要求
                    {
                        ItemModal.RemoveItemUI(prevTransform.name);
                        CreateNewItemUI(dragItemUI.GetLinkedItem(), enterTransform);
                    }
                    else//如果该网格有装备要求
                    {
                        if (dragItemUI.GetLinkedItem() is EquipmentBase)//如果是装备
                        {
                            //Debug.Log("是装备");
                            EquipmentBase equipment = (EquipmentBase)dragItemUI.GetLinkedItem();
                            if (gridUI.GetPermittedType() == equipment.GetEquipmentType())//如果是对应类型装备
                            {
                                ItemModal.RemoveItemUI(prevTransform.name);
                                CreateNewItemUI(dragItemUI.GetLinkedItem(), enterTransform);
                            }
                            else
                            {
                                CreateNewItemUI(dragItemUI.GetLinkedItem(), prevTransform);//如果不是对应类型装备则返回原来
                            }
                        }
                        else//如果不是
                        {
                            CreateNewItemUI(dragItemUI.GetLinkedItem(), prevTransform);
                        }

                    }
                }
                else//交换
                {
                    if (gridUI.GetPermittedType() == EQ_TYPE.NOT_EQUIPMENT)//如果该网格没有装备要求
                    {
                        ItemBase item = ItemModal.GetItemUI(enterTransform.name).GetLinkedItem();
                        DespawnItemUI(enterTransform.GetChild(0));
                        CreateNewItemUI(item, prevTransform);
                        CreateNewItemUI(dragItemUI.GetLinkedItem(), enterTransform);
                    }
                    else
                    {
                        if (dragItemUI.GetLinkedItem() is EquipmentBase)
                        {
                            EquipmentBase equipment = (EquipmentBase)dragItemUI.GetLinkedItem();
                            if (gridUI.GetPermittedType() == equipment.GetEquipmentType())//如果是对应类型装备
                            {
                                ItemBase item = ItemModal.GetItemUI(enterTransform.name).GetLinkedItem();
                                DespawnItemUI(enterTransform.GetChild(0));
                                CreateNewItemUI(item, prevTransform);
                                CreateNewItemUI(dragItemUI.GetLinkedItem(), enterTransform);
                            }
                            else
                            {
                                CreateNewItemUI(dragItemUI.GetLinkedItem(), prevTransform);//如果不是对应类型装备则返回原来
                            }
                        }
                        else
                        {
                            CreateNewItemUI(dragItemUI.GetLinkedItem(), prevTransform);
                        }

                    }

                }

            }
            else//拖到格子以外的地方
            {
                CreateNewItemUI(dragItemUI.GetLinkedItem(), prevTransform);
            }
        }
        isDragingItem = false;
        dragItemUI.Hide();
    }
    #endregion

    #region 提示回调函数
    void OnGridEnter(Transform gridTransform)
    {
        if (isDragingItem)
        {
            //Debug.Log("Draging");

            return;
        }
        if (gridTransform.childCount == 0 || gridTransform.tag != "Grid")
        {
            //Debug.Log("Nothing");
            return;
        }
        else if (ItemModal.GetItemUI(gridTransform.name) != null)
        {
            isToolTipHelping = true;
            var itemUI = ItemModal.GetItemUI(gridTransform.name);
            itemUI.BeHighLight();
            ItemBase item = itemUI.GetLinkedItem();
            if (item is EquipmentBase)
            {
                EquipmentBase equipment = (EquipmentBase)item;
                toolTipUI.linkedItemUI = itemUI;
                toolTipUI.LinkItem(item, FindEqiupmentInSpecType(equipment.GetEquipmentType()));
            }
            else
            {
                toolTipUI.linkedItemUI = itemUI;
                toolTipUI.LinkItem(item, null);
            }
            toolTipUI.Show();
        }
    }
    void OnGridExit()
    {
        SetToolTipHide();
    }
    void SetToolTipHide()
    {
        if (toolTipUI.linkedItemUI != null)
        {
            toolTipUI.linkedItemUI.DeactHighLight();
            toolTipUI.linkedItemUI = null;
        }
        isToolTipHelping = false;

        toolTipUI.Hide();
    }
    #endregion

    void OnGridRightClick(Transform gridTransform)
    {
        if (gridTransform.childCount == 0)
        {
            return;
        }
        BaseGridUI gridUI = gridTransform.GetComponent<BaseGridUI>();
        if (gridUI.GetPermittedType() != EQ_TYPE.NOT_EQUIPMENT)//如果是装备栏
        {
            ItemUI itemUI = ItemModal.GetItemUI(gridTransform.name);
            ItemModal.RemoveItemUI(gridTransform.name);
            DespawnItemUI(itemUI.transform);
            CreateNewItemUI(itemUI.GetLinkedItem(), gridPanel.GetEmptyGrid());
        }
        else//如果不是
        {
            ItemUI itemUI = ItemModal.GetItemUI(gridTransform.name);//当前背包格的itemui
            if (itemUI.GetLinkedItem() is EquipmentBase)//如果网格内的是装备
            {
                EquipmentBase equipment = (EquipmentBase)itemUI.GetLinkedItem();
                Transform targetGrid = gridPanel.FindEqTypeGrid(equipment.GetEquipmentType());
                if (targetGrid == null)
                {
                    return;
                }
                ItemModal.RemoveItemUI(gridTransform.name);
                DespawnItemUI(itemUI.transform);
                if (targetGrid.childCount == 0)//如果指定装备栏为空
                {
                    CreateNewItemUI(itemUI.GetLinkedItem(), targetGrid);
                }
                else//如果装备栏存在相同类型装备
                {
                    ItemBase item = targetGrid.GetChild(0).GetComponent<ItemUI>().GetLinkedItem();
                    ItemModal.RemoveItemUI(targetGrid.name);
                    DespawnItemUI(targetGrid.GetChild(0));
                    CreateNewItemUI(equipment, targetGrid);
                    CreateNewItemUI(item, gridPanel.GetEmptyGrid());
                }
            }
        }
        OnGridEnter(gridTransform);

    }
    /// <summary>
    /// 查询在装备格内的指定类型装备
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    EquipmentBase FindEqiupmentInSpecType(EQ_TYPE type)
    {
        Transform targetGrid = gridPanel.FindEqTypeGrid(type);
        if (targetGrid != null)
        {
            if (targetGrid.childCount == 0)
            {
                return null;
            }
            else
            {
                EquipmentBase equipment = (EquipmentBase)ItemModal.GetItemUI(targetGrid.name).GetLinkedItem();
                return equipment;
            }
        }
        return null;
    }
    /// <summary>
    /// 在某个网格里新建一个指定物品,会让该物品的PackIndex改变为parent所在index
    /// <para>这个过程会要求联网</para>
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void CreateNewItemUI(ItemBase item, Transform parent)
    {
        Transform itemTrans = spawnPool.Spawn(itemPrefab);
        //GameObject itemGo = GameObject.Instantiate(Resources.Load("Prefab/Items/UI/Item")) as GameObject;
        itemTrans.SetParent(parent);
        itemTrans.localPosition = Vector3.zero;
        itemTrans.localScale = Vector3.one;
        BaseGridUI gridUI = parent.GetComponent<BaseGridUI>();
        ItemUI itemUI = itemTrans.GetComponent<ItemUI>();
        itemUI.LinkItem(item);
        if (gridUI.GetPermittedType() != EQ_TYPE.NOT_EQUIPMENT)//如果是网格有装备要求的话，则判定这个是装备
        {
            EquipmentBase equipment = (EquipmentBase)item;
            equipment.SetEquipped(true);
            item.SetIndexInPack(gridPanel.GetIndex(parent, true));
        }
        else//网格对装备没要求的话
        {
            if (item is EquipmentBase)//如果是装备，在背包内就自动设置为未装备
            {
                ((EquipmentBase)item).SetEquipped(false);
            }
            item.SetIndexInPack(gridPanel.GetIndex(parent, false));
        }
        //如果这时候是互换、摧毁等场合的话
        if (!isRefreshing)
        {
            PlayerRequestBundle.UpdateItemsIndex(item);
        }

        ItemModal.AddItemUI(parent.name, itemUI);
    }
    /// <summary>
    /// 整理背包。正在装备的不参与排序
    /// <para>0为装备为最前，根据灵基排序</para>
    /// </summary>
    /// <param name="type"></param>
    public void SortItems(int type)
    {
        List<ItemBase> items = PlayerInfoInGame.GetAllItems();
        items.Sort((x, y) => { return x.sort.CompareTo(y.sort); });
        List<EquipmentBase> equips = PlayerInfoInGame.GetAllEquipments(false);
        //最终完成排序的List，需要上传
        Dictionary<ItemBase, int> sortedItems = new Dictionary<ItemBase, int>();
        int index = 0;
        switch (type)
        {
            case 0://武器优先，灵基最高
                {
                    while (equips.Count != 0)
                    {
                        EquipmentBase temp = null;
                        float spb = 0;
                        for (int i = 0; i < equips.Count; i++)
                        {
                            if (equips[i].GetSpb() >= spb)
                            {
                                temp = equips[i];
                                spb = equips[i].GetSpb();
                            }
                        }
                        if (temp != null)
                        {
                            equips.Remove(temp);
                            sortedItems.Add(temp, index++);
                        }
                    }
                    for (int i = 0; i < items.Count; i++)
                    {
                        sortedItems.Add(items[i], index++);
                    }
                    break;
                }
            default:
                break;
        }
        StartCoroutine(SortCor(sortedItems));
    }
    public void QuickDestoryEquipments()
    {
        InputIntBox.Show("指定一个灵基值，摧毁该灵基值及以下的装备。（不包括已被装备的）", "温馨提示", int.MaxValue, (result, value) =>
        {
            if (result == DialogResult.OK)
            {
                List<string> eqToDestory = new List<string>();
                lint spbPieces = 0;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<size=10>");
                foreach (var item in PlayerInfoInGame.GetAllEquipments(false))
                {
                    EquipmentBase equip = (EquipmentBase)item;
                    if (equip.GetSpb() <= value)
                    {
                        eqToDestory.Add(equip.item_id);
                        spbPieces += equip.GetSpbPieceAmount();
                        sb.AppendLine(equip.GetModifyiedName());
                    }
                }
                sb.Append("</size>");
                if (eqToDestory.Count != 0)
                {
                    MessageBox.Show("您是否确定摧毁灵基值在" + value + "及以下的装备？你将摧毁以下" + eqToDestory.Count + "件装备并获得" + spbPieces + "灵基碎片。" + sb.ToString(), "警告", (result2) =>
                    {
                        if (result2 == DialogResult.Yes)
                        {
                            StartCoroutine(_QuickDestoryEquipments(eqToDestory.ToArray(), spbPieces));
                        }
                    }, MessageBoxButtons.YesNo);
                }
                else
                {
                    MessageBox.Show("没有装备符合你的标准。", "提示", (result3) => { }, MessageBoxButtons.OK);
                }
            }
        }, MessageBoxButtons.OKCancel);
    }
    /// <summary>
    /// 快捷出售指定灵基以下的武器。
    /// </summary>
    public void QuickSellEquipments()
    {
        InputIntBox.Show("指定一个灵基值，出售该灵基值及以下的装备。（不包括已被装备的）", "温馨提示", int.MaxValue, (result, value) =>
           {
               if (result == DialogResult.OK)
               {
                   List<string> eqToSell = new List<string>();
                   int sellmoney = 0;
                   StringBuilder sb = new StringBuilder();
                   sb.AppendLine("<size=10>");
                   foreach (var item in PlayerInfoInGame.GetAllEquipments(false))
                   {
                       if (item.CanBeSold())
                       {
                           EquipmentBase equip = (EquipmentBase)item;
                           if (equip.GetSpb() <= value)
                           {
                               eqToSell.Add(equip.item_id);
                               sellmoney += equip.price;
                               sb.AppendLine(equip.GetModifyiedName());
                           }
                       }
                   }
                   sb.Append("</size>");
                   if (eqToSell.Count != 0)
                   {
                       MessageBox.Show("您是否确定出售灵基值在" + value + "及以下的装备？你将出售以下" + eqToSell.Count + "件装备并获得" + sellmoney + "星币。" + sb.ToString(), "警告", (result2) =>
                                       {
                                           if (result2 == DialogResult.Yes)
                                           {
                                               StartCoroutine(QuickSellEquipmentsCor(eqToSell.ToArray(), sellmoney));
                                           }
                                       }, MessageBoxButtons.YesNo);
                   }
                   else
                   {
                       MessageBox.Show("没有装备符合你的标准。", "提示", (result3) => { }, MessageBoxButtons.OK);
                   }
               }
           }, MessageBoxButtons.OKCancel);
    }
    IEnumerator QuickSellEquipmentsCor(string[] equipids, int price)
    {
        TempPlayerAttribute attr = new TempPlayerAttribute();
        attr.money = price;
        yield return PlayerRequestBundle.RequestUpdateRecord<Object>(null, new IIABinds(null, null, equipids), price <= 0 ? null : attr, null);
        yield return RequestLoad();
    }
    IEnumerator _QuickDestoryEquipments(string[] equipids, int spbPieces)
    {
        SyncRequest.AppendRequest(Requests.EQ_TO_DELETE_DATA, new IIABinds(null, null, equipids).ToJson(true));
        SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(Items.SPB_PIECE, spbPieces));
        yield return PlayerRequestBundle.RequestSyncUpdate(false);
        yield return RequestLoad();
    }
    IEnumerator SortCor(Dictionary<ItemBase, int> sortedItems)
    {
        yield return PlayerRequestBundle.RequestUpdateIndexInPack(sortedItems);
        yield return RequestLoad();
    }
    void DespawnItemUI(Transform item)
    {
        item.SetParent(transform);
        spawnPool.Despawn(item);
    }
}
