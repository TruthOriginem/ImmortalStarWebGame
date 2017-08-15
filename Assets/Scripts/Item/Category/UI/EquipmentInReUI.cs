using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InterfaceTools;
using System;

public class EquipmentInReUI : MonoBehaviour, ISpawnPoolItem
{
    public Text eqName;
    public Image eqIcon;
    private EquipmentBase equip;

    public void SetInfo(EquipmentBase equipment)
    {
        eqIcon.sprite = SpriteLibrary.GetSprite(equipment.GetIconPath());
        eqName.text = string.Format("{0}\n{1}", equipment.GetModifyiedName(), equipment.GetRehanceString());
        equip = equipment;
    }
    public void MoveIt()
    {
        StorageManager.Instance.MoveIt(equip);
    }
    public void OnSpawned()
    {
    }

    public void OnDespawned()
    {
        eqIcon.sprite = null;
        equip = null;
    }
}
