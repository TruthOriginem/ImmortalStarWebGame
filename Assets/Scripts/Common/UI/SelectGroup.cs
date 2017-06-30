using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectGroup : MonoBehaviour
{
    public List<Selectable> selections;
    public bool loop = false;

    List<GameObject> selectsGos = new List<GameObject>();
    void Start()
    {
        for (int i = 0; i < selections.Count; i++)
        {
            selectsGos.Add(selections[i].gameObject);
        }
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (selectsGos.Contains(EventSystem.current.currentSelectedGameObject))
            {
                int index = selectsGos.IndexOf(EventSystem.current.currentSelectedGameObject);
                if (index == selections.Count - 1)
                {
                    if (!loop)
                    {
                        return;
                    }
                    else
                    {
                        selections[0].Select();
                    }
                }
                else
                {
                    selections[index + 1].Select();
                }
            }
        }
    }
}
