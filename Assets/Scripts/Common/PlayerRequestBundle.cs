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
    private const string UPDATE_ITEM_INDEX_FILEPATH = "scripts/player/item/updateIndexInPack.php";
    private const string UPDATE_ITEMS_INDEX_FILEPATH = "scripts/player/item/updateIndexsInPack.php";
    private const string UPDATE_ITEMS_FILEPATH = "scripts/player/item/loaditems.php";
    public const string UPDATE_UNIVERSAL_FILEPATH = "scripts/player/universalUpdate.php";
    private const string GET_LIG_FILEPATH = "scripts/player/instance/getLastInstanceGrid.php";
    private const string GET_SKILLS_FILEPATH = "scripts/player/skill/getSkillDatas.php";
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
            Instance.StartCoroutine(Instance.UpdateItemIndex(item.item_id, item.indexInPack, 1, ((EquipmentBase)item).IsEquipped()));
        }
        else
        {
            Instance.StartCoroutine(Instance.UpdateItemIndex(item.item_id, item.indexInPack, 0, false));
        }
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
        WWW w = new WWW(ConnectUtils.ParsePath(DESIGNATION_CHANGE_PATH), form);
        ConnectUtils.ShowConnectingUI();
        yield return w;
        ConnectUtils.HideConnectingUI();
        if (!ConnectUtils.IsPostSucceed(w))
        {
            ConnectUtils.ShowConnectFailed();
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
        ConnectUtils.ShowConnectingUI();

        ///刷新武器
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("type", 1);
        WWW w = new WWW(ConnectUtils.ParsePath(UPDATE_ITEMS_FILEPATH), form);
        yield return w;
        //清空M_items
        pii.ClearAllItems();
        if (w.isDone && w.text != null)
        {
            //这个Json数组是由数据库里所有该id的武器通过php集合生成的
            TempEquipment[] tempEquips = JsonHelper.GetJsonArray<TempEquipment>(w.text);

            if (tempEquips != null && tempEquips.Length != 0)
            {

                foreach (TempEquipment temp in tempEquips)
                {
                    PlayerInfoInGame.Now_Items.Add(EquipmentFactory.CreateEquipment(temp));
                }
            }
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
            yield break;
        }
        //刷新道具
        yield return ItemDataManager.GetItemsAmount();

        ItemDataManager.AddItemsToPlayerInfo();
        ConnectUtils.HideConnectingUI();
    }
    /// <summary>
    /// 批量排序相对应的协程。
    /// </summary>
    /// <param name="itemToKeys"></param>
    /// <returns></returns>
    public IEnumerator UpdateItemsIndexInPack(Dictionary<ItemBase, int> itemToKeys)
    {
        ConnectUtils.ShowConnectingUI();
        TempItemIndex indexs = TempItemIndex.Create(itemToKeys); ;
        string json = JsonUtility.ToJson(indexs);
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("data", json);
        WWW w = new WWW(ConnectUtils.ParsePath(UPDATE_ITEMS_INDEX_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != "failed")
        {
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
        }
        ConnectUtils.HideConnectingUI();
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
        ConnectUtils.ShowConnectingUI();
        SyncRequest.AppendRequest("recordData", record == null ? "" : JsonUtility.ToJson(record));
        SyncRequest.AppendRequest("playerData", attr == null ? "" : JsonUtility.ToJson(attr));
        SyncRequest.AppendRequest("itemData", iia == null ? "" : iia.GenerateJsonString(false));
        SyncRequest.AppendRequest("deleteEqData", iia == null ? "" : iia.GenerateJsonString(true));
        SyncRequest.AppendRequest("rndEquipData", requests == null ? "" : TempRandEquipRequest.GenerateJsonArray(requests));
        WWW w = SyncRequest.CreateSyncWWW();
        yield return w;
        if (ConnectUtils.IsPostSucceed(w))
        {
            yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        }
        else
        {
            Debug.LogWarning(w.text);
            ConnectUtils.ShowConnectFailed();
        }
        ConnectUtils.HideConnectingUI();
    }

    /// <summary>
    /// 向数据库请求记录，并为指定类赋值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <returns></returns>
    IEnumerator RequestGetRecordCor<T>() where T : class
    {
        ConnectUtils.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        WWW w = new WWW(ConnectUtils.ParsePath(GET_LIG_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != "failed")
        {
            string json = w.text;
            record = JsonUtility.FromJson<T>(json);
        }
        else
        {
            record = null;
            ConnectUtils.ShowConnectFailed();
        }
        // Debug.Log(((target as TempLigRecord).lig_id));
        ConnectUtils.HideConnectingUI();
    }

    /// <summary>
    /// 获取当前技能信息的协程
    /// </summary>
    /// <returns></returns>
    IEnumerator RequestGetSkillDataCor()
    {
        ConnectUtils.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        WWW w = new WWW(ConnectUtils.ParsePath(GET_SKILLS_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != "failed")
        {
            TempSkills skills = JsonUtility.FromJson<TempSkills>(w.text);
            SkillDataManager.SetSkillsByTempSkills(skills);
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
            yield break;
        }
        ConnectUtils.HideConnectingUI();
    }
    /// <summary>
    /// 令数据库更新装备、道具等位置。
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="index"></param>
    /// <param name="type">0为道具，1为装备</param>
    /// <param name="isEquipped">如果是装备，则上传是否装备的信息。</param>
    /// <returns></returns>
    IEnumerator UpdateItemIndex(string itemId, int index, int type, bool isEquipped)
    {
        ConnectUtils.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("playerId", PlayerInfoInGame.Id);
        form.AddField("itemId", itemId);
        form.AddField("index", index);
        form.AddField("type", type);
        form.AddField("equipped", isEquipped ? 1 : 0);
        WWW w = new WWW(ConnectUtils.ParsePath(UPDATE_ITEM_INDEX_FILEPATH), form);
        yield return w;
        ConnectUtils.HideConnectingUI();
    }
    /// <summary>
    /// 令数据库更新技能数据
    /// </summary>
    /// <param name="skills"></param>
    /// <param name="skillPointChange"></param>
    /// <returns></returns>
    IEnumerator UpdateSkillData(TempSkills skills, int skillPointChange)
    {
        ConnectUtils.ShowConnectingUI();
        BundleForm form = new BundleForm();
        form.SetField("skillData", JsonUtility.ToJson(skills));
        TempPlayerAttribute attr = new TempPlayerAttribute();
        attr.skillPoint = skillPointChange;
        form.SetField("playerData", skillPointChange == 0 ? "" : JsonUtility.ToJson(attr));
        WWW w = new WWW(ConnectUtils.ParsePath(UPDATE_UNIVERSAL_FILEPATH), form.CompleteForm());
        yield return w;
        if (ConnectUtils.IsPostSucceed(w))
        {
            yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
            SkillReadingPart.Instance.RefreshTargetSkill();
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
        }
        ConnectUtils.HideConnectingUI();
    }


    /// <summary>
    /// 这个空的attr会让目标php获得需要变更的数值（相对的）
    /// </summary>
    /// <returns></returns>
    public static TempPlayerAttribute CreateEmpty()
    {
        TempPlayerAttribute attr = new TempPlayerAttribute();
        return attr;
    }
}
/// <summary>
/// 用于一次性向php post应该post的表单.
/// </summary>
public class BundleForm
{
    private WWWForm linkedForm;
    private Dictionary<string, string> fieldToValues;
    private bool isCompleted = false;
    public bool IsCompleted { get { return isCompleted; } }
    public BundleForm()
    {
        linkedForm = new WWWForm();
        fieldToValues = new Dictionary<string, string>();
        fieldToValues.Add("id", PlayerInfoInGame.Id);
        fieldToValues.Add("recordData", "");
        fieldToValues.Add("itemData", "");
        fieldToValues.Add("playerData", "");
        fieldToValues.Add("deleteEqData", "");
        fieldToValues.Add("rndEquipData", "");
        fieldToValues.Add("enhanceEquipData", "");
        fieldToValues.Add("skillData", "");
        fieldToValues.Add("update", "true");
        fieldToValues.Add("eckey", GlobalSettings.GetEncryptKey());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <returns>是否成功</returns>
    public bool SetField(string field, string value)
    {
        if (!isCompleted)
        {
            fieldToValues[field] = value;
        }
        return !isCompleted;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns>是否成功</returns>
    public bool SetField(BundleFormType type, string value)
    {
        string field = "";
        switch (type)
        {
            case BundleFormType.RECORD:
                field = "recordData";
                break;
            case BundleFormType.IIAITEM:
                field = "itemData";
                break;
            case BundleFormType.PLAYERATTR:
                field = "playerData";
                break;
            case BundleFormType.IIADELETEEQ:
                field = "deleteEqData";
                break;
            case BundleFormType.RNDEQREQUEST:
                field = "rndEquipData";
                break;
            case BundleFormType.EQDATA:
                field = "enhanceEquipData";
                break;
            case BundleFormType.SKILLDATA:
                field = "skillData";
                break;
            default:
                break;
        }
        return SetField(field, value);
    }
    /// <summary>
    /// 完成这个表格并返回WWWForm。
    /// </summary>
    public WWWForm CompleteForm()
    {
        if (!isCompleted)
        {
            foreach (var kv in fieldToValues)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                    linkedForm.AddField(kv.Key, kv.Value);
            }
            isCompleted = true;
        }
        return linkedForm;
    }
    public WWWForm GetForm()
    {
        return linkedForm;
    }
}

public enum BundleFormType
{
    RECORD,
    IIAITEM,
    PLAYERATTR,
    IIADELETEEQ,
    RNDEQREQUEST,
    EQDATA,
    SKILLDATA
}
/// <summary>
/// 用于与服务器端同步更新的请求类。
/// </summary>
public class SyncRequest
{
    static List<SyncRequest> requestToUpload = new List<SyncRequest>();
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
    /// <param name="content">php收到的数据流/字符串</param>
    public static void AppendRequest(string id, string content)
    {
        requestToUpload.Add(new SyncRequest(id, content));
    }
    /// <summary>
    /// 集合当前所有创建的请求，合并为一个WWWForm.(发送给UniveralUpdate.php)
    /// </summary>
    /// <returns></returns>
    public static WWWForm CompleteRequestForm()
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
    /// <summary>
    /// 通过SyncRequest集合创建WWW通讯类，会合并被清空所有待同步的SyncRequest。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static WWW CreateSyncWWW(string path = PlayerRequestBundle.UPDATE_UNIVERSAL_FILEPATH)
    {
        return new WWW(ConnectUtils.ParsePath(path), CompleteRequestForm());
    }
}