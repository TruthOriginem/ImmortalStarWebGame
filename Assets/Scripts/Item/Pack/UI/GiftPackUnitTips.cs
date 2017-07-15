using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GiftPackUnitTips : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GiftPackUnit linkedUnit;
    public static Action<GiftPackUnit> onPointerEnter;
    public static Action onPointerExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null)
        {
            onPointerEnter(linkedUnit);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       onPointerExit();
    }
}
