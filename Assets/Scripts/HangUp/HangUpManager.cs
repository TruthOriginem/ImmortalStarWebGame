using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SerializedClassForJson;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class HangUpManager : MonoBehaviour
{
    private enum BUTTON_TYPE
    {
        BEGIN,
        COUNT
    }
    [Header("LIG")]
    public Text titleForLig;
    public Text contentForLig;
    [Header("离线挂机")]
    public Text allForHang;//挂机显示信息
    public Text buttonTextForHang;//按钮
    private BUTTON_TYPE button_type = BUTTON_TYPE.BEGIN;
    /// <summary>
    /// 挂机系数，每轮时间需乘这个数
    /// </summary>
    private const float HANG_UP_TIME_FACTOR = 1.5f;
    /// <summary>
    /// 挂机基础，每轮时间都会加上这个值
    /// </summary>
    private const int HANG_UP_TIME_ADDON = 3;
    /// <summary>
    /// 挂机获得经验补正
    /// </summary>
    private const float HANG_UP_EXP_MULT = 1f;
    /// <summary>
    /// 挂机获得星币补正
    /// </summary>
    private const float HANG_UP_MONEY_MULT = 1f;
    /// <summary>
    /// 挂机获得装备概率补正
    /// </summary>
    private const float HANG_UP_EQUIP_MULT = 0.5f;

    /// <summary>
    /// 表示挂机中，挂机时无法战斗
    /// </summary>
    [HideInInspector]
    public bool isHanging = false;
    /// <summary>
    /// 如果为true将LoadScene
    /// </summary>
    public bool isDirty = false;

    private TempLigRecord linkedRecord;
    public static HangUpManager Instance { get; set; }

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        //StartCoroutine(Init());
    }
    IEnumerator Init()
    {
        yield return new WaitForSeconds(5f);
        MakeDirty();
    }

    void Update()
    {
        if (isDirty)
        {
            LoadScene();
            isDirty = false;
        }
    }
    public void MakeDirty()
    {
        isDirty = true;
    }

    public void LoadScene()
    {
        StartCoroutine(RefreshSceneCor());
    }
    IEnumerator RefreshSceneCor()
    {
        yield return BattleInstanceManager.Instance.RefreshAllGrids();
        //刷新左边的信息
        yield return PlayerRequestBundle.RequestGetRecord<TempLigRecord>();
        TempLigRecord record = PlayerRequestBundle.record as TempLigRecord;
        if (record == null)
        {
            ConnectUtils.ShowConnectFailed();
            yield break;
        }
        if (record.lig_id == null || record.lig_id == "")
        {
            yield break;
        }
        linkedRecord = record;
        isHanging = record.lig_hangTime >= 0;
        BatInsGridData grid = ScenarioManager.GetGridDataById(record.lig_id);
        if (grid == null)
        {
            yield break;
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(TextUtils.GetSizedString(grid.name, 20));
        sb.AppendLine(BattleInstanceTooltip.CompleteDataContent(grid));
        sb.Append("花费时间:" + GetHangTime() + "秒");
        contentForLig.text = sb.ToString();
        Button button = buttonTextForHang.GetComponentInParent<Button>();
        //刷新挂机信息
        OnInputHangUpTimesChange();

        if (isHanging)//如果有挂机状态
        {
            titleForLig.text = "您上次挂机的关卡信息";
            buttonTextForHang.text = "结算";
            button_type = BUTTON_TYPE.COUNT;
            button.interactable = true;
        }
        else
        {
            titleForLig.text = "上次您通过的关卡信息";
            buttonTextForHang.text = "开始挂机";
            button_type = BUTTON_TYPE.BEGIN;
            button.interactable = true;
        }
    }
    /// <summary>
    /// 当时间变更时调用，根据挂机时间来计算次数
    /// </summary>
    /// <param name="value"></param>
    public void OnInputHangUpTimesChange()
    {
        if (linkedRecord == null || !isHanging)
        {
            allForHang.text = "无";
            return;
        }
        BatInsGridData grid = ScenarioManager.GetGridDataById(linkedRecord.lig_id);
        if (grid != null)
        {
            //到目前为止挂机刷怪轮数
            int times = GetHangTimes();
            //总计秒
            int seconds = linkedRecord.lig_hangTime;
            int maxHours = CheckMaxHangTime(ref seconds);
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            int money;
            long exp;

            Dictionary<string, int> resultsDic =
            BattleResult.GenerateResultsDict(grid.enemys, false, true, times, out money, out exp);
            money = Mathf.RoundToInt(money * HANG_UP_MONEY_MULT);
            exp = (long)(exp * HANG_UP_EXP_MULT);
            StringBuilder sb = new StringBuilder();
            sb.Append("您现在是");
            if (PlayerInfoInGame.VIP_Level > 0)
            {
                sb.Append("VIP");
                sb.Append(PlayerInfoInGame.VIP_Level);
            }
            else
            {
                sb.Append("普通");
            }
            sb.AppendLine("玩家,最高挂机时间为：");
            sb.Append(maxHours);
            sb.AppendLine("小时");
            sb.Append("距离开始挂机，已经过去了");
            sb.Append((int)ts.TotalHours);
            sb.Append("时");
            sb.Append(ts.Minutes);
            sb.Append("分");
            sb.Append(ts.Seconds);
            sb.Append("秒");
            sb.Append("。总计");
            sb.Append(TextUtils.GetYellowText(times.ToString()));
            sb.AppendLine("轮战斗。");

            sb.AppendLine();
            sb.Append("可以获得经验:");
            sb.AppendLine(TextUtils.GetColoredText(exp.ToString(), Color.white));
            sb.Append("可以获得星币:");
            sb.AppendLine(TextUtils.GetColoredText(money.ToString(), Color.white));
            sb.AppendLine();
            sb.AppendLine(TextUtils.GetColoredText("<size=17>大概掉落:</size>", Color.cyan));
            string[] item_ids = resultsDic.Keys.ToArray();
            int[] amounts = resultsDic.Values.ToArray();

            for (int i = 0; i < item_ids.Length; i++)
            {
                sb.Append(ItemDataManager.GetItemName(item_ids[i]));
                sb.Append(" ~ ");
                sb.AppendLine(TextUtils.GetColoredText(amounts[i].ToString(), Color.white));
            }
            allForHang.text = sb.ToString();
        }
    }
    int GetHangTimes()
    {
        int seconds = linkedRecord.lig_hangTime;
        CheckMaxHangTime(ref seconds);
        return linkedRecord != null ? (int)(seconds / GetHangTime()) : 0;
    }
    /// <summary>
    /// 每轮战斗所需要的时间
    /// </summary>
    /// <returns></returns>
    float GetHangTime()
    {
        return HANG_UP_TIME_FACTOR * ((HANG_UP_TIME_ADDON + (linkedRecord.lig_lostTime)));
    }
    public void PressButton()
    {
        switch (button_type)
        {
            case BUTTON_TYPE.BEGIN:
                {
                    MessageBox.Show("您是否开始离线挂机？这将导致挂机未结算前，您无法战斗。", "温馨提示",
                        (dialogResult) => { StartCoroutine(BeginHang(dialogResult)); }, MessageBoxButtons.YesNo);
                    break;
                }
            case BUTTON_TYPE.COUNT:
                {
                    StartCoroutine(BeginCount());
                    break;
                }
            default: break;
        }
    }
    IEnumerator BeginHang(DialogResult dialogResult)
    {
        if (DialogResult.Yes == dialogResult)
        {
            linkedRecord.lig_hangTime = 0;//标识为0代表开始挂机
            yield return PlayerRequestBundle.RequestUpdateRecord(linkedRecord);
            LoadScene();
        }
    }

    IEnumerator BeginCount()
    {
        yield return PlayerRequestBundle.RequestGetRecord<TempLigRecord>();
        if (PlayerRequestBundle.record == null)
        {
            yield break;
        }
        linkedRecord = PlayerRequestBundle.record as TempLigRecord;

        BatInsGridData grid = ScenarioManager.GetGridDataById(linkedRecord.lig_id);
        int times = GetHangTimes();
        int money;
        long exp;
        IIABinds iia = new IIABinds(BattleResult.GenerateResultsDict(grid.enemys, true, true, times, out money, out exp));
        TempPlayerAttribute attr = new TempPlayerAttribute();
        attr.money = Mathf.RoundToInt(money * HANG_UP_MONEY_MULT);
        attr.exp = (long)(exp * HANG_UP_EXP_MULT);
        TempRandEquipRequest[] requests = grid.eqDrop.CreateSpecRequests(times, HANG_UP_EQUIP_MULT);

        //标识完成挂机并更新状态
        linkedRecord.lig_hangTime = -1;
        yield return PlayerRequestBundle.RequestUpdateRecord(linkedRecord, iia, attr, requests);

        StringBuilder sb = new StringBuilder();
        sb.Append("您挂机了");
        sb.Append(times);
        sb.AppendLine("轮战斗。");
        sb.Append("获得经验:");
        sb.AppendLine(attr.exp.ToString());
        sb.Append("获得星币:");
        sb.AppendLine(attr.money.ToString());
        for (int i = 0; i < iia.item_ids.Length; i++)
        {
            sb.Append("获得");
            sb.Append(ItemDataManager.GetItemName(iia.item_ids[i]));
            sb.Append(" x ");
            sb.AppendLine(iia.amounts[i].ToString());
        }
        if (requests != null && requests.Length != 0)
        {
            sb.Append("获得 ");
            sb.Append(requests.Length);
            sb.Append(" 件装备");
        }

        MessageBox.Show(sb.ToString(), "结算完成!");
        LoadScene();
    }
    /// <summary>
    /// 检查是否超出最大挂机时间，返回最大的小时。
    /// </summary>
    /// <param name="nowSeconds"></param>
    /// <returns></returns>
    public int CheckMaxHangTime(ref int nowSeconds)
    {
        int maxHours = 18 + PlayerInfoInGame.VIP_Level * 6;
        TimeSpan maxTs = TimeSpan.FromHours(maxHours);
        TimeSpan nowTs = TimeSpan.FromSeconds(nowSeconds);
        if (nowTs > maxTs)
        {
            nowSeconds = (int)maxTs.TotalSeconds;
        }
        return maxHours;
    }
}
