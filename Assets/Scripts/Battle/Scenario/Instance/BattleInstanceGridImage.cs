using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

/// <summary>
/// 用于InstanceTooltip，提示里面的怪物与掉落列表
/// </summary>
public class BattleInstanceGridImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BattleInstanceGrid instanceGrid;
    public static Action<BattleInstanceGrid> onIconEnter;
    public static Action onIconExit;

    void Start()
    {

    }

    void Update()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onIconEnter != null)
        {
            onIconEnter(instanceGrid);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onIconExit != null)
        {
            onIconExit();
        }
    }
}
