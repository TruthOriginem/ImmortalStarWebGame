using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HideByCtrl : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public bool isHided;
    private void Start()
    {
        canvasGroup.alpha = 1;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (isHided)
        {
            canvasGroup.alpha = 0;
        }
    }
#endif
}
