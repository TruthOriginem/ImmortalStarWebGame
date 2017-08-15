using UnityEngine;
using System.Collections;
using SerializedClassForJson;
using UnityEngine.SceneManagement;

/// <summary>
/// 下载一些专用数据的。
/// </summary>
public class ModDataManager : MonoBehaviour
{
    private const string EQUIP_MOD_LOAD_PATH = "scripts/modules/loadEquipMods.php";
    public static bool StartInit = false;

    public static IEnumerator InitEquipmentFactory()
    {
        CU.ShowConnectingUI();
        WWW w = new WWW(CU.ParsePath(EQUIP_MOD_LOAD_PATH));
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            //Debug.Log(w.text);
            EquipmentFactory.InitFactory(JsonUtility.FromJson<TempEquipModCollection>(w.text));
        }
        else
        {
            CU.ShowConnectFailed();
        }
        CU.HideConnectingUI();
    }

}
