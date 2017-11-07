using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson;
using System.Linq;
using System.Text;
using GameId;
using MachineMatchJson;

/// <summary>
/// 战斗结束后生成的结算类，用于记载各种数据
/// </summary>
public class BattleResult
{
    public object linkedInfo;
    /// <summary>
    /// 构造该类时，根据战斗结果向数据库发送信息。
    /// </summary>
    /// <param name="playersRecord"></param>
    public BattleResult()
    {

    }
    /// <summary>
    /// 设置该战斗结果相关的实例。生成结果时根据这个实例做变动。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="linkedInfo"></param>
    public void SetLinkedInfo<T>(T linkedInfo)
    {
        this.linkedInfo = linkedInfo;
    }

    /// <summary>
    /// 通过grid生成结算奖励
    /// </summary>
    /// <param name="data">为null时代表失败</param>
    public void GainResult(BatInsGridData grid, int lostTime)
    {
        PlayerInfoInGame.Instance.StartCoroutine(GainResultCor(grid, lostTime));
    }
    /// <summary>
    /// 通过linkedInfo生成结算奖励
    /// </summary>
    /// <param name="win"></param>
    public void GainResultByLinkedInfo(bool win)
    {
        PlayerInfoInGame.Instance.StartCoroutine(GainResultCor(win));
    }
    /// <summary>
    /// 普通关卡战斗的结果
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="lostTime"></param>
    /// <returns></returns>
    IEnumerator GainResultCor(BatInsGridData grid, int lostTime)
    {
        //yield return PlayerInfoInGame.Instance.RequestUpdatePlayerItems();
        TempPlayerAttribute pattr = new TempPlayerAttribute();
        TempRandEquipRequest[] requests = null;
        TempLigRecord record = null;
        IIABinds binds = null;
        if (grid != null)
        {
            EnemySpawnData data = grid.enemys.GetActualData(grid.sId);
            long totalExp;//经验总计
            int money;//金钱统计
            Dictionary<string, lint> idsToAmount = GenerateResultsDict(data, true, true, 1, out money, out totalExp);
            StringBuilder sb = new StringBuilder();
            requests = grid.eqDrop.CreateSpecRequests(1, BattleAwardMult.GetDropMult());
            if (requests != null)
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    sb.Append("获得了灵基为");
                    sb.Append(requests[i].value.ToString("0.0"));
                    sb.Append("的");
                    sb.AppendLine(EquipmentBase.GetEqTypeNameByType((EQ_TYPE)requests[i].eqType));
                }
                SyncRequest.AppendRequest(Requests.RND_EQ_GENA_DATA, TempRandEquipRequest.GenerateJsonArray(requests));
            }
            var genData = ChipManager.GenerateRandomDataByBattle(grid, data);
            //ChipUIManager.rawDatas.Add(gen)
            if (genData != null)
            {
                if (genData.starRarity > 0)
                {
                    sb.AppendFormat("获得了一个稀有度为T{0}的芯片！", genData.starRarity);
                    sb.AppendLine();
                }
                //如果没有获得芯片且当前玩家不存在芯片的话，那么就不发送这次的信息了
                if (!(genData.starRarity == 0 && PlayerInfoInGame.CurrentChips.Count == 0))
                {
                    SyncRequest.AppendRequest(Requests.CHIP_HANDLER_DATA, genData);
                }
            }
            //双倍卡
            if (ItemDataManager.GetItemAmount(Items.CARD_EXP_DOUBLE) >= 1)
            {
                totalExp *= 2;
                idsToAmount.Add(Items.CARD_EXP_DOUBLE, -1);
                sb.Append("使用了");
                sb.AppendLine(ItemDataManager.GetItemName(Items.CARD_EXP_DOUBLE));
            }
            if (ItemDataManager.GetItemAmount(Items.CARD_DROP_DOUBLE) >= 1)
            {
                var temp = new Dictionary<string, lint>(idsToAmount);
                foreach (var key in temp.Keys)
                {
                    idsToAmount[key] = temp[key] * 2;
                }
                idsToAmount.Add(Items.CARD_DROP_DOUBLE, -1);
                sb.Append("使用了");
                sb.AppendLine(ItemDataManager.GetItemName(Items.CARD_DROP_DOUBLE));
            }
            sb.AppendLine("获得星币 " + TextUtils.GetMoneyText(money));
            sb.AppendLine("获得经验 " + TextUtils.GetExpText(totalExp));
            string[] items_ids = idsToAmount.Keys.ToArray();//里面不存在，在更新道具的时候
            lint[] amounts = idsToAmount.Values.ToArray();
            for (int i = 0; i < items_ids.Length; i++)
            {
                if (amounts[i] > 0) sb.AppendLine(ItemDataManager.GetItemName(items_ids[i]) + " x " + amounts[i]);
            }
            binds = new IIABinds(items_ids, amounts);
            pattr.money += money;
            pattr.exp += totalExp;
            //记录最后一次胜利
            record = new TempLigRecord();
            record.lig_id = grid.id;
            record.lig_lostTime = lostTime;
            record.SetHangFinish();


