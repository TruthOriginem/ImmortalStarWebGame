using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OCManager : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public Toggle initToggle;
    public static OCManager Instance { get; set; }

    void Awake()
    {
        Instance = this;
    }
    public static void Refresh()
    {
        Instance.initToggle.isOn = true;
    }
}
