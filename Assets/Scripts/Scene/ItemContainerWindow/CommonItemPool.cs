using PathologicalGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonItemPool : MonoBehaviour
{
    [SerializeField]
    private SpawnPool spawnPool;

    public const string SHOP_ITEM_PREFAB = "itemSell";
    public static CommonItemPool Instance { get; set; }
    private static List<Transform> spawnedTrans = new List<Transform>();

    void Awake()
    {
        Instance = this;
    }
    /*
    void Update()
    {

    }*/
    public static Transform Spawn(string prefabId)
    {
        var ins = Instance.spawnPool.Spawn(prefabId, Instance.transform);
        spawnedTrans.Add(ins);
        return ins;
    }
    /// <summary>
    /// 一次性回收并禁用所有由该池生成的对象体。
    /// </summary>
    public static void RecycleAll()
    {
        for (int i = 0; i < spawnedTrans.Count; i++)
        {
            var transform = spawnedTrans[i];
            transform.SetParent(Instance.transform, false);
        }
        Instance.spawnPool.DespawnAll();
        spawnedTrans.Clear();
    }
}
