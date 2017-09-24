using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InterfaceTools;
using PathologicalGames;
using GameId;
using ItemContainerSuite;

/// <summary>
/// 管理商店
/// </summary>
public class ShopItemManager : MonoBehaviour
{
    public static ShopItemManager Instance { get; set; }
    public Transform shopItemPrefab;
    public SpawnPool spawnPool;

    public Transform targetContent;
    private void Awake()
    {
        Instance = this;
    }
    public void Despawn()
    {
        spawnPool.DespawnAll();
        targetContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
    }

    public SpawnPool GetSpawnPool()
    {
        return spawnPool;
    }

    public ShopItemGrid Spawn()
    {
        Transform item = spawnPool.Spawn(shopItemPrefab, targetContent);
        item.SetAsLastSibling();
        return item.GetComponent<ShopItemGrid>();
    }

    public void RefreshRecommendView(bool toggle)
    {
        if (toggle)
        {
            Despawn();
            Spawn().SetParam(Items.CARD_EXP_DOUBLE);
            Spawn().SetParam(Items.CARD_DROP_DOUBLE);
            Spawn().SetParam(Items.CARD_DM_DOUBLE);
            Spawn().SetParam(Items.REHANCE_STONE);
            Spawn().SetParam(Items.REHANCE_SPAR);
            Spawn().SetParam(Items.BOSS_RESET_POWDER);
            Spawn().SetParam(Items.MM_TICKET);
            Spawn().SetParam("dm_resetcard");
            Spawn().SetParam(Items.ENH(1));
            Spawn().SetParam(Items.ENH(2));
            Spawn().SetParam(Items.ENH(3));
            /*
            List<Transform> items = new List<Transform>();
            ShopItemGrid item = Spawn().SetParam(Items.CARD_EXP_DOUBLE);
            items.Add(item.transform);
            item = Spawn().SetParam(Items.CARD_DROP_DOUBLE);
            items.Add(item.transform);
            ItemContainer.ShowContainer(items, () =>
            {
                Rect rect = item.GetComponent<RectTransform>().rect;
                new ItemContainerParam(rect.width, rect.height,4);
            }, () =>
            {
                foreach (var it in items)
                {
                    it.SetParent(transform, false);
                }
            }, "test", "我们正在测试一些东西");*/
        }
    }
    public void RefreshMaterialView(bool toggle)
    {
        if (toggle)
        {
            Despawn();
            Spawn().SetParam("iron_normal");
            Spawn().SetParam("iron_azure");
            Spawn().SetParam("iron_fiery");
            Spawn().SetParam("oil_low");
            Spawn().SetParam("oil_normal");
            Spawn().SetParam("oil_noble");
            Spawn().SetParam("crystal_lasurite");
            Spawn().SetParam("crystal_cuprum");
        }
    }
}