using GameId;
using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionManager : MonoBehaviour
{
    private static string GET_DATA_PATH = "scripts/player/expedition/getExpeditionData.php";

    /// <summary>
    /// 光年/该数 = 目前应该的难度等级。
    /// 目前是每3光年一级。一次前进
    /// </summary>
    public static int LIGHTYEAR_TO_DIFFICULTY_SCALE = 3;
    public static ExpeditionManager Instance { get; set; }
    [Header("界面组件")]
    public Button beginExpedition;
    public Button[] expeditionOptionButtons;
    public Text playersInfo;
    public Text expeditionInfo;
    public Text rankInfo;
    public Toggle expeditionWindowToggle;

    private string originExpeditionInfo;
    private TempPlayerExpeditionInfo expeInfo;

    private void Awake()
    {
        Instance = this;
        originExpeditionInfo = expeditionInfo.text;
    }
    /// <summary>
    /// 进入场景后调用的.
    /// </summary>
    public void InitOrRefresh()
    {
        StartCoroutine(RefreshCor());
    }
    IEnumerator RefreshCor()
    {
        SetAllExpeditionOptionsInteractble(false);
        expeditionInfo.text = originExpeditionInfo;
        beginExpedition.interactable = false;
        yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        WWWForm form = new WWWForm();
        form.AddField("player_id", PlayerInfoInGame.Id);
        WWW w = new WWW(ConnectUtils.ParsePath(GET_DATA_PATH), form);
        ConnectUtils.ShowConnectingUI();
        yield return w;
        ConnectUtils.HideConnectingUI();
        if (w.text != "failed")
        {
            expeInfo = JsonUtility.FromJson<TempPlayerExpeditionInfo>(w.text);
            RefreshPlayersRecordText();
            //可以刷新具体信息了（按按钮
            beginExpedition.interactable = true;
            StartCoroutine(GetRankListCor());
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
            yield break;
        }
    }
    /// <summary>
    /// 点击开始远征,顺便判断玩家是否拥有足够的星币，如果是第一次开始远征，可以根据
    /// </summary>
    public void RefreshExpeditionInfo()
    {
        if (expeInfo.nowLightYear == 0 && expeInfo.maxLightYear >= 30)
        {
            int money = 0;
            int ly = (int)(expeInfo.maxLightYear * 0.75);
            for (int i = 1; i <= ly; i += 3)
            {
                money += CalculateTotalMoneyToPay(i);
            }
            money /= 2;
            bool canPay = !(money > PlayerInfoInGame.GetMoney());
            MessageBox.Show("可以支付" + money + "星币来直接前进" + ly + "光年。" + (!canPay ? "不过你目前无法支付这笔钱。" : ""), "提示", (result) =>
                 {
                     if (DialogResult.Yes == result)
                     {
                         SetAllExpeditionOptionsInteractble(false);
                         StartCoroutine(JumpToTargetLightYear(ly, money));
                     }
                 }, canPay ? MessageBoxButtons.YesNo : MessageBoxButtons.OK);
        }
        beginExpedition.interactable = false;
        SetAllExpeditionOptionsInteractble(true);
        if (expeInfo.ifEscaped)
        {
            expeditionOptionButtons[2].interactable = false;
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("·你接下来可能遇到的敌人：");
        sb.AppendLine(EnemyExpeditionManager.Instance.GenearatePossibleEnemyText(expeInfo.nowLightYear + 1));
        sb.AppendLine("·你需要支付的星币，并根据三个不同选项分割。");
        int nowLightYear = expeInfo.nowLightYear;
        int[] toPay = { 0, 0, 0 };
        sb.Append("远征油费:");
        toPay[0] += CalculateOilMoneyToPay(nowLightYear + 3);
        sb.Append(toPay[0]);
        sb.Append("/");
        toPay[1] += CalculateOilMoneyToPay(nowLightYear + 2);
        sb.Append(toPay[1]);
        sb.Append("/");
        toPay[2] += CalculateOilMoneyToPay(nowLightYear + 1) * 2;
        sb.Append(toPay[2]);
        sb.AppendLine();
        sb.Append("远征补给费:");
        toPay[0] += CalculateSupplyMoneyToPay(nowLightYear + 3);
        sb.Append(CalculateSupplyMoneyToPay(nowLightYear + 3));
        sb.Append("/");
        toPay[1] += CalculateSupplyMoneyToPay(nowLightYear + 2);
        sb.Append(CalculateSupplyMoneyToPay(nowLightYear + 2));
        sb.Append("/");
        toPay[2] += CalculateSupplyMoneyToPay(nowLightYear + 1) * 2;
        sb.Append(CalculateSupplyMoneyToPay(nowLightYear + 1) * 2);
        sb.AppendLine();
        sb.Append("合计费用:");
        sb.Append(toPay[0]);
        sb.Append("/");
        sb.Append(toPay[1]);
        sb.Append("/");
        sb.Append(toPay[2]);

        int nowMoney = PlayerInfoInGame.GetMoney();
        if (nowMoney < toPay[0])
        {
            expeditionOptionButtons[0].interactable = false;
            if (nowMoney < toPay[1])
            {
                expeditionOptionButtons[1].interactable = false;
                if (nowMoney < toPay[2])
                {
                    expeditionOptionButtons[2].interactable = false;
                }
            }
        }

        expeditionInfo.text = sb.ToString();
    }
    /// <summary>
    /// 起始的直接跳过
    /// </summary>
    /// <param name="ly"></param>
    /// <param name="money"></param>
    /// <returns></returns>
    IEnumerator JumpToTargetLightYear(int ly, int money)
    {
        TempPlayerExpeditionInfo exInfo = new TempPlayerExpeditionInfo();
        exInfo.nowLightYear = ly;
        exInfo.maxLightYear = exInfo.nowLightYear > expeInfo.maxLightYear ? exInfo.nowLightYear : expeInfo.maxLightYear;
        TempPlayerAttribute attr = new TempPlayerAttribute();
        attr.money -= money;
        yield return PlayerRequestBundle.RequestUpdateRecord(exInfo, null, attr);
        InitOrRefresh();
    }
    /// <summary>
    /// 刷新前五排名
    /// </summary>
    /// <returns></returns>
    IEnumerator GetRankListCor()
    {
        WWWForm form = RankListManager.Instance.GenerateRefreshForm(1, RankListManager.RANK_TYPE.EXPEDITION, 5);
        WWW w = new WWW(ConnectUtils.ParsePath(RankListManager.RANK_PATH), form);
        yield return w;
        if (w.text != null)
        {
            TempRankInfoBundle bundle = JsonUtility.FromJson<TempRankInfoBundle>(w.text);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bundle.infos.Length; i++)
            {
                var info = bundle.infos[i];
                sb.Append(i + 1);
                sb.Append(" : ");
                sb.Append(info.arg2);
                sb.Append(" - ");
                sb.AppendLine(info.arg3);
            }
            rankInfo.text = sb.ToString();
        }
    }
    /// <summary>
    /// 开始远征（前进）
    /// </summary>
    /// <param name="diff"></param>
    public void StartGoing(int diff)
    {
        StartCoroutine(StartBattle(diff));
    }
    IEnumerator StartBattle(int diff)
    {
        EXPEDITION_DIFFICULTY difficulty = (EXPEDITION_DIFFICULTY)diff;
        if (difficulty == EXPEDITION_DIFFICULTY.LOW)
        {
            TempPlayerExpeditionInfo exInfo = new TempPlayerExpeditionInfo();
            exInfo.ifEscaped = true;
            exInfo.nowLightYear = expeInfo.nowLightYear + 1;
            exInfo.maxLightYear = exInfo.nowLightYear > expeInfo.maxLightYear ? exInfo.nowLightYear : expeInfo.maxLightYear;
            TempPlayerAttribute attr = new TempPlayerAttribute();
            attr.money -= CalculateTotalMoneyToPay(exInfo.nowLightYear) * 2;
            yield return PlayerRequestBundle.RequestUpdateRecord(exInfo, null, attr);
            InitOrRefresh();
            yield break;
        }
        UIWindow window = GetComponentInParent<UIWindow>();
        window.Hide();
        var info = new ExpeditionBattleInfo();
        info.targetLightYear = expeInfo.nowLightYear + diff + 1;
        info.nowMaxLightYear = expeInfo.maxLightYear;
        info.moneyToBePaied = CalculateTotalMoneyToPay(info.targetLightYear);

        float hard = 0.8f + 0.2f * diff;
        info.enemySpawnData = EnemyExpeditionManager.Instance.GenerateEnemySpawn((int)(info.targetLightYear * hard));

        yield return BattleManager.Instance.InitBattle(info);
        BattleManager.Instance.SetBattleCanvasVisible(true);
        BattleManager.Instance.SetBackInvoke(() =>
        {
            BattleManager.Instance.SetBattleCanvasVisible(false);
            InitOrRefresh();
            window.Show();
        });
    }
    /// <summary>
    /// 设置前进的按钮是否可执行
    /// </summary>
    /// <param name="interactable"></param>
    void SetAllExpeditionOptionsInteractble(bool interactable)
    {
        for (int i = 0; i < expeditionOptionButtons.Length; i++)
        {
            expeditionOptionButtons[i].interactable = interactable;
        }
    }
    /// <summary>
    /// 刷新玩家记录的文本。
    /// </summary>
    void RefreshPlayersRecordText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("你目前前进的里程数：<b>");
        sb.Append(expeInfo.nowLightYear);
        sb.AppendLine(" 光年</b>");
        sb.AppendLine("你的最高纪录：<b>");
        sb.Append(expeInfo.maxLightYear);
        sb.AppendLine(" 光年</b>");
        sb.AppendLine("你上次是否消极怠工：<b>");
        sb.Append(expeInfo.ifEscaped ? "是" : "否");
        sb.Append("</b>");
        playersInfo.text = sb.ToString();
    }
    /// <summary>
    /// 合计的费用
    /// </summary>
    /// <param name="lightYear"></param>
    /// <returns></returns>
    int CalculateTotalMoneyToPay(int lightYear)
    {
        return CalculateOilMoneyToPay(lightYear) + CalculateSupplyMoneyToPay(lightYear);
    }
    /// <summary>
    /// 计算指定光年需要支付的远征油费
    /// </summary>
    /// <returns></returns>
    int CalculateOilMoneyToPay(int lightYear)
    {
        int result = 400;//最开始需要支付的
        int d = 100 + PlayerInfoInGame.Level / 2;
        result += d * lightYear;
        return result;
    }
    /// <summary>
    /// 计算指定光年需要支付的远征补给费
    /// </summary>
    /// <returns></returns>
    int CalculateSupplyMoneyToPay(int lightYear)
    {
        int result = 500 + (PlayerInfoInGame.Level / 100) * 200;//最开始需要支付的
        int d = PlayerInfoInGame.Level - 20 > 0 ? (PlayerInfoInGame.Level - 20) * 100 : 0;
        result += d;
        return result;
    }
    /// <summary>
    /// BattleResult调用函数，用于生成特殊掉落
    /// </summary>
    /// <param name="items"></param>
    public static void AddExepSpecItemsToDict(Dictionary<string, int> items, ExpeditionBattleInfo info)
    {
        float mult = BattleAwardMult.GetExpeditionDropMult();
        float baseChance = info.targetLightYear * 0.03f;

        float chanceForDG = baseChance * 0.25f * mult;
        float chanceForSA = chanceForDG * 3f;
        chanceForDG *= Random.Range(1f, 1.2f);
        chanceForSA *= Random.Range(1f, 1.2f);
        int amountDG = (int)chanceForDG;
        amountDG += Random.value <= chanceForDG - amountDG ? 1 : 0;
        int amountSA = (int)chanceForSA;
        amountSA += Random.value <= chanceForSA - amountSA ? 1 : 0;
        items.Add(Items.DEEP_GOOD, amountDG);
        items.Add(Items.STAR_ASH, amountSA);
    }
}
public enum EXPEDITION_DIFFICULTY
{
    LOW,//0
    NORMAL,//1
    HIGH//2
}
public class ExpeditionBattleInfo
{
    public int targetLightYear;
    public int nowMaxLightYear;
    public int moneyToBePaied;

    public EnemySpawnData enemySpawnData;
}