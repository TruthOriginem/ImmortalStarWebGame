﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerializedClassForJson;
using GameId;

/// <summary>
/// 用于管理从网站上获得的怪物资料，以及一些管理
/// <para>获得怪物的EnemyAttribute，通过id来获取具体信息</para>
/// </summary>
public class EnemyDataManager : MonoBehaviour
{

    public static EnemyDataManager Instance { get; set; }

    //private static List<string> enemyIds = new List<string>();
    private static Dictionary<string, EnemyAttribute> idToEnmeyAttribute = new Dictionary<string, EnemyAttribute>();

    private static string LOAD_ENEMY_ATTR_PATH = "scripts/battle/enemy/loadEnemyAttr.php";
    private static string LOAD_ENEMIES_PATH = "scripts/battle/enemy/loadEnemies.php";

    static EnemyDataManager()
    {

    }

    void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 初始化所有怪物
    /// </summary>
    /// <returns></returns>
    public static IEnumerator InitAllEnemiesInList()
    {
        CU.ShowConnectingUI();
        WWW w = new WWW(CU.ParsePath(LOAD_ENEMIES_PATH));
        yield return w;
        if (CU.IsDownloadCompleted(w))
        {
            TempEnemyAttribute[] attrs = JsonHelper.GetJsonArray<TempEnemyAttribute>(w.text);
            for (int i = 0; i < attrs.Length; i++)
            {
                var attr = attrs[i];
                if (IfEnemyAttributeDownloaded(attr.id))
                {
                    continue;
                }
                EnemyProperty baseP = new EnemyProperty();

                baseP.Update(attr);
                EnemyProperty growP = new EnemyProperty();

                growP.Update(attr.growAttr);
                Texture2D iconTexture = null;
                WWW wIcon = new WWW(CU.ParsePath(EnemyAttribute.GetCompletedFilePathById(attr.iconName)));
                yield return wIcon;
                if (CU.IsDownloadCompleted(wIcon))
                {
                    iconTexture = wIcon.texture;
                }
                else
                {
                    yield break;
                }
                iconTexture.Compress(true);
                EnemyAttribute eAttr = new EnemyAttribute(attr.idkey, attr.id, attr.name,attr.description, baseP, growP, iconTexture, attr.dropItems);
                idToEnmeyAttribute.Add(attr.id, eAttr);
            }
        }
        else
        {
            CU.HideConnectingUI();
            yield break;
        }
        CU.HideConnectingUI();
    }

    /// <summary>
    /// 请求向服务器获得并加载指定id的怪物属性。
    /// </summary>
    /// <param name="id">敌人指定id</param>
    /// <param name="refresh">是否重新加载某种敌人的属性</param>
	public static EnemyAttribute AskForEnemyAttribute(string id)
    {
        return Instance.Ask(id);
    }
    public EnemyAttribute Ask(string id)
    {
        if (idToEnmeyAttribute.ContainsKey(id))
        {
            return idToEnmeyAttribute[id];
        }
        else
        {   /*
            //先设置为null
            idToEnmeyAttribute.Add(id, null);
            //开始尼玛的下载
            StartCoroutine(DownLoadEnemyAttribute(id));
            */
            return null;
        }
    }

    /// <summary>
    /// 下载敌人基础信息的协程，全部下载完毕后才可以得到该敌人信息，才可以和相关敌人战斗
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEnumerator DownLoadEnemyAttribute(string id)
    {
        CU.ShowConnectingUI();
        EnemyProperty baseP = new EnemyProperty();
        EnemyProperty growP = new EnemyProperty();
        TempEnemyAttribute tempAttr = null;//除了有基本属性还记录了名字，描述等
        Texture2D iconTexture = null;
        #region 第一阶段，获得基础属性
        WWWForm fb = new WWWForm();
        fb.AddField("id", id);
        fb.AddField("type", 0);
        WWW w1 = new WWW(CU.ParsePath(LOAD_ENEMY_ATTR_PATH), fb);
        yield return w1;
        if (w1.isDone && w1.text != "failed")
        {
            string jsonText = w1.text;
            tempAttr = JsonUtility.FromJson<TempEnemyAttribute>(jsonText);
            baseP.Update(tempAttr);
        }
        else
        {
            Debug.LogWarning(w1.text);
            CU.HideConnectingUI();
            yield break;
        }
        #endregion
        if (tempAttr == null)
        {
            yield break;
        }
        #region 第二阶段，获得成长属性
        WWWForm fb2 = new WWWForm();
        fb2.AddField("id", id);
        fb2.AddField("type", 1);
        WWW w2 = new WWW(CU.ParsePath(LOAD_ENEMY_ATTR_PATH), fb2);
        yield return w2;
        if (w2.isDone && w2.text != "failed")
        {
            string jsonText = w2.text;
            //只需要记录一般属性即可
            TempEnemyAttribute temp2 = JsonUtility.FromJson<TempEnemyAttribute>(jsonText);
            growP.Update(temp2);
        }
        else
        {
            CU.HideConnectingUI();
            CU.ShowConnectFailed();
            yield break;
        }
        #endregion

        #region 第三阶段，下载敌人图标
        WWW wIcon = new WWW(CU.ParsePath(EnemyAttribute.GetCompletedFilePathById(tempAttr.iconName)));
        yield return wIcon;
        if (wIcon.isDone && wIcon.error == null)
        {
            iconTexture = wIcon.texture;
            wIcon.Dispose();
        }
        else
        {
            Debug.Log(id);
            Debug.LogWarning(wIcon.error);
            CU.ShowConnectFailed();
            wIcon.Dispose();
            yield break;
        }

        #endregion
        EnemyAttribute eAttr = new EnemyAttribute(tempAttr.idkey, id, tempAttr.name, tempAttr.description,baseP, growP, iconTexture, tempAttr.dropItems);
        idToEnmeyAttribute[id] = eAttr;
        CU.HideConnectingUI();
    }



    /// <summary>
    /// 检测指定id的敌人是否被加载。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IfEnemyAttributeDownloaded(string id)
    {
        return idToEnmeyAttribute.ContainsKey(id) && idToEnmeyAttribute[id] != null;
    }

    /// <summary>
    /// 生成单个敌人的属性集合。并且如果设置了难度系数，将全属性乘以这个系数值
    /// </summary>
    public static AttributeCollection GenerateAttrs(EnemyAttribute enemyAttrPrefab, int level)
    {
        AttributeCollection attrs = new AttributeCollection();
        var enemyAttrs = enemyAttrPrefab.baseP.attrs;
        foreach (var attr in AttributeCollection.GetAllAttributes())
        {
            float value = enemyAttrs[attr];
            value += (level - 1) * enemyAttrPrefab.GetGrowthValue(attr);
            value *= GlobalSettings.ENEMY_ENHANCE_FACTOR;
            attrs[attr] = value;
        }
        return attrs;
    }
}

