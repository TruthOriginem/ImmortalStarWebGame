using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRankInfo : MonoBehaviour {
    public Text arg1;
    public Text arg2;
    public Text arg3;
    public void SetInfo(TempPlayerRankInfo info)
    {
        arg1.text = info.arg1;
        arg2.text = info.arg2;
        arg3.text = info.arg3;
    }
}
