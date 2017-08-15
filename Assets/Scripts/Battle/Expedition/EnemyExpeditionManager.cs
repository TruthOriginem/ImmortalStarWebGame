using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EnemyExpeditionManager : MonoBehaviour
{
    public static EnemyExpeditionManager Instance { get; set; }
    private static string FILEPATH = "scripts/modules/datas/expedition_enemy_picker.php";
    private static TempEnemyExpeditionInfo[] infos = null;
    void Awake()
    {
        Instance = this;
    }
    public EnemySpawnData GenerateEnemySpawn(int lightYear)
    {
        //换算成目标等级
        int targetLevel = 1 + lightYear / ExpeditionManager.LIGHTYEAR_TO_DIFFICULTY_SCALE;
        //首先生成可能pick的怪物id
        WeightedRandomPicker<string> picker = new WeightedRandomPicker<string>();
        Dictionary<string, float> idToWeight = new Dictionary<string, float>();
        for (int i = 0; i < infos.Length; i++)
        {
            var info = infos[i];
            var needs = info.lvr;
            var prs = info.pr;
            if (targetLevel >= needs[0] && (targetLevel <= needs[1] || needs[1] == -1))
            {
                float weight = (1f - (targetLevel - needs[0]) / (needs[1] - needs[0])) * (prs[0] - prs[1]);
                weight = needs[1] == -1 ? prs[0] : weight;

                if (idToWeight.ContainsKey(info.id))
                {
                    idToWeight[info.id] += weight;
                }
                else
                {
                    idToWeight.Add(info.id, weight);
                }
            }
        }
        foreach (var kv in idToWeight)
        {
            picker.Add(kv.Key, kv.Value);
        }
        Dictionary<string, int> idToAmount = new Dictionary<string, int>();
        int totalPick = 1;
        var pick = picker.Pick();
        idToAmount.Add(pick, 1);
        for (int i = 0; i < 3; i++)
        {
            if (Random.value < 0.2f)
            {
                totalPick++;
                var p = picker.Pick();
                if (idToAmount.ContainsKey(p))
                {
                    idToAmount[p] = idToAmount[p] + 1;
                }
                else
                {
                    idToAmount.Add(p, 1);
                }
            }
        }
        EnemySpawnData data = new EnemySpawnData();
        data.enemyGroups = new List<EnemyGroup>();
        int levelDown = 0;
        levelDown = targetLevel / 10;
        foreach (var kv in idToAmount)
        {
            BaseEnemy enemy = new BaseEnemy();
            enemy.id = kv.Key;
            enemy.Level = targetLevel - (totalPick - 1) * levelDown;
            EnemyGroup group = new EnemyGroup();
            group.enemy = enemy;
            group.amount = kv.Value;
            data.enemyGroups.Add(group);
        }
        return data;
    }
    /// <summary>
    /// 生成此次前进可能会遇见的怪物。
    /// </summary>
    /// <param name="lightYear"></param>
    /// <returns></returns>
    public string GenearatePossibleEnemyText(int lightYear)
    {
        int targetLevel = 1 + lightYear / ExpeditionManager.LIGHTYEAR_TO_DIFFICULTY_SCALE;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < infos.Length; i++)
        {
            var info = infos[i];
            var needs = info.lvr;
            if (targetLevel >= needs[0] && (targetLevel <= needs[1] || needs[1] == -1))
            {
                var attr = EnemyDataManager.AskForEnemyAttribute(info.id);
                sb.Append(attr.name);
                sb.Append(',');
            }
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
    public static Coroutine InitEnemyExpeditionInfos()
    {
        return Instance.StartCoroutine(Instance.LoadEnemyExpeditionInfos());
    }
    /// <summary>
    /// 下载相关信息
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadEnemyExpeditionInfos()
    {
        if (infos == null)
        {

            CU.ShowConnectingUI();
            WWW w = new WWW(CU.ParsePath(FILEPATH));
            yield return w;
            if (CU.IsPostSucceed(w))
            {
                //Debug.Log(w.text);
                infos = JsonHelper.GetJsonArray<TempEnemyExpeditionInfo>(w.text);
                /*
                for (int i = 0; i < infos.Length; i++)
                {
                    var info = infos[i];
                    Debug.Log(info.lvr[0]);
                }
                */
            }
            else
            {
                CU.ShowConnectFailed();

                yield break;
            }
            CU.HideConnectingUI();
            //yield return new WaitForSeconds(3);
            //Debug.Log(GenearatePossibleEnemyText(PlayerInfoInGame.Level));
        }
    }
}
