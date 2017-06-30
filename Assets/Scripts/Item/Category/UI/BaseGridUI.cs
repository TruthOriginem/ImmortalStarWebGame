using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class BaseGridUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,IPointerClickHandler
{
    [SerializeField]
    private bool isPackGrid;//是不是背包（如果是背包则允许所有武器与道具）
    [SerializeField]
    private EQ_TYPE equipmentType;//这个格子允许的武器

    /// <summary>
    /// 得到这个格子允许存放的武器类型
    /// </summary>
    /// <returns></returns>
    public EQ_TYPE GetPermittedType()
    {
        return equipmentType;
    }

    public static Action<Transform> onTipPointerEnter;
    public static Action onTipPointerExit;

    public static Action<Transform> onGridRightClick;
    public static Action<Transform> onGridLeftClick;

    public static Action<Transform> onGridBeginDrag;
    public static Action<Transform,Transform> onGridEndDrag;
    public static Action<string> onGridDraging;


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (onGridBeginDrag != null)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                onGridBeginDrag(transform);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(onGridEndDrag != null)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if(eventData.pointerEnter == null)
                {
                    onGridEndDrag(transform,null);
                }
                else
                {
                    onGridEndDrag(transform,eventData.pointerEnter.transform);
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onTipPointerEnter != null)
        {
            onTipPointerEnter(eventData.pointerEnter.transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onTipPointerExit != null)
        {
            onTipPointerExit();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(onGridRightClick != null)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                //Debug.Log(eventData.pointerEnter.transform.name);
                onGridRightClick(transform);
            }else if(eventData.button == PointerEventData.InputButton.Left)
            {
                onGridLeftClick(transform);
            }
        }
    }
}
