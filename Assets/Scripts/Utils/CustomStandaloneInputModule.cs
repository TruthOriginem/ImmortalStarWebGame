using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CustomStandaloneInputModule : StandaloneInputModule
{
    public static PointerEventData GetPointerEventData(int pointerId = -1)
    {
        PointerEventData eventData;
        _instance.GetPointerData(pointerId, out eventData, true);
        return eventData;
    }

    private static CustomStandaloneInputModule _instance;

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }
}
