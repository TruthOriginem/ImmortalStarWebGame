using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Collections.Generic;

public class HighlightGroup : MonoBehaviour, IPointerEnterHandler
{
    private List<CanvasGroup> groupMembers = new List<CanvasGroup>();
    private List<GameObject> goes = new List<GameObject>();

    public bool isShowing = false;
    private static HighlightGroup lastGroup;
    void Start()
    {
    }

    void Update()
    {
        if (isShowing)
        {
            PointerEventData data = CustomStandaloneInputModule.GetPointerEventData();
            foreach (var item in data.hovered)
            {
                if (goes.Contains(item) || item == gameObject)
                {
                    return;
                }
            }
            HideAllMember();
            isShowing = false;
        }
    }
    public void AddMember(HighlightMember member)
    {
        CanvasGroup go = member.GetComponent<CanvasGroup>();
        groupMembers.Add(go);
        goes.Add(member.gameObject);
        go.interactable = false;
        go.blocksRaycasts = false;
        go.alpha = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isShowing)
        {
            isShowing = true;
            if (lastGroup != null && lastGroup != this)
            {
                lastGroup.isShowing = false;
                lastGroup.HideAllMember();
            }
            lastGroup = this;
            StartCoroutine(Show(0.25f));
        }
    }
    IEnumerator Show(float time)
    {
        float dealTime = time;
        while (dealTime > 0)
        {
            float alpha = 1 - dealTime / time;
            foreach (CanvasGroup go in groupMembers)
            {
                go.interactable = true;
                go.blocksRaycasts = true;
                go.alpha = alpha;
            }
            dealTime -= Time.deltaTime;
            if (isShowing)
            {
                yield return 0;
            }
            else
            {
                HideAllMember();
                break;
            }

        }

    }
    void HideAllMember()
    {
        foreach (CanvasGroup go in groupMembers)
        {
            go.interactable = false;
            go.blocksRaycasts = false;
            go.alpha = 0;
        }
    }

}
