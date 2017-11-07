using GameId;
using ItemContainerSuite;
using MachineMatchJson;
using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 管理机械擂台战逻辑
/// </summary>
public class MachineMatchManager : MonoBehaviour
{
    public static MachineMatchManager Instance { get; set; }

    public static PlayerUnit CurrentTarget
    {
        get
        {
            return currentTarget;
        }

        set
        {
            currentTarget = value;
        }
    }

    public static TempMachineMatchInfo CurrentInfo
    {
        get
        {
            return Instance.currentInfo;
        }
    }

    private const string GET_INFO_PATH = "scripts/player/machinematch/getMachineMatchInfo.php";
    private const string GET_MEM_PATH = "scripts/player/machinematch/getMMMemInfo.php";
    private static PlayerUnit currentTarget;
    private MachineMatchUIManager UIManager;
    private TempMachineMatchInfo currentInfo;
    private void Awake()
    {
        Instance = this;
    }
    public void InitOrRefresh()
    {
        UIManager = MachineMatchUIManager.Instance;
        StartCoroutine(_InitOrRefresh());
    }
    IEnumerator _InitOrRefresh()
    {
        yield return ItemDataManager.RequestGetItemsAmount();
        CU.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("player_id", PlayerInfoInGame.Id);
        WWW w = new WWW(CU.ParsePath(GET_INFO_PATH), form);
        yield return w;
        CU.HideConnectingUI();
        if (CU.IsPostSucceed(w))
        {
            //首先获取玩家基本属性，需要注意的是，大部分逻辑将在这里搞定。
            TempMachineMatchInfo info = JsonUtility.FromJson<TempMachineMatchInfo>(w.text);
            currentInfo = info;
            var targetIds = info.currentTargetIds;
            List<PlayerUnit> unitsToBeInit = new List<PlayerUnit>();
            for (int i = 0; i < targetIds.Length; i++)
            {
                WWWForm nf = new WWWForm();
                nf.AddField("id", targetIds[i]);
                WWW nw = new WWW(CU.ParsePath(GET_MEM_PATH), nf);
                CU.ShowConnectingUI();
                yield return nw;
                CU.HideConnectingUI();
                if (CU.IsPostSucceed(nw))
                {
                    // Debug.Log(nw.text);
                    var tempInfo = JsonUtility.FromJson<TempMMPlayerInfo>(nw.text);
                    PlayerUnit unit = new PlayerUnit();
                    unit.Id = targetIds[i];
                    unit.level = tempInfo.level;
                    unit.designId = tempInfo.designId;
                    unit.challRank = tempInfo.rank;
                    unit.nickname = tempInfo.nickname;
                    unitsToBeInit.Add(unit);
                }
                else
                {
                    CU.ShowConnectFailed();
                    break;
                }
            }
            UIManager.Refresh(info, unitsToBeInit);
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
    }
    /// <summary>
    /// 开始战斗！
    /// </summary>
    /// <param name="member"></param>
    public void InitTargetPlayerUnitBattle(MMUIMember member)
    {
        StartCoroutine(_InitTargetPlayerUnitBattle(member.LinkedPlayerUnit));
    }
    IEnumerator _InitTargetPlayerUnitBattle(PlayerUnit unit)
    {
        CurrentTarget = unit;
        yield return StartCoroutine(unit.InitUnitProperty());
        yield return BattleManager.Instance.InitBattle(this);
        UIManager.SetVisiable(false);
        BattleManager.Instance.SetBattleCanvasVisible(true);
        BattleManager.Instance.SetBackInvoke(() =>
        {
            BattleManager.Instance.SetBattleCanvasVisible(false);
            InitOrRefresh();
            UIManager.SetVisiable(true);
        });
    }
    public void RefreshAndChange()
    {
        StartCoroutine(_RefreshAndChange());
    }
    IEnumerator _RefreshAndChange()
    {
        TempMMSyncInfo info = new TempMMSyncInfo();
        info.addChange = 2;
        SyncRequest.AppendRequest(Requests.MACHINE_MATCH_DATA, info);
        SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(Items.MM_TICKET, -2).ToJson(false));
        WWW w = SyncRequest.CreateSyncWWW();
        yield return w;
        if (!CU.IsPostSucceed(w))
        {
            CU.ShowConnectFailed();
        }
        InitOrRefresh();
    }
    public static int GetMaxChangePerDay()
    {
        return 30 + PlayerInfoInGame.VIP_Level * 1;
    }
    public static int GetMaxChallPerTimes()
    {
        return 5 + Mathf.CeilToInt(PlayerInfoInGame.VIP_Level * 0.25f);
    }
    public void OpenShop()
    {
        List<Transform> transforms = new List<Transform>();
        Transform trans = CommonItemPool.Spawn(CommonItemPool.SHOP_ITEM_PREFAB);
        trans.GetComponent<ShopItemGrid>().SetParam(Items.BOX_CHIP, ShopItemGrid.WORTH_TYPE.MM_SCORE, 100);
        transforms.Add(trans);

        ItemContainer.ShowContainer(transforms, () =>
        {
            var rect = trans.GetComponent<RectTransform>().rect;
            ItemContainerParam.SetParam(rect.width, rect.height, 4);
        }, () =>
        {
            CommonItemPool.RecycleAll();
            InitOrRefresh();
            //Debug.Log("回收成功");
        }, "挑战商店", "使用挑战积分兑换/购买物品。");
    }
}
namespace MachineMatchJson
{
    [System.Serializable]
    public class TempMachineMatchInfo
    {
        public string[] currentTargetIds;
        public int[] currentTargetRanks;
        public int currentChallTimes;
        public int currentChangeTimes;
        public int score;
        public int rank;
        public string lastRefreshTime;
    }
    [System.Serializable]
    public class TempMMPlayerInfo
    {
        public string nickname;
        public int level;
        public int designId;
        public int rank;
    }
    [System.Serializable]
    public class TempMMSyncInfo
    {
        public int addScore;
        public int addChall;
        public int addChange;
    }
}
