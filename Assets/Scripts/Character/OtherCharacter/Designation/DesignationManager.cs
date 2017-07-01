using GameId;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 管理称号的类。
/// </summary>
public class DesignationManager : MonoBehaviour
{
    public const string DATA_PATH = "scripts/player/designation/getDesignationDatas.php";
    public static DesignationManager Instance { get; set; }
    /// <summary>
    /// 需要获得的成就们，在每次刷新人物属性的时候刷新
    /// </summary>
    public static List<int> DesignToGets = new List<int>();
    private static Dictionary<int, DesignationData> designDatas = new Dictionary<int, DesignationData>();

    public Dropdown designDropdown;
    public Text designDescription;
    public Button applyButton;
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {

    }
    public void ToggleThis(bool toggle)
    {
        if (toggle)
        {
            InitOrRefresh();
        }
    }
    /// <summary>
    /// 通过玩家当前拥有称号，重新载入称号信息，
    /// </summary>
    void InitOrRefresh()
    {
        designDropdown.ClearOptions();
        if (PlayerInfoInGame.Design_Ids.Count == 0)
        {
            return;
        }
        int index = 0;
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        for (int i = 0; i < PlayerInfoInGame.Design_Ids.Count; i++)
        {
            int id = PlayerInfoInGame.Design_Ids[i];
            if (id == PlayerInfoInGame.Design_NowEquipped)
            {
                index = i;
            }
            var optionData = new Dropdown.OptionData(GetDesignationName(id));
            optionDatas.Add(optionData);
        }
        designDropdown.AddOptions(optionDatas);
        designDropdown.value = index;
        ChangeDesignIndex(index);
    }
    public void ChangeDesignIndex(int index)
    {
        var id = PlayerInfoInGame.Design_Ids[index];
        var data = GetDesignationData(id);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(data.description);
        sb.AppendLine();
        sb.Append(data.GetPlusDescription());
        designDescription.text = sb.ToString();
        applyButton.interactable = PlayerInfoInGame.Design_NowEquipped != id;
    }
    /// <summary>
    /// 按下应用按钮
    /// </summary>
    public void ApplyDesignChange()
    {
        var id = PlayerInfoInGame.Design_Ids[designDropdown.value];
        StartCoroutine(ApplyCor(id));
    }
    IEnumerator ApplyCor(int id)
    {
        yield return PlayerRequestBundle.RequestEquipDesignation(id);
        yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        InitOrRefresh();
        applyButton.interactable = PlayerInfoInGame.Design_NowEquipped != id;
    }
    #region 初始化称号信息
    /// <summary>
    /// 初始化称号属性的协程。
    /// </summary>
    /// <returns></returns>
    IEnumerator InitDesignationDatasCor()
    {
        WWW w = new WWW(ConnectUtils.ParsePath(DATA_PATH));
        ConnectUtils.ShowConnectingUI();
        yield return w;
        ConnectUtils.HideConnectingUI();
        if (ConnectUtils.IsPostSucceed(w))
        {
            designDatas.Clear();
            DesignationData[] data = JsonHelper.GetJsonArray<DesignationData>(w.text);
            for (int i = 0; i < data.Length; i++)
            {
                designDatas.Add(data[i].id, data[i]);
                // Debug.Log(data[i].name);
            }
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
            yield break;
        }
    }
    /// <summary>
    /// 返回初始化协程。
    /// </summary>
    /// <returns></returns>
    public static Coroutine InitDesignationDatas()
    {
        return Instance.StartCoroutine(Instance.InitDesignationDatasCor());
    }
    /// <summary>
    /// 解析从服务器获得的称号信息字符串，并得到当前使用的称号和拥有称号的集合。
    /// </summary>
    /// <param name="designData"></param>
    /// <param name="equippedKey"></param>
    /// <param name="ownedKeys"></param>
    public static void ParseDesignData(string designData, out int equippedKey, out int[] ownedKeys)
    {
        if (!string.IsNullOrEmpty(designData))
        {
            bool succeed = true;
            string[] splits = designData.Split('|');
            succeed &= int.TryParse(splits[0], out equippedKey);
            string[] ownedSplits = splits[1].Split(',');
            ownedKeys = new int[ownedSplits.Length];
            for (int i = 0; i < ownedSplits.Length; i++)
            {
                succeed &= int.TryParse(ownedSplits[i], out ownedKeys[i]);
            }
            if (!succeed)
            {
                equippedKey = Designations.NOTHING;
                ownedKeys = new int[0];
            }
        }
        else
        {
            equippedKey = Designations.NOTHING;
            ownedKeys = new int[0];
        }
    }
    #endregion
    /// <summary>
    /// 检查玩家当前是否拥有某称号
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool CheckDesignationOwned(int id)
    {
        return PlayerInfoInGame.Design_Ids.Contains(id);
    }
    /// <summary>
    /// 如果没有指定id，则在下次更新里加入指定成就。
    /// </summary>
    /// <param name="id"></param>
    public static void CheckAndTryGetDesign(int id)
    {
        if (!CheckDesignationOwned(id))
        {
            DesignToGets.Add(id);
        }
    }
    /// <summary>
    /// 获得指定ID称号的属性。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static DesignationData GetDesignationData(int id)
    {
        return designDatas.ContainsKey(id) ? designDatas[id] : null;
    }
    /// <summary>
    /// 得到称号的名字。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetDesignationName(int id)
    {
        var data = GetDesignationData(id);
        return data != null ? data.name : "无";
    }
    /// <summary>
    /// 检查所有和“完成关卡”有关的成就获取
    /// </summary>
    /// <param name="completedGrids"></param>
    /// <param name="toGetDesignIds">本次操作可以获得的成就列表</param>
    public static void CheckGridsCompleteDesign(List<BatInsGridData> completedGrids)
    {
        int checkBossAmount = 0;
        for (int i = 0; i < completedGrids.Count; i++)
        {
            var grid = completedGrids[i];
            if (grid.isBoss)
            {
                checkBossAmount++;
            }
        }
        //击败的boss数量所获得的称号
        if (checkBossAmount > 0)
        {
            //打过一个boss就会获得初心者称号
            if (!CheckDesignationOwned(Designations.CXZ))
            {
                DesignToGets.Add(Designations.CXZ);
            }
            if (checkBossAmount >= 4)
            {
                if (!CheckDesignationOwned(Designations.CJZL))
                {
                    DesignToGets.Add(Designations.CJZL);
                }
            }
        }
    }
    /// <summary>
    /// 对同一个关卡攻击超过一定次数有关的成就获取
    /// </summary>
    /// <param name="amount"></param>
    public static void CheckMaxAttackAmountDesign(float amount)
    {
        if (amount == 0)
        {
            return;
        }
        if (amount >= 15)
        {
            CheckAndTryGetDesign(Designations.GWZR);
        }
    }
}
/// <summary>
/// 称号信息
/// </summary>
[System.Serializable]
public class DesignationData : BaseModification
{

}
