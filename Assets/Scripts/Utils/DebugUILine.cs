﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
public class DebugUILine : MonoBehaviour
{
    public bool isOn = true;

    static Vector3[] fourCorners = new Vector3[4];
    void OnDrawGizmos()
    {
        if (isOn)
        {
            foreach (MaskableGraphic g in GameObject.FindObjectsOfType<MaskableGraphic>())
            {
                if (g.raycastTarget)
                {
                    RectTransform rectTransform = g.transform as RectTransform;
                    rectTransform.GetWorldCorners(fourCorners);
                    Gizmos.color = Color.blue;
                    for (int i = 0; i < 4; i++)
                        Gizmos.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);

                }
            }
        }
    }
}
#endif