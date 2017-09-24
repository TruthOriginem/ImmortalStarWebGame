using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class ShopItemGridImage : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler{

    public static Action<ShopItemGrid> OnMouseEnter;
    public static Action OnMouseExit;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OnMouseEnter != null) OnMouseEnter(transform.GetComponentInParent<ShopItemGrid>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnMouseExit != null) OnMouseExit();
    }
}
