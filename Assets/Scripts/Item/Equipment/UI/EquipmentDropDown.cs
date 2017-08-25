using InterfaceTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EquipmentDropDown : MonoBehaviour, ICustomDropDown<EquipmentBase>
{
    [SerializeField]
    private Dropdown dropdown;

    public Func<EquipmentBase, string> _GetItemName;
    public Func<IComparer<EquipmentBase>> _GetComparer;
    public Action<EquipmentBase> _OnItemChanged;

    private Dictionary<int, EquipmentBase> indexToItem = new Dictionary<int, EquipmentBase>();
    /// <summary>
    /// 当前选择的index
    /// </summary>
    public int CurrentIndex
    {
        get
        {
            return dropdown.value;
        }

        set
        {
            dropdown.value = value;
        }
    }

    public Dictionary<int, EquipmentBase> IndexToItem
    {
        get
        {
            if (indexToItem == null) indexToItem = new Dictionary<int, EquipmentBase>();
            return indexToItem;
        }
    }

    public EquipmentBase GetCurrentSelectedItem()
    {
        return indexToItem.ContainsKey(CurrentIndex) ? IndexToItem[CurrentIndex] : null;
    }

    private string GetItemName(EquipmentBase t)
    {
        return _GetItemName(t);
    }

    private Sprite GetItemSprite(EquipmentBase t)
    {
        return SpriteLibrary.GetSprite(t.GetIconPath());
    }

    public void OnIndexChanged(int index)
    {
        if (_OnItemChanged != null) _OnItemChanged(GetCurrentSelectedItem());
    }

    public void Init(List<EquipmentBase> items)
    {
        dropdown.ClearOptions();
        IndexToItem.Clear();
        var copyItems = new List<EquipmentBase>(items);
        if (_GetComparer != null) copyItems.Sort(_GetComparer());
        var optionDatas = new List<Dropdown.OptionData>();
        for (int i = 0; i < copyItems.Count; i++)
        {
            var item = copyItems[i];
            var optionData = new Dropdown.OptionData(GetItemName(item), GetItemSprite(item));
            optionDatas.Add(optionData);
            indexToItem.Add(i, item);
        }
        dropdown.AddOptions(optionDatas);
    }

}
