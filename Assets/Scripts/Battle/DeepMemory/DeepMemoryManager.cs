using GameId;
using ItemContainerSuite;
using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DeepMemoryManager : MonoBehaviour
{
    private const string GET_PATH = "scripts/player/deepmemory/getDeepMemoryData.php";
    public static DeepMemoryManager Instance { get; set; }

    public static TempDeepMemoryData CurrData
    {
        get
        {
            return Instance.currData;
        }
    }

    public List<DeepMemoryItem> uiItems;
    public Text currentItemDescription;
    public Button resetButton;
    public Button extreButton;
    public Button diveButton;
    public Text playerInfo;

    private TempDeepMemoryData currData;

    private void Awake()
    {
        Instance = this;
    }

    public void InitOrRefresh()
    {
        resetButton.interactable = false;
        extreButton.interactable = false;
        diveButton.interactable = false;
        StartCoroutine(_InitOrRefresh());
    }
    IEnumerator _InitOrRefresh()
    {
        yield return ItemDataManager.RequestGetItemsAmount();
        WWWForm form = new WWWForm();
        form.AddField("player_id", PlayerInfoInGame.Id);
        WWW w = new WWW(CU.ParsePath(GET_PATH), form);
        CU.ShowConnectingUI();
        yield return w;
        CU.HideConnectingUI();
        if (CU.IsPostSucceed(w))
        {
            currData = JsonUtility.FromJson<TempDeepMemoryData>(w.text);
            DesignationManager.CheckDeepMemory(currData);
            RefreshDynamicInfo();
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
    }

    /// <summary>
    /// 刷新信息，并管理按钮
    /// </summary>
    void RefreshDynamicInfo()
    {
        int difficulty = currData.currDifficulty;
        StringBuilder sb = new StringBuilder();
        //深层
        sb.AppendLine(string.Format("{0}层", currData.currDepth));
        //记忆浓度
        sb.AppendFormat("{0}%({1})", GetNonduByDifficulty(difficulty).ToString("0.00"), GetDifficultyDescription(difficulty));
        sb.AppendLine();
        //回溯积分倍率
        sb.AppendFormat("{0}<color=grey>({1})</color>", GetScoreMultiFactor(difficulty).ToString("0.00"), GetScoreMultiFactor(difficulty).ToString("0.00"));
        sb.AppendLine();
        //敌人属性倍率
        sb.AppendFormat("{0}", GetEnemyLevelMultiFactor(difficulty).ToString("0.00"));
        sb.AppendLine();
        //最高层数
        sb.AppendLine(string.Format("<b>{0}</b>层", currData.maxDepth));
        //积分
        sb.AppendFormat("<color=lime>{0}</color>", currData.score);
        sb.AppendLine();
        //当前回溯卡倍率
        int amountlv1 = ItemDataManager.GetItemAmount(Items.CARD_DM_DOUBLE);
        sb.Append(amountlv1 > 0 ? 2 : 1);
        sb.AppendLine();
        //倍率卡数目
        sb.Append(amountlv1);
        sb.AppendLine();
        //现在是否可以重置
        sb.Append(currData.freeResetUsed ? "<color=red>不可以</color>" : "<color=green>可以</color>");
        playerInfo.text = sb.ToString();
        //目前只有40层
        diveButton.interactable = true;
        resetButton.interactable = currData.currDepth != 0 || currData.currDifficulty != 0;
        extreButton.interactable = currData.currDepth == 0 && currData.currDifficulty == 0;
        RefreshItem();
    }
    /// <summary>
    /// 刷新5个现有的玩意儿
    /// </summary>
    void RefreshItem()
    {
        int depth = currData.currDepth;
        for (int i = 0; i < uiItems.Count; i++)
        {
            var item = uiItems[i];
            if (depth + i == 0)
            {
                item.Refresh(SpriteLibrary.EMPTY(), "无");
                currentItemDescription.text = "当前深度为0。";
                continue;
            }
            var attr = GetEnemyAttrByDepth(depth + i);
            if (attr == null)
            {
                item.Refresh(SpriteLibrary.EMPTY(), "无");
                if (i == 0)
                {
                    currentItemDescription.text = "暂无。";
                    diveButton.interactable = false;
                }
            }
            else
            {
                item.Refresh(attr.GetIconSprite(), string.Format("{0}\nLv.{1}", attr.name, GetEnemyLevel(depth + i)));
                if (i == 0) currentItemDescription.text = attr.description;
            }
        }
    }
    /// <summary>
    /// 获得指定深度的怪物属性，如果为默认值-1则获取当层怪物属性。
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    public EnemyAttribute GetEnemyAttrByDepth(int depth = -1)
    {
        if (depth == -1)
        {
            depth = currData.currDepth;
        }
        int baseIndex = depth / 10 + 1;
        bool isBoss = depth % 10 == 0;
        string id = string.Format("dm_{0}{1}", isBoss ? "boss" : "base", isBoss ? baseIndex - 1 : baseIndex);
        return EnemyDataManager.AskForEnemyAttribute(id);
    }
    /// <summary>
    /// 获取指定层数怪物指定等级
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    public int GetEnemyLevel(int depth = -1)
    {
        if (depth == -1)
        {
            depth = currData.currDepth;
        }
        return (int)Mathf.Ceil(10 * depth * GetEnemyLevelMultiFactor(currData.currDifficulty));
    }
    /// <summary>
    /// 下潜/开始时选择下潜到哪里，或者开始打怪。
    /// </summary>
    public void Dive()
    {
        //没有下潜之时
        if (currData.currDepth == 0)
        {
            if (currData.maxDepth > 10)//超过10层即可快速进行
            {
                int maxDepth = (int)(currData.maxDepth * 0.8f);
                InputIntBox.Show("你可以一次性下潜到" + maxDepth + "的深度，请选择你要下潜的深度。", "提示", maxDepth, (result, inputValue) =>
                       {
                           inputValue = inputValue <= 1 ? 1 : inputValue;
                           if (result == DialogResult.Yes)
                           {
                               int scoreToGet = GetInitDiveTotalScore(inputValue);
                               MessageBox.Show("你将获得" + scoreToGet + "回溯积分并下潜至" + inputValue + "层，是否确定？", "提示", (result2) =>
                                   {
                                       if (result2 == DialogResult.OK)
                                       {
                                           SyncRequest.AppendRequest(Requests.DEEP_MEMORY_DATA, new TempDeepMemorySyncData { addDepth = inputValue, addScore = scoreToGet });
                                           StartCoroutine(_OpenSync());
                                       }
                                   }, MessageBoxButtons.OKCancel);
                           }

                       }, MessageBoxButtons.YesNo);
            }
            else
            {
                MessageBox.Show("你将直接下潜，是否确定？", "提示", (result2) =>
                {
                    if (result2 == DialogResult.OK)
                    {
                        SyncRequest.AppendRequest(Requests.DEEP_MEMORY_DATA, new TempDeepMemorySyncData { addDepth = 1 });
                        StartCoroutine(_OpenSync());
                    }
                }, MessageBoxButtons.OKCancel);
            }
        }
        else
        {
            StartCoroutine(_StartDive());
        }

    }
    IEnumerator _StartDive()
    {
        UIWindow window = GetComponentInParent<UIWindow>();
        window.Hide();
        yield return BattleManager.Instance.InitBattle(this);
        BattleManager.Instance.SetBattleCanvasVisible(true);
        BattleManager.Instance.SetBackInvoke(() =>
        {
            BattleManager.Instance.SetBattleCanvasVisible(false);
            InitOrRefresh();
            window.Show();
        });
    }
    /// <summary>
    /// 重置
    /// </summary>
    public void ResetDM()
    {
        int amount = ItemDataManager.GetItemAmount("dm_resetcard");
        string content;
        bool canReset = false;
        if (!currData.freeResetUsed)
        {
            content = "可以使用免费重置。";
            canReset = true;
        }
        else if (amount > 0)
        {
            content = "消耗一个记忆中枢重置卡进行重置。";
            canReset = true;
        }
        else
        {
            content = "你当前无法进行重置。请等到第二天免费重置或者获取重置卡进行重置。";
        }
        MessageBox.Show(content, "提示", (result) =>
        {
            if (result == DialogResult.OK && canReset)
            {
                var data = new TempDeepMemorySyncData
                {
                    setReset = true
                };
                if (currData.freeResetUsed)
                {
                    SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds("dm_resetcard", -1).ToJson(false));
                }
                SyncRequest.AppendRequest(Requests.DEEP_MEMORY_DATA, data);
                StartCoroutine(_OpenSync());
            }
        }, canReset ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK);

    }
    /// <summary>
    /// 投入极限水晶
    /// </summary>
    public void GiveCrystal()
    {
        List<int> allowedLevel = new List<int>();
        lint crystal_1 = ItemDataManager.GetItemAmount(Items.EXTREME_CRYSTAL_LV1);
        lint crystal_2 = ItemDataManager.GetItemAmount(Items.EXTREME_CRYSTAL_LV2);
        int score = currData.score;
        lint targetLevel = 0;
        int amount;
        while (targetLevel < 250)//暂时最高250
        {
            targetLevel++;
            if (score / 100 >= targetLevel)
            {
                if (targetLevel > 10)
                {
                    if (targetLevel <= 30)
                    {
                        amount = targetLevel / 10;
                        if (targetLevel % 10 == 0) amount--;
                        if (crystal_1 < amount) continue;
                        allowedLevel.Add(targetLevel);
                    }
                    else
                    {
                        amount = (targetLevel - 30) / 10 + 1;
                        if (targetLevel % 10 == 0) amount--;
                        if (crystal_2 < amount) continue;
                        allowedLevel.Add(targetLevel);
                    }
                }
                else
                {
                    allowedLevel.Add(targetLevel);
                }
            }
            else
            {
                targetLevel--;
                break;
            }
        }
        InputIntBox.Show("请输入你想提升的难度等级。（需要100*等级的回溯积分，并且额外的，11~30级需要极限水晶lv1,30级以上需要极限水晶lv2）", "提示", targetLevel, (result, inputValue) =>
        {
            if (result == DialogResult.Yes)
            {
                if (!allowedLevel.Contains(inputValue))
                {
                    MessageBox.Show("你可能缺少了一些极限水晶。不能提升至该难度。", "抱歉");
                }
                else
                {
                    int needScore = inputValue * 100;
                    int needCry1 = inputValue > 10 && inputValue <= 30 ? (inputValue % 10 == 0 ? inputValue / 10 - 1 : inputValue / 10) : 0;
                    int needCry2 = inputValue > 30 ? (inputValue % 10 == 0 ? (inputValue - 30) / 10 : (inputValue - 30) / 10 + 1) : 0;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("你将需要:");
                    sb.AppendFormat("·回溯积分x{0}", needScore);
                    if (needCry1 > 0)
                    {
                        sb.AppendLine();
                        sb.AppendFormat("·极限水晶Lv.1x{0}", needCry1);
                    }
                    if (needCry2 > 0)
                    {
                        sb.AppendLine();
                        sb.AppendFormat("·极限水晶Lv.2x{0}", needCry2);
                    }
                    MessageBox.Show(sb.ToString(), "提醒", (result2) =>
                    {
                        if (result2 == DialogResult.OK)
                        {
                            SyncRequest.AppendRequest(Requests.DEEP_MEMORY_DATA, new TempDeepMemorySyncData { addScore = -needScore, setDifficulty = inputValue });
                            Dictionary<string, lint> itemToAmount = new Dictionary<string, lint>();
                            if (needCry1 > 0) itemToAmount.Add(Items.EXTREME_CRYSTAL_LV1, needCry1);
                            if (needCry2 > 0) itemToAmount.Add(Items.EXTREME_CRYSTAL_LV2, needCry2);
                            //Dictionary<string,int>
                            if (itemToAmount.Count != 0) SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(itemToAmount).ToJson(false));
                            StartCoroutine(_OpenSync());
                        }
                    }, MessageBoxButtons.OKCancel);
                }
            }
        }, MessageBoxButtons.YesNo);
    }

    IEnumerator _OpenSync()
    {
        yield return RequestBundle.RequestSyncUpdate();
        yield return _InitOrRefresh();
    }
    /// <summary>
    /// 成功击败当前层怪物可获得的积分
    /// </summary>
    /// <returns></returns>
    public int GetScoreShouldGet()
    {
        return (int)(10 * currData.currDepth * GetScoreMultiFactor(currData.currDifficulty));
    }
    /// <summary>
    /// 下次下潜时可选择的深度
    /// </summary>
    /// <returns></returns>
    public int GetInitDiveTotalScore(int targetDepth)
    {
        int n = targetDepth;
        float a1 = 10;
        float d = 10f;
        float shouldGet = (n * a1 + (n * (n - 1) * 0.5f * d)) * GetScoreMultiFactor(currData.lastDifficulty) * 0.05f;
        float factor = 5f / (currData.todayResetTimes + 5f);
        return (int)(shouldGet * factor);
    }
    /// <summary>
    /// 根据难度的敌人等级加成
    /// </summary>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    public float GetEnemyLevelMultiFactor(int difficulty)
    {
        return Mathf.Sqrt(1f + difficulty);
    }
    /// <summary>
    /// 根据难度的回溯积分
    /// </summary>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    public float GetScoreMultiFactor(int difficulty)
    {
        return Mathf.Sqrt(1f + difficulty / 2.0f);
    }
    /// <summary>
    /// 难度换算记忆浓度(0.xxxxx)
    /// </summary>
    /// <returns></returns>
    public float GetNonduByDifficulty(int difficulty)
    {
        return 1f - (60f / (difficulty + 60f));
    }
    /// <summary>
    /// 难度换算难度描述
    /// </summary>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    public string GetDifficultyDescription(int difficulty)
    {
        string result = "浅层";
        if (difficulty <= 10)
        {
            result = "浅层";
        }
        else if (difficulty <= 20)
        {
            result = "入微";
        }
        else if (difficulty <= 30)
        {
            result = "入微";
        }
        else if (difficulty <= 40)
        {
            result = "磐石";
        }
        else if (difficulty <= 50)
        {
            result = "梓木";
        }
        else if (difficulty <= 60)
        {
            result = "暮林";
        }
        else if (difficulty <= 70)
        {
            result = "山谷";
        }
        else if (difficulty <= 80)
        {
            result = "群峦";
        }
        else if (difficulty <= 90)
        {
            result = "海峡";
        }
        else if (difficulty <= 100)
        {
            result = "湍河";
        }
        else if (difficulty <= 110)
        {
            result = "江流";
        }
        else if (difficulty <= 120)
        {
            result = "大陆";
        }
        else if (difficulty <= 130)
        {
            result = "深海";
        }
        else
        {
            result = "观测";
        }
        int temp = difficulty % 10;
        if (difficulty != 0 && temp == 0)
        {
            temp = 10;
        }
        return result + TextUtils.GetLoumaNumber(temp);
    }
    public void GiveReading()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("·下潜与潜意识战斗以获得<b>回溯积分</b>，回溯积分可以更换道具等一般等价物。");
        sb.AppendLine("·一开始为0层，如果下潜最高层数超过<b>10</b>层，那么可以指定一个层数（小于等于最高层数*0.8）跳过之前层直接下潜到该层，并获得<color=lime>之前所有层数合计积分x5%(包括难度加成)</color>的回溯积分，不过当天重置次数越多，获得的积分越少。");
        sb.AppendLine("·你可以在一开始使用回溯积分和极限水晶提升本次下潜的难度。");
        sb.AppendLine("·战胜潜意识则下潜一层，无法战胜的可以重复挑战，实在战胜不了可以重置。");
        sb.Append("·每天有<b>一次</b>免费重置的机会。你也可以在商店购买重置卡进行重置。");
        MessageBox.Show(sb.ToString());
    }

    /// <summary>
    /// 回溯积分商店
    /// </summary>
    public void OpenShopWindow()
    {
        List<Transform> transforms = new List<Transform>();
        Transform trans = CommonItemPool.Spawn(CommonItemPool.SHOP_ITEM_PREFAB);
        trans.GetComponent<ShopItemGrid>().SetParam(Items.BOSS_RESET_POWDER, ShopItemGrid.WORTH_TYPE.DEEP_SCORE, 100);
        transforms.Add(trans);

        trans = CommonItemPool.Spawn(CommonItemPool.SHOP_ITEM_PREFAB);
        trans.GetComponent<ShopItemGrid>().SetParam(Items.SPB_PIECE, ShopItemGrid.WORTH_TYPE.DEEP_SCORE, 5);
        transforms.Add(trans);

        trans = CommonItemPool.Spawn(CommonItemPool.SHOP_ITEM_PREFAB);
        trans.GetComponent<ShopItemGrid>().SetParam("dm_resetcard", ShopItemGrid.WORTH_TYPE.DEEP_SCORE, 5000);
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
         }, "回溯商店", "使用回溯积分兑换/购买物品。");
    }
}
namespace SerializedClassForJson
{
    [System.Serializable]
    public class TempDeepMemoryData
    {
        public int score;
        public int currDepth;
        public int maxDepth;
        public int currDifficulty;
        public int lastDifficulty;
        public int todayResetTimes;
        public bool freeResetUsed = true;
    }
    [System.Serializable]
    public class TempDeepMemorySyncData
    {
        public int addDepth;//加深度
        public int addScore;//加回溯积分
        public bool setReset;//设置重置
        public int setDifficulty;//设置难度
    }
}
