using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChipUIItem : MonoBehaviour
{
    public Image chip_Icon;
    public Text name_Text;
    public Toggle toggle;
    public Action<ChipUIItem> onToggled;

    private ChipData linkedData;

    public void SetLinkedData(ChipData data, Sprite icon)
    {
        linkedData = data;
        name_Text.text = data.GetFullName(true);
        chip_Icon.sprite = icon;
    }

    public ChipData GetLinkedData()
    {
        return linkedData;
    }
    public void SetToggle(bool on)
    {
        toggle.isOn = on;
    }
    public void OnToggled(bool isOn)
    {
        if (onToggled == null)
        {
            return;
        }
        if (isOn)
        {
            onToggled(this);
        }else
        {
            onToggled(null);
        }
    }
    void OnDespawned()
    {
        onToggled = null;
        toggle.isOn = false;
    }
}
