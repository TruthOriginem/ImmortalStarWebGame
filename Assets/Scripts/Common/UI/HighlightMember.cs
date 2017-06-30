using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class HighlightMember : MonoBehaviour
{
    public HighlightGroup group;

    void Start()
    {
        if (group != null)
        {
            group.AddMember(this);
        }
    }
}
