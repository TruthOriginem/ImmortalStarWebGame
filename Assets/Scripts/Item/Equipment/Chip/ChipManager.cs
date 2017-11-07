using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson.Chip;
using UnityEngine;
using GameId;
using SerializedClassForJson;

/// <summary>
/// 芯片管理类
/// </summary>
public class ChipManager : MonoBehaviour
{
    private const string GET_CHIP_PATH = "scripts/player/equipment/chip/getAllChips.php";
    public const float BASE_DROP_POSSIBILITY = 0.001f;
    public const float BASE_ALLOCATE_ATTRFACTOR_MIN = 0.03f;
    public const float BASE_ALLOCATE_ATTRFACTOR_MAX = 0.05f;
    public static ChipManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 返回一个进行中的更新所有芯片数据的协程。
    /// </summary>
    /// <returns></returns>
    public static Coroutine RequestGetAllChipsData()
    {
        return Instance.StartCoroutine(_GetAllChipsData());
    }
    public static Coroutine RequestInstallChip(ChipData data, string equip_id)
    {
        return Instance.StartCoroutine(_InstallChip(data, equip_id));
    }
    public static Coroutine RequestUnstallChip(ChipData data)
    {
        return Instance.StartCoroutine(_UnstallChip(data));
    }
    public static Coroutine RequestUpgradeChip(ChipData data)
    {
        return Instance.StartCoroutine(_UpgradeChip(data));
    }
    public static Coroutine RequestDropChip(ChipData data)
    {
        return Instance.StartCoroutine(_DropChip(data));
    }
    static IEnumerator _GetAllChipsData()
    {
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        WWW w = new WWW(CU.ParsePath(GET_CHIP_PATH), form);
        CU.ShowConnectingUI();
        yield return w;
        CU.HideConnectingUI();
        if (CU.IsPostSucceed(w))
        {
            TempChipRawData[] rawDatas = JsonHelper.GetJsonArray<TempChipRawData>(w.text);
            var newChips = new List<ChipData>();
            if (!EArray.IsNullOrEmpty(rawDatas))
            {
                for (int i = 0; i < rawDatas.Length; i++)
                {
                    newChips.Add(new ChipData(rawDatas[i]));
                }
            }
            PlayerInfoInGame.CurrentChips = newChips;
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
    }
    static IEnumerator _InstallChip(ChipData data, string equip_id)
    {
        TempChipRelativeData change = new TempChipRelativeData();
        change.setEquipId = equip_id;
        change.chipId = data.GetId();
        SyncRequest.AppendRequest(Requests.CHIP_HANDLER_DATA, change);
        yield return RequestBundle.RequestSyncUpdate(false);
    }
    static IEnumerator _UnstallChip(ChipData data)
    {
        TempChipRelativeData change = new TempChipRelativeData();
        change.setEquipId = "-1";
        change.chipId = data.GetId();
        change.addedLevel = -data.GetLevel() / 2;
        change.addedKillAmounts = -data.GetKillAmounts();
        SyncRequest.AppendRequest(Requests.CHIP_HANDLER_DATA, change);
        yield return RequestBundle.RequestSyncUpdate(false);
    }
    static IEnumerator _UpgradeChip(ChipData data)
    {
        TempChipRelativeData change = new TempChipRelativeData();
        change.chipId = data.GetId();
        change.addedLevel = 1;
        change.addedKillAmounts = -data.GetNextLevelNeedKillAmounts();
        SyncRequest.AppendRequest(Requests.CHIP_HANDLER_DATA, change);
        SyncRequest.AppendRequest(Requests.ITEM_DATA, new IIABinds(Items.SPB_PIECE, data.GetNextLevelNeedSpbPieces()).ToJson(false));
        yield return RequestBundle.RequestSyncUpdate(false);
    }
    static IEnumerator _DropChip(ChipData data)
    {
        TempChipRelativeData change = new TempChipRelativeData();
        change.chipId = data.GetId();
        change.deleteIt = true;
        SyncRequest.AppendRequest(Requests.CHIP_HANDLER_DATA, change);
        yield return RequestBundle.RequestSyncUpdate(false);
    }
    /// <summary>
    /// 根据给定信息随机生成芯片数据，如果未能生成成功则返回只有其他加经验的。
    /// </summary>
    /// <param name="gridData"></param>
    /// <param name="actualSpawnData"></param>
    /// <returns></returns>
    public static TempChipRandGenData GenerateRandomDataByBattle(BatInsGridData gridData, EnemySpawnData actualSpawnData = null)
    {
        TempChipRandGenData genData = new TempChipRandGenData();
        genData.addOthersKillAmounts = actualSpawnData.GetEnemiesCount();
        if (gridData == null)
        {
            return genData;
        }
        var stage = gridData.GetParentStageData();
        lint exLevel = stage.ExtremeLevel;
        int tier = GetMaxTierByExLevel(exLevel);
        //Debug.Log(tier);
        if (tier == 0) return genData;
        EnemySpawnData enemySpawnData = actualSpawnData;
        if (actualSpawnData == null) enemySpawnData = gridData.enemys.GetActualData(stage.sId);
        float actualPoss = GetActualDropPossibility(enemySpawnData);
        //如果随机失败则返回空
        //Debug.Log(actualPoss);
        if (Random.value > actualPoss) return genData;
        //生成数据
        //随机稀有度
        WeightedRandomPicker<int> tierPicker = new WeightedRandomPicker<int>();
        for (int i = 1; i <= tier; i++)
        {
            if (tier == 1)
            {
                tierPicker.Add(1, 100f);
            }
            else if (tier == 2)
            {
                tierPicker.Add(2, 50f);
            }
            else if (tier == 3)
            {
                tierPicker.Add(3, 30f);
            }
            else if (tier == 4)
            {
                tierPicker.Add(4, 15f);
            }
            else if (tier == 5)
            {
                tierPicker.Add(5, 5f);
            }
        }
        tier = tierPicker.Pick();
        //随机效果，但目前只有属性加成
        int effectId = Effects.ATTR;
        string effectGenInfo = GenerateEffectGenInfo(effectId);
        genData.effectGenInfo = effectGenInfo;
        genData.effectId = effectId;
        genData.starRarity = tier;
        return genData;
    }
    /// <summary>
    /// 直接根据指定Tier生成一个随机芯片。
    /// </summary>
    /// <param name="tier"></param>
    /// <returns></returns>
    public static TempChipRandGenData GenerateRandomDataByTargetTier(int tier,int effectId = Effects.ATTR)
    {
        var data = new TempChipRandGenData()
        {
            effectId = effectId,
            effectGenInfo = GenerateEffectGenInfo(effectId),
            starRarity = tier
        };

        return data;
    }
    /// <summary>
    /// 通过怪物组获得当前掉落芯片概率
    /// </summary>
    /// <param name="actualSpawnData"></param>
    /// <returns></returns>
    public static float GetActualDropPossibility(EnemySpawnData actualSpawnData)
    {
        float actualPoss = BASE_DROP_POSSIBILITY * BattleAwardMult.GetDropMult();
        int enemyAmount = 0;
        int totalLevel = 0;
        for (int i = 0; i < actualSpawnData.enemyGroups.Count; i++)
        {
            var group = actualSpawnData.enemyGroups[i];
            enemyAmount += group.amount;
            totalLevel += group.enemy.Level;
        }
        float averageEnemyLevel = totalLevel / (float)enemyAmount;
        float limitLevel = PlayerInfoInGame.Level * 0.33f;
        if (limitLevel > averageEnemyLevel)
        {
            //最低降至原本概率的0.1倍
            actualPoss *= (1f - 0.9f * ((limitLevel - averageEnemyLevel) / limitLevel));
        }
        return actualPoss;
    }
    /// <summary>
    /// 通过关卡，极限等级获得最高能得到的芯片稀有度
    /// </summary>
    /// <param name="extreLevel"></param>
    /// <returns></returns>
    public static int GetMaxTierByExLevel(lint extreLevel)
    {
        lint exLevel = extreLevel;
        int tier = 1;
        if (exLevel >= 20)
        {
            tier = 5;
        }
        else if (exLevel >= 10)
        {
            tier = 4;
        }
        else if (exLevel >= 5)
        {
            tier = 3;
        }
        else if (exLevel >= 2)
        {
            tier = 2;
        }
        else if (exLevel >= 1)
        {
            tier = 1;
        }
        else
        {
            tier = 0;
        }
        return tier;
    }
    /// <summary>
    /// 获得指定芯片效果的芯片生成信息。
    /// </summary>
    /// <param name="effectId"></param>
    /// <returns></returns>
    static string GenerateEffectGenInfo(int effectId)
    {
        switch (effectId)
        {
            case Effects.ATTR:
                #region ATTR
                //随机选有几个属性躺枪，1~3个
                int attrAmounts = Random.Range(1, 4);
                //属性合计可以分配的属性
                float allocateAttrValue = Random.Range(BASE_ALLOCATE_ATTRFACTOR_MIN, BASE_ALLOCATE_ATTRFACTOR_MAX);
                float[] attrValues = new float[attrAmounts];
                if (attrAmounts != 1)
                {
                    //当前分配平均值
                    float avgtmp = allocateAttrValue / attrAmounts;
                    float totaltmp = 0;
                    for (int i = 0; i < attrAmounts; i++)
                    {
                        attrValues[i] = Random.Range(-avgtmp, avgtmp);
                        totaltmp += attrValues[i];
                    }
                    //获得当前分配属性与目标分配属性的平均差
                    float offseteachtmp = (allocateAttrValue - totaltmp) / attrAmounts;
                    for (int i = 0; i < attrAmounts; i++)
                    {
                        attrValues[i] += offseteachtmp;
                        attrValues[i] = EMath.Round(attrValues[i], 4);
                        if (attrValues[i] < 0)
                        {
                            attrValues[i] = 0;
                        }
                    }
                }
                else
                {
                    attrValues[0] = allocateAttrValue;
                    attrValues[0] = EMath.Round(attrValues[0], 4);
                }
                //随机选取，分配属性
                //属性选取权重
                WeightedRandomPicker<Attr> picker = new WeightedRandomPicker<Attr>();
                picker.AddAll(AttributeCollection.GetAllAttributes());
                Dictionary<string, float> dic = new Dictionary<string, float>();
                for (int i = 0; i < attrAmounts; i++)
                {
                    if (attrValues[i] != 0)
                    {
                        var attr = picker.PickAndRemove();
                        dic.Add(attr.Id, attrValues[i]);
                    }
                }
                return EDictionary.SerializeToJson(dic);
            #endregion
            default:
                return "";
        }
    }
}
