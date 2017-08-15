using GameId;
using MachineMatchJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 处理机械擂台战的UI界面
/// </summary>
public class MachineMatchUIManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    [Header("玩家状态")]
    public Text yourRankText;
    public Text yourScoreText;
    public Text yourTicketText;
    [Header("挑战状态")]
    public Text todaysCPTimesText;
    public Text nowCanChallText;
    public Text nextTimeText;
    [Header("敌人挑战")]
    public List<MMUIMember> members = new List<MMUIMember>(4);

    public static MachineMatchUIManager Instance { get; set; }
    private const double TWOHOURSCOENDS = 7200;
    private double leftTime = 0;
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        //下次刷新？
        if (leftTime > 0)
        {
            leftTime -= Time.deltaTime;
            nextTimeText.text = new DateTime((long)(leftTime * TimeSpan.TicksPerSecond)).ToString("HH:mm:ss");
        }
    }
    public void SetVisiable(bool visiable)
    {
        canvasGroup.alpha = visiable ? 1f : 0f;
        canvasGroup.blocksRaycasts = visiable;
        canvasGroup.interactable = visiable;
    }
    public void Refresh(TempMachineMatchInfo info, List<PlayerUnit> targets)
    {
        targets.Sort(new MMSorter());
        yourRankText.text = info.rank.ToString();
        yourScoreText.text = info.score.ToString();
        lint ticketAmount = ItemDataManager.GetItemAmount(Items.MM_TICKET);
        yourTicketText.text = ticketAmount.ToString();
        var maxChangeTimes = MachineMatchManager.GetMaxChangePerDay();
        var maxChallTimes = MachineMatchManager.GetMaxChallPerTimes();
        var diffChange = maxChangeTimes - info.currentChangeTimes;
        var diffChall = maxChallTimes - info.currentChallTimes;
        bool freeze = diffChall <= 0 || ticketAmount <= 0;
        todaysCPTimesText.text = string.Format("{0}/{1}", diffChange, maxChangeTimes);
        nowCanChallText.text = string.Format("{0}/{1}", diffChall, maxChallTimes);
        for (int i = 0; i < members.Count; i++)
        {
            var member = members[i];
            if (targets.Count > i)
            {
                member.LinkedPlayerUnit = targets[i];
            }
            else
            {
                member.LinkedPlayerUnit = null;
            }
            if (freeze) member.Freeze();
        }
        DateTime dateTime = DateTime.Parse(info.lastRefreshTime);
        leftTime = TWOHOURSCOENDS - ((dateTime.Hour) % 2 * 3600 + dateTime.Minute * 60 + dateTime.Second);
    }
    private class MMSorter : IComparer<PlayerUnit>
    {
        public int Compare(PlayerUnit x, PlayerUnit y)
        {
            return x.challRank.CompareTo(y.challRank);
        }
    }
}
