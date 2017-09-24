using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson;
using System.Text;
using GameId;

/// <summary>
/// 依附于PlayerInfoInGame
/// <para>主要功能是对数据库提出请求之类的</para>
/// </summary>
public class PlayerRequestBundle : MonoBehaviour
{
    /// <summary>
    /// 用于记录当前的player_record的
    /// </summary>
    public static System.Object record;
    public PlayerInfoInGame pii;
    public const string UPDATE_ITEM_INDEX_FILEPATH = "scripts/player/item/updateIndexInPack.php";
    private const string UPDATE_ITEMS_INDEX_FILEPATH = "scripts/player/item/updateIndexsInPack.php";
    private const string UPDATE_ITEMS_FILEPATH = "scripts/player/item/loaditems.php";
    public const string UPDATE_UNIVERSAL_FILEPATH = "scripts/player/universalUpdate.php";
    private const string GET_LIG_FILEPATH = "scripts/player/instance/getLastInstanceGrid.php";
    private const string GET_SKILLS_FILEPATH = "scripts/player/skill/getSkillDatas.php";
    private const string GIVE_SUGGESTION = "scripts/player/system/giveSuggestion.php";
    public static PlayerRequestBundle Instance { get; set; }

    void Awake()
    {
        Instance = this;
    }


    /// <summary>
    /// 上传道具/武器的背包序号
    /// </summary>
    public static void UpdateItemsIndex(ItemBase item)
    {
        if (item is EquipmentBase)
        {
            Instance.StartCoroutine(Instance.UpdateItemIndex(item.item_id, item.indexInPack, 1, ((EquipmentBase)item).IsEquipped));
        }
        else
        {
            Instance.StartCoroutine(Instance.UpdateItemIndex(item.item_id, item.indexInPack, 0, false));
        }
    }
    public static Coroutine MakeEquipToStorage(EquipmentBase equip)
    {
        return Instance.StartCoroutine(Instance.UpdateItemIndex(equip.item_id, 0, 2, false, true));
    }

    /// <summary>
    /// 请求数据库更新背包，会连带装备、道具数量一同更新(m_items)，返回一个协程。
    /// </summary>
    /// <returns></returns>
    public static Coroutine RequestUpdateItemsInPack()
    {
        return Instance.StartCoroutine(Instance.UpdateItemsInPack());
    }
    /// <summary>
    /// 请求更新技能信息，返回一个协程
    /// </summary>
    /// <returns></returns>
    public static Coroutine RequestGetSkillDatas()
    {
        return Instance.StartCoroutine(Instance.RequestGetSkillDataCor());
    }
    /// <summary>
    /// 请求数据库更新技能数据
    /// </summary>
    /// <param name="skillsToChange"></param>
    /// <returns></returns>
    public static Coroutine RequestUpdateSkills(int skillsToChange)
    {
        return Instance.StartCoroutine(Instance.UpdateSkillData(SkillDataManager.GenerateTempSkills(), skillsToChange));
    }

    /// <summary>
    /// 请求数据库更新装备，道具的顺序
    /// </summary>
    /// <returns></returns>
    public static Coroutine RequestUpdateIndexInPack(Dictionary<ItemBase, int> idToIndexs)
    {
        return Instance.StartCoroutine(Instance.UpdateItemsIndexInPack(idToIndexs));
    }
    public static Coroutine RequestUpdateIIA(IIABinds binds, TempPlayerAttribute attr = null)
    {
        return Instance.StartCoroutine(Instance.RequestIIABinds(binds, attr));
    }

    /// <summary>
    /// 请求上传与Player_record表相关的数据,这将会导致一次玩家信息刷新
    /// </summary>
    /// <param name="record">相应记录</param>
    /// <param name="iia">道具数量组合</param>
    /// <param name="attr">玩家相对属性</param>
    /// <param name="randEqReqs">武器生成请求数组</param>
    /// <returns></returns>
    public static Coroutine RequestUpdateRecord<T>(T record = default(T), IIABinds iia = null, TempPlayerAttribute attr = null, TempRandEquipRequest[] randEqReqs = null)
    {
        return Instance.StartCoroutine(Instance.RequestUpdateRecordCor(record, iia, attr, randEqReqs));
    }