            //
            //如果不是能无限攻击的关卡，则不会记录在数据库最record里
            if (grid.limit.attackTimesPerDay != -1)
            {
                record.lig_dontRecord = true;
            }
            SyncRequest.AppendRequest(Requests.RECORD_DATA, record);
            SyncRequest.AppendRequest(Requests.ITEM_DATA, binds.ToJson(false));
            SyncRequest.AppendRequest(Requests.PLAYER_DATA, pattr);


            BattleManager.ResultString = sb.ToString();
        }
        else
        {
            BattleManager.ResultString = "失败。";
        }

        yield return RequestBundle.RequestSyncUpdate();
        yield return BattleInstanceManager.Instance.RefreshAllGrids();
    }
    IEnumerator GainResultCor(bool win)
    {
        if (linkedInfo == null)
        {
            yield break;
        }
        //yield return PlayerInfoInGame.Instance.RequestUpdatePlayerItems();
        TempPlayerAttribute pattr = new TempPlayerAttribute();
        IIABinds binds = null;

        #region 远征
        if (linkedInfo is ExpeditionBattleInfo)
        {
            var info = linkedInfo as ExpeditionBattleInfo;
            if (win)
            {
                EnemySpawnData data = info.enemySpawnData;
                //掉落
                Dictionary<string, lint> idsToAmount = GenerateExpeditionResultsDict(data, info);

                StringBuilder sb = new StringBuilder();
                string[] items_ids = idsToAmount.Keys.ToArray();//里面不存在，在更新道具的时候
                lint[] amounts = idsToAmount.Values.ToArray();
                for (int i = 0; i < items_ids.Length; i++)
                {
                    if (amounts[i] > 0) sb.AppendLine(ItemDataManager.GetItemName(items_ids[i]) + " x " + amounts[i]);
                }
                binds = new IIABinds(items_ids, amounts);
                pattr.money -= info.moneyToBePaied;
                //Debug.Log("损失的：" + pattr.money);
                TempPlayerExpeditionInfo expeInfo = new TempPlayerExpeditionInfo();
                expeInfo.nowLightYear = info.targetLightYear;
                expeInfo.maxLightYear = info.targetLightYear > info.nowMaxLightYear ? info.targetLightYear : info.nowMaxLightYear;
                expeInfo.ifEscaped = false;
                BattleManager.ResultString = sb.ToString();
                //Debug.Log(expeInfo.nowLightYear);
                yield return RequestBundle.RequestUpdateRecord(expeInfo, binds, pattr);
            }
            else
            {
                BattleManager.ResultString = "失败";
                pattr.money -= info.moneyToBePaied;
                TempPlayerExpeditionInfo expeInfo = new TempPlayerExpeditionInfo();
                expeInfo.nowLightYear = 0;
                expeInfo.maxLightYear = info.nowMaxLightYear;
                yield return RequestBundle.RequestUpdateRecord(expeInfo, null, pattr);
            }
        }
        #endregion
        #region 机械擂台
        if (linkedInfo is MachineMatchManager)
        {
            BattleManager.ResultString = win ? "胜利。" : "失败。";
            TempMMSyncInfo info = new TempMMSyncInfo();
            info.addScore = win ? 3 : 1;
            info.addChall = 1;
            SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(Items.MM_TICKET, -1).ToJson(false));
            SyncRequest.AppendRequest(Requests.MACHINE_MATCH_DATA, info);
            yield return RequestBundle.RequestSyncUpdate();
        }
        #endregion
        if (linkedInfo is DeepMemoryManager)
        {
            DeepMemoryManager manager = linkedInfo as DeepMemoryManager;
            if (win)
            {
                int score = manager.GetScoreShouldGet();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("胜利！");
                if (ItemDataManager.GetItemAmount(Items.CARD_DM_DOUBLE) > 0)
                {
                    score *= 2;
                    sb.AppendLine("使用了双倍卡，获得积分翻倍。");
                    SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(Items.CARD_DM_DOUBLE, -1).ToJson(false));
                }
                TempDeepMemorySyncData data = new TempDeepMemorySyncData
                {
                    addDepth = 1,
                    addScore = score
                };
                SyncRequest.AppendRequest(Requests.DEEP_MEMORY_DATA, data);
                yield return RequestBundle.RequestSyncUpdate();
                sb.AppendFormat("获得{0}回溯积分并成功下潜一层。", score);
                BattleManager.ResultString = sb.ToString();
            }
            else
            {
                BattleManager.ResultString = "失败...";
            }
        }
    }


    /// <summary>
    /// 生成消灭一个怪物组会带来的结果，不计算生命扣除。以道具id-数量的字典返回。
    /// </summary>
    /// <param name="data">怪物组</param>
    /// <param name="doRandom">是否生成随机，否则取平均值(用于显示说明)</param>
    /// <param name="countMult">是否计算额外的加成,@BattleAwardMult</param>
    /// <param name="times">money，exp的倍数,用于计算挂机</param>
    /// <param name="money"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    public static Dictionary<string, lint> GenerateResultsDict(EnemySpawnData data, bool doRandom, bool countMult, int times, out int money, out long exp)
    {
        if (data == null)
        {
            exp = 0;
            money = 0;
            return null;
        }
        Dictionary<string, lint> idsToAmount = new Dictionary<string, lint>();
        exp = 0;//经验总计
        foreach (EnemyGroup group in data.enemyGroups)
        {
            int e_amount = group.amount;
            int e_level = group.enemy.Level;
            string e_id = group.enemy.id;
            EnemyAttribute attr = EnemyDataManager.AskForEnemyAttribute(e_id);
            long oneGroupExp = e_amount * (attr.baseP.exp + attr.growth.exp * (e_level - 1));
            exp += times * oneGroupExp;
            for (int i = 0; i < attr.dropItems.Length; i++)
            {
                TempItemDrops itemDrop = attr.dropItems[i];
                //怪物等级大于掉落道具需求等级才有效
                if (e_level >= itemDrop.needLevel)
                {
                    float total = times * e_amount * itemDrop.amount * (1f + itemDrop.multLevel * (e_level - itemDrop.needLevel));
                    //Debug.Log(total);
                    if (!itemDrop.id.Equals(Items.MONEY))
                    {
                        total *= countMult ? BattleAwardMult.GetDropMult() : 1f;
                    }
                    float number;//最低和最高的浮点数
                    if (doRandom)
                    {
                        number = Random.Range(total * itemDrop.chance, total);
                    }
                    else
                    {
                        number = Mathf.Round(total * (0.5f + 0.5f * itemDrop.chance));
                    }
                    int amount = (int)number;//比如number = 0.9，amount = 0，下面就有有90%的可能性
                    if (Random.value < (number - amount))
                    {
                        amount++;
                    }
                    if (idsToAmount.ContainsKey(itemDrop.id))
                    {
                        idsToAmount[itemDrop.id] += amount;
                    }
                    else
                    {
                        idsToAmount.Add(itemDrop.id, amount);
                    }
                }
            }
        }
        if (idsToAmount.ContainsKey(Items.MONEY))
        {
            money = idsToAmount["money"];
            money = Mathf.RoundToInt(money * (countMult ? BattleAwardMult.GetMoneyMult() : 1f));
            idsToAmount.Remove("money");
        }
        else
        {
            money = 0;
        }
        exp = (long)(exp * (countMult ? BattleAwardMult.GetExpMult() : 1));
        return idsToAmount;
    }
    /// <summary>
    /// 生成远征消灭一个怪物组会带来的结果，不计算生命扣除。以道具id-数量的字典返回。
    /// </summary>
    /// <param name="data">怪物组</param>
    /// <returns></returns>
    public static Dictionary<string, lint> GenerateExpeditionResultsDict(EnemySpawnData data, ExpeditionBattleInfo info)
    {

        Dictionary<string, lint> idsToAmount = new Dictionary<string, lint>();
        foreach (EnemyGroup group in data.enemyGroups)
        {
            int e_amount = group.amount;
            int e_level = group.enemy.Level;
            string e_id = group.enemy.id;
            EnemyAttribute attr = EnemyDataManager.AskForEnemyAttribute(e_id);
            for (int i = 0; i < attr.dropItems.Length; i++)
            {
                TempItemDrops itemDrop = attr.dropItems[i];
                //怪物等级大于掉落道具需求等级才有效
                if (e_level >= itemDrop.needLevel)
                {
                    float most = 1f * e_amount * itemDrop.amount * (1f + itemDrop.multLevel * (e_level - itemDrop.needLevel));
                    most *= BattleAwardMult.GetExpeditionDropMult();//使用远征的专属物品掉率
                    float number;//最低和最高的浮点数

                    number = Mathf.Round(most * (0.5f + 0.5f * itemDrop.chance));

                    int amount = (int)number;//比如number = 0.9，amount = 0，下面就有有90%的可能性
                    if (Random.value < (number - amount))
                    {
                        amount++;
                    }
                    if (idsToAmount.ContainsKey(itemDrop.id))
                    {
                        idsToAmount[itemDrop.id] += amount;
                    }
                    else
                    {
                        idsToAmount.Add(itemDrop.id, amount);
                    }
                }
            }
        }
        ExpeditionManager.AddExepSpecItemsToDict(idsToAmount, info);
        if (idsToAmount.ContainsKey(Items.MONEY)) idsToAmount.Remove(Items.MONEY);
        return idsToAmount;
    }
}
