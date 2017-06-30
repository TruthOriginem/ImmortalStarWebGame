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


    void Start()
    {
#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name != "login")
        {
            //StartInit = true;
        }
#endif
        if (!EquipmentFactory.IsFactoryInit && StartInit)
        {
            StartCoroutine(InitEquipmentFactory());
        }
    }
    public static IEnumerator InitEquipmentFactory()
    {
        ConnectUtils.ShowConnectingUI();
        WWW w = new WWW(ConnectUtils.ParsePath(EQUIP_MOD_LOAD_PATH));
        yield return w;
        if (w.isDone && w.text != "failed")
        {
            //Debug.Log(w.text);
            EquipmentFactory.InitFactory(JsonUtility.FromJson<TempEquipModCollection>(w.text));
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
        }
        ConnectUtils.HideConnectingUI();
    }


    void Update()
    {

    }
}