    /// <summary>
    /// 请求与Player_record表相关的数据
    /// </summary>
    /// <returns></returns>
    public static Coroutine RequestGetRecord<T>() where T : class
    {

        return Instance.StartCoroutine(Instance.RequestGetRecordCor<T>());
    }
    /// <summary>
    /// 请求装备指定id的称号。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Coroutine RequestEquipDesignation(int id)
    {
        return Instance.StartCoroutine(Instance.ChangeDesignation(id, 0));
    }
    /// <summary>
    /// 请求获得指定id的称号。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Coroutine RequestAddDesignation(int id)
    {
        return Instance.StartCoroutine(Instance.ChangeDesignation(id, 1));
    }
    /// <summary>
    /// 在外部AppendRequest后再进行同步。完成后默认更新玩家信息。如果没有需要更新的信息，那么直接更新玩家信息。
    /// </summary>
    /// <returns></returns>
    public static Coroutine RequestSyncUpdate(bool updatePlayerInfo = true)
    {
        return Instance.StartCoroutine(Instance.RequestSyncUpdateCor(updatePlayerInfo));
    }
    const string DESIGNATION_CHANGE_PATH = "scripts/player/designation/updateDesignationData.php";
    /// <summary>
    /// 关于装备/新增称号的迭代器
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IEnumerator ChangeDesignation(int id, int type)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", PlayerInfoInGame.Id);
        form.AddField("design_id", id);
        form.AddField("type", type);
        WWW w = new WWW(CU.ParsePath(DESIGNATION_CHANGE_PATH), form);
        CU.ShowConnectingUI();
        yield return w;
        CU.HideConnectingUI();
        if (!CU.IsPostSucceed(w))
        {
            CU.ShowConnectFailed();
        }
        else
        {
            if (type == 1)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("恭喜您获得称号：");
                sb.Append(DesignationManager.GetDesignationName(id));
                ConTipsManager.AddMessage(sb.ToString(), DesignationManager.GetDesignationData(id).description);
            }
        }
    }
    /// <summary>
    /// 更新背包道具的协程，会一同更新武器和道具
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateItemsInPack()
    {
        CU.ShowConnectingUI();
        yield return ChipManager.RequestGetAllChipsData();
        ///刷新武器
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("type", 1);
        WWW w = new WWW(CU.ParsePath(UPDATE_ITEMS_FILEPATH), form);
        yield return w;
        //清空M_items
        pii.ClearAllItems();
        if (w.isDone && w.text != null)
        {
            //这个Json数组是由数据库里所有该id的武器通过php集合生成的
            TempEquipment[] tempEquips = JsonHelper.GetJsonArray<TempEquipment>(w.text);

            if (tempEquips != null && tempEquips.Length != 0)
            {
                Dictionary<string, EquipmentBase> idToEq = new Dictionary<string, EquipmentBase>();
                foreach (TempEquipment temp in tempEquips)
                {
                    var eq = EquipmentFactory.CreateEquipment(temp);
                    PlayerInfoInGame.CurrentItems.Add(eq);
                    idToEq.Add(eq.item_id, eq);
                }
                foreach (var data in PlayerInfoInGame.CurrentChips)
                {
                    var dataId = data.GetEquippedId().ToString();
                    if (idToEq.ContainsKey(dataId))
                    {
                        idToEq[dataId].AddChip(data);
                    }
                }
            }
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
        //刷新道具
        yield return ItemDataManager.RequestGetItemsAmount();

        ItemDataManager.AddItemsToPlayerInfo();
        CU.HideConnectingUI();
    }
    /// <summary>
    /// 批量排序相对应的协程。
    /// </summary>
    /// <param name="itemToKeys"></param>
    /// <returns></returns>
    public IEnumerator UpdateItemsIndexInPack(Dictionary<ItemBase, int> itemToKeys)
    {
        CU.ShowConnectingUI();
        TempItemIndex indexs = TempItemIndex.Create(itemToKeys); ;
        string json = JsonUtility.ToJson(indexs);
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("data", json);
        WWW w = new WWW(CU.ParsePath(UPDATE_ITEMS_INDEX_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != "failed")
        {
        }
        else
        {
            CU.ShowConnectFailed();
        }
        CU.HideConnectingUI();
    }
    IEnumerator RequestIIABinds(IIABinds binds, TempPlayerAttribute attr)
    {
        yield return RequestUpdateRecordCor<Object>(null, binds, attr, null);
    }
    /// <summary>
    /// 上传相应记录的协程，也可对道具,武器，玩家状态进行修改
    /// </summary>
    IEnumerator RequestUpdateRecordCor<T>(T record, IIABinds iia, TempPlayerAttribute attr, TempRandEquipRequest[] requests)
    {
        CU.ShowConnectingUI();
        SyncRequest.AppendRequest(Requests.RECORD_DATA, record);
        SyncRequest.AppendRequest(Requests.PLAYER_DATA, attr);
        SyncRequest.AppendRequest(Requests.ITEM_DATA, iia != null ? iia.ToJson(false) : null);
        SyncRequest.AppendRequest(Requests.EQ_TO_DELETE_DATA, iia != null ? iia.ToJson(true) : null);
        SyncRequest.AppendRequest(Requests.RND_EQ_GENA_DATA, TempRandEquipRequest.GenerateJsonArray(requests));
        WWW w = SyncRequest.CreateSyncWWW();
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        }
        else
        {
            Debug.LogWarning(w.text);
            CU.ShowConnectFailed();
        }
        CU.HideConnectingUI();
    }
    IEnumerator RequestSyncUpdateCor(bool updatePlayerInfo)
    {
        WWW w = SyncRequest.CreateSyncWWW();
        if (w == null)
        {
            if (updatePlayerInfo) yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        }
        else
        {
            CU.ShowConnectingUI();
            yield return w;
            CU.HideConnectingUI();
            if (CU.IsPostSucceed(w))
            {
                //Debug.Log(w.text);
                if (updatePlayerInfo) yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
            }
            else
            {
                Debug.LogError(w.text);
                CU.ShowConnectFailed();
            }
        }
    }
    /// <summary>
    /// 向数据库请求记录，并为指定类赋值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <returns></returns>
    IEnumerator RequestGetRecordCor<T>() where T : class
    {
        CU.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        WWW w = new WWW(CU.ParsePath(GET_LIG_FILEPATH), form);
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            string json = w.text;
            record = JsonUtility.FromJson<T>(json);
        }
        else
        {
            record = null;
            CU.ShowConnectFailed();
        }
        // Debug.Log(((target as TempLigRecord).lig_id));
        CU.HideConnectingUI();
    }

    /// <summary>
    /// 获取当前技能信息的协程
    /// </summary>
    /// <returns></returns>
    IEnumerator RequestGetSkillDataCor()
    {
        CU.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        WWW w = new WWW(CU.ParsePath(GET_SKILLS_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != "failed")
        {
            TempSkills skills = JsonUtility.FromJson<TempSkills>(w.text);
            SkillDataManager.SetSkillsByTempSkills(skills);
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
        CU.HideConnectingUI();
    }
    /// <summary>
    /// 令数据库更新装备、道具等位置。
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="index"></param>
    /// <param name="type">0为道具，1为装备</param>
    /// <param name="isEquipped">如果是装备，则上传是否装备的信息。</param>
    /// <returns></returns>
    IEnumerator UpdateItemIndex(string itemId, int index, int type, bool isEquipped, bool inStorage = false)
    {
        CU.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("itemId", itemId);
        form.AddField("index", index);
        form.AddField("type", type);
        form.AddField("equipped", isEquipped ? 1 : 0);
        form.AddField("isInStorage", type == 2 ? (inStorage ? 1 : 0) : 0);
        WWW w = new WWW(CU.ParsePath(UPDATE_ITEM_INDEX_FILEPATH), form);
        yield return w;
        CU.HideConnectingUI();
    }
    /// <summary>
    /// 令数据库更新技能数据
    /// </summary>
    /// <param name="skills"></param>
    /// <param name="skillPointChange"></param>
    /// <returns></returns>
    IEnumerator UpdateSkillData(TempSkills skills, int skillPointChange)
    {
        CU.ShowConnectingUI();
        SyncRequest.AppendRequest("skillData", skills);
        TempPlayerAttribute attr = new TempPlayerAttribute();
        attr.skillPoint = skillPointChange;
        SyncRequest.AppendRequest("playerData", skillPointChange == 0 ? null : attr);
        WWW w = SyncRequest.CreateSyncWWW();
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
            SkillReadingPart.Instance.RefreshTargetSkill();
        }
        else
        {
            CU.ShowConnectFailed();
        }
        CU.HideConnectingUI();
    }


    public void GiveSuggestion()
    {
        InputStringBox.Show("请输入您的宝贵意见", "提示", (result, text) =>
        {
            if (result == DialogResult.OK)
            {
                if (text.Length >= 5)
                {
                    StartCoroutine(_GiveSuggestion(text));
                }
                else
                {
                    MessageBox.Show("请不要低于5个字！", "提示");
                }

            }
        }, MessageBoxButtons.OKCancel);
    }
    IEnumerator _GiveSuggestion(string text)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", PlayerInfoInGame.Id);
        form.AddField("content", text);
        WWW w = new WWW(CU.ParsePath(GIVE_SUGGESTION), form);
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            MessageBox.Show("感谢您的宝贵意见！", "谢谢");
        }
        else
        {
            CU.ShowConnectFailed();
        }
    }
}
/// <summary>
/// 用于与服务器端同步更新的请求类。
/// </summary>
public class SyncRequest
{
    static readonly List<SyncRequest> requestToUpload = new List<SyncRequest>();
    string requestString;
    string requestId;
    public SyncRequest(string id, string content)
    {
        requestString = content;
        requestId = id;
    }
    /// <summary>
    /// 为下一次同步提交请求消息。
    /// </summary>
    /// <param name="id">请求id</param>
    /// <param name="content">php收到的数据流/字符串，若为空则不会发送任何东西</param>
    public static void AppendRequest(string id, string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            requestToUpload.Add(new SyncRequest(id, content));
        }
    }
    public static void AppendRequest<T>(string id, T jsonObject)
    {
        if (jsonObject != null)
        {
            AppendRequest(id, JsonUtility.ToJson(jsonObject));
        }
    }
    /// <summary>
    /// 集合当前所有创建的请求，合并为一个WWWForm.(发送给UniveralUpdate.php)
    /// </summary>
    /// <returns></returns>
    public static WWWForm CompleteRequestForm()
    {
        if (requestToUpload.Count != 0)
        {
            WWWForm form = new WWWForm();
            for (int i = 0; i < requestToUpload.Count; i++)
            {
                var request = requestToUpload[i];
                form.AddField(request.requestId, request.requestString);
            }
            requestToUpload.Clear();
            form.AddField(Requests.ID, PlayerInfoInGame.Id);
            form.AddField(Requests.ECKEY, GlobalSettings.GetEncryptKey());
            return form;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 通过SyncRequest集合创建WWW通讯类，会合并被清空所有待同步的SyncRequest。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static WWW CreateSyncWWW(string path = PlayerRequestBundle.UPDATE_UNIVERSAL_FILEPATH)
    {
        var form = CompleteRequestForm();
        return form == null ? null : new WWW(CU.ParsePath(path), form);
    }

}