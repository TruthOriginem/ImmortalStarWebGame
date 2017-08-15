using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InterfaceTools;
using PathologicalGames;
using System;

public class StorageManager : MonoBehaviour
{
    public Transform targetPool;
    public SpawnPool spawnPool;
    public Transform prefabItem;
    public static StorageManager Instance { get; set; }
    public void Spawn(EquipmentBase equip)
    {
        var item = spawnPool.Spawn(prefabItem, targetPool).GetComponent<EquipmentInReUI>();
        item.SetInfo(equip);
    }
    void Awake()
    {
        Instance = this;
    }
    public void InitOrRefresh()
    {
        StartCoroutine(_InitOrRefresh());
    }
    IEnumerator _InitOrRefresh()
    {
        spawnPool.DespawnAll();
        yield return PlayerRequestBundle.RequestUpdateItemsInPack();
        List<EquipmentBase> eqInRe = PlayerInfoInGame.GetAllEquipments(false, true);
        eqInRe.Sort(new EquipmentLevelSorter());
        for (int i = 0; i < eqInRe.Count; i++)
        {
            Spawn(eqInRe[i]);
        }
    }
    public void MoveIt(EquipmentBase equip)
    {
        if (PlayerInfoInGame.GetEquipmentAmount() < PlayerInfoInGame.MAX_EQUIPMENT_IN_PACK)
        {
            StartCoroutine(_MoveIt(equip));
        }
        else
        {
            MessageBox.Show("您的背包装备数量已满，不能取出了。", "温馨提示");
        }
    }
    IEnumerator _MoveIt(EquipmentBase equip)
    {
        CU.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("itemId", equip.item_id);
        form.AddField("index", 0);
        form.AddField("type", 2);
        form.AddField("equipped", 0);
        form.AddField("isInStorage", 0);
        WWW w = new WWW(CU.ParsePath(PlayerRequestBundle.UPDATE_ITEM_INDEX_FILEPATH), form);
        yield return w;
        CU.HideConnectingUI();
        if (!CU.IsPostSucceed(w))
        {
            CU.ShowConnectFailed();
            yield break;
        }
        yield return _InitOrRefresh();
    }
}
