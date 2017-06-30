using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SerializedClassForJson;

[System.Serializable]
public class EquipmentDropBundle
{
    public List<EquipmentRandInfo> randInfos;

    /// <summary>
    /// 用于显示掉落的字符串
    /// </summary>
    public string GetDropInfoToolTip()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(TextUtils.GetSizedString("特殊掉落", 18));
        foreach (EquipmentRandInfo randInfo in randInfos)
        {
            sb.Append("<color=#C2E2E8FF><b>");
            sb.Append(((randInfo.dropChance * 100).ToString("0.0")));
            sb.Append("</b></color>");
            sb.Append("%获得灵基值约");
            sb.Append(Mathf.RoundToInt((randInfo.minValue + randInfo.maxValue) * 0.5f));
            sb.Append("的");
            sb.AppendLine(EquipmentBase.GetName(EquipmentBase.GetEqTypeNameByType(randInfo.type),randInfo.quality));
        }
        if (randInfos.Count == 0)
        {
            sb.Append("无");
        }
        return sb.ToString();
    }

    /// <summary>
    /// 随机生成灵基已定的武器生成请求,游戏最多能生成15个请求,不能超过装备数量最大值
    /// </summary>
    /// <param name="times">随机生成的次数</param>
    /// <param name="chanceMult">每次生成的概率补正系数</param>
    /// <returns></returns>
    public TempRandEquipRequest[] CreateSpecRequests(int times, float chanceMult)
    {
        List<TempRandEquipRequest> requests = new List<TempRandEquipRequest>();
        int nowEqAmount = PlayerInfoInGame.GetEquipmentAmount();
        for (int a = 0; a < times; a++)
        {
            foreach (EquipmentRandInfo randInfo in randInfos)
            {
                if (Random.value <= randInfo.dropChance * chanceMult && nowEqAmount <= PlayerInfoInGame.MAX_EQUIPMENT_IN_PACK)
                {
                    if (requests.Count >= 15)
                    {
                        return requests.ToArray();
                    }
                    TempRandEquipRequest request = new TempRandEquipRequest();
                    request.eqQuality = (int)randInfo.quality;
                    request.eqType = (int)randInfo.type;
                    request.value = Random.Range(randInfo.minValue, randInfo.maxValue);
                    int amount = 0;
                    for (int i = 0; i < randInfo.prefixChanceForEach.Length; i++)
                    {
                        if (Random.value <= randInfo.prefixChanceForEach[i])
                        {
                            amount++;
                        }
                    }
                    request.prefixAmount = amount;
                    requests.Add(request);
                    nowEqAmount++;
                }
            }
        }
        return requests.Count == 0 ? null : requests.ToArray();
    }
}
/// <summary>
/// 记载每个可随机的装备记录
/// </summary>
[System.Serializable]
public class EquipmentRandInfo
{
    [Tooltip("装备类型")]
    public EQ_TYPE type;//装备类型
    [Tooltip("装备品质")]
    public EQ_QUALITY quality;//装备等阶
    [Tooltip("掉落的概率")]
    public float dropChance;//机会
    public float minValue;//最小灵基值
    public float maxValue;//最大灵基值
    [Tooltip("获得该装备的机会")]
    public float[] prefixChanceForEach;//最多可以有几个词缀
}