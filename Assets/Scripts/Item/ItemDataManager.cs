using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson;
using UnityEngine.SceneManagement;
using GameId;

/// <summary>
/// 储存普通道具
/// </summary>
public class ItemDataManager : MonoBehaviour
{
    private const string INIT_ITEMS_FILEPATH = "scripts/player/item/initItems.php";
    private const string GET_ITEMS_FILEPATH = "scripts/player/item/loadItems.php";
    public static ItemDataManager Instance { get; set; }

    /// <summary>
    /// 玩家拥有道具的数量
    /// </summary>
    public static Dictionary<ItemBase, lint> itemsToAmount = new Dictionary<ItemBase, lint>();
    public static Dictionary<string, ItemBase> idsToItems = new Dictionary<string, ItemBase>();
    public static WWW InitWWWPost;
    /// <summary>
    /// 该实例初始化时是不是要直接下载
    /// </summary>
    public static bool StartInit = false;

    void Awake()
    {
        Instance = this;

    }

    #region 道具更新和初始化
    /// <summary>
    /// 初始化所有道具，一般在游戏一开始时使用
    /// </summary>
    public static Coroutine InitAllItems()
    {
        itemsToAmount = new Dictionary<ItemBase, lint>();
        idsToItems = new Dictionary<string, ItemBase>();
        return Instance.StartCoroutine(Instance.InitAllItemsCor());
    }
    /// <summary>
    /// 请求更新指定道具玩家所拥有的数量和道具位置
    /// </summary>
    /// <param name="item"></param>
    public static Coroutine RequestGetItemsAmount()
    {
        return Instance.StartCoroutine(Instance.GetItemAmountCor());
    }

    /// <summary>
    /// 更新道具数量和道具位置的协程
    /// </summary>
    /// <returns></returns>
    IEnumerator GetItemAmountCor()
    {
        CU.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("type", 0);
        WWW w = new WWW(CU.ParsePath(GET_ITEMS_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != null)
        {
            TempItemInfo[] tempItems = JsonHelper.GetJsonArray<TempItemInfo>(w.text);

            if (tempItems != null && tempItems.Length != 0)
            {

                foreach (TempItemInfo temp in tempItems)
                {
                    if (idsToItems.ContainsKey(temp.item_id))
                    {
                        ItemBase item = idsToItems[temp.item_id];
                        if (itemsToAmount.ContainsKey(item))
                        {
                            itemsToAmount[item] = temp.amount;
                            item.SetIndexInPack(temp.indexInPack);
                        }
                    }
                }
            }
        }
        CU.HideConnectingUI();

    }

    /// <summary>
    /// 初始化道具的协程,获得图标
    /// </summary>
    /// <returns></returns>
    IEnumerator InitAllItemsCor()
    {
        CU.ShowConnectingUI();
        WWW InitWWWPost = new WWW(CU.ParsePath(INIT_ITEMS_FILEPATH));
        yield return InitWWWPost;
        if (CU.IsPostSucceed(InitWWWPost))
        {
            ItemBase[] tempItems = JsonHelper.GetJsonArray<ItemBase>(InitWWWPost.text);

            if (tempItems != null && tempItems.Length != 0)
            {

                foreach (ItemBase temp in tempItems)
                {
                    itemsToAmount.Add(temp, 0);
                    idsToItems.Add(temp.item_id, temp);
                    temp.GenarateCompoundData();
                    yield return StartCoroutine(RequestLoadTexture(temp));
                }
            }
            string path = "icons/empty.png";
            if (!SpriteLibrary.IsSpriteDownLoading(path))
            {
                SpriteLibrary.SetSpriteDownLoading(path);
                WWW w = new WWW(CU.ParsePath(path));
                CU.ShowConnectingUI();
                yield return w;
                CU.HideConnectingUI();
                if (CU.IsDownloadCompleted(w))
                {
                    Texture2D iconTex = w.texture;
                    iconTex.Compress(true);
                    Sprite _icon = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
                    ///预加载
                    SpriteLibrary.AddSprite(path, _icon);
                    ///
                }
                else
                {
                    Debug.LogWarning(w.error);
                    CU.ShowConnectFailed();
                    yield break;
                }
                w.Dispose();
            }
        }
        CU.HideConnectingUI();
    }
    #endregion

    /// <summary>
    /// 返回道具名
    /// </summary>
    /// <param name="id">若为money，返回星币</param>
    /// <returns></returns>
    public static string GetItemName(string id)
    {
        if (id == Items.MONEY)
        {
            return "星币";
        }
        if (id == Items.DIMEN)
        {
            return "次元币";
        }
        return idsToItems[id].name;
    }
    /// <summary>
    /// 通过id返回ItemBase类
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static ItemBase GetItemBase(string id)
    {
        return idsToItems.ContainsKey(id) ? idsToItems[id] : null;
    }

    /// <summary>
    /// 把当前的道具全加入m_items内，应确保m_items经过初始化
    /// </summary>
    public static void AddItemsToPlayerInfo()
    {
        if (itemsToAmount == null)
        {
            return;
        }
        foreach (var kv in itemsToAmount)
        {
            if (kv.Value > 0)
            {
                PlayerInfoInGame.CurrentItems.Add(kv.Key);
            }
        }
    }

    /// <summary>
    /// 把道具的数量通通清空
    /// </summary>
    public void ClearItemAmount()
    {
        foreach (var kv in itemsToAmount)
        {
            itemsToAmount[kv.Key] = 0;
        }
    }
    /// <summary>
    /// 返回指定道具的数量
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static lint GetItemAmount(ItemBase item)
    {
        return itemsToAmount.ContainsKey(item) ? itemsToAmount[item] : (lint)0;
    }
    /// <summary>
    /// 返回指定道具的数量，如果是money则返回星币
    /// </summary>
    /// <param name="item_id"></param>
    /// <returns></returns>
    public static lint GetItemAmount(string item_id)
    {
        if (item_id == "money")
        {
            return PlayerInfoInGame.GetMoney();
        }
        return idsToItems.ContainsKey(item_id) ? itemsToAmount[idsToItems[item_id]] : (lint)0;
    }
    public static Coroutine LoadTargetItemIcon(ItemBase item)
    {
        return Instance.StartCoroutine(Instance.RequestLoadTexture(item));
    }

    IEnumerator RequestLoadTexture(ItemBase linkedItem)
    {
        string path = linkedItem.GetIconPath();
        if (!SpriteLibrary.IsSpriteDownLoading(path))
        {
            SpriteLibrary.SetSpriteDownLoading(path);
            WWW w = new WWW(CU.ParsePath(path));
            CU.ShowConnectingUI();
            yield return w;
            CU.HideConnectingUI();
            if (CU.IsDownloadCompleted(w))
            {
                Texture2D iconTex = w.texture;
                iconTex.Compress(true);
                Sprite _icon = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
                ///预加载
                SpriteLibrary.AddSprite(path, _icon);
                ///
            }
            else
            {
                Debug.LogWarning(w.error);
                CU.ShowConnectFailed();
                yield break;
            }
            w.Dispose();
        }
    }



}
