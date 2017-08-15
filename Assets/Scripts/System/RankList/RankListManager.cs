using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankListManager : MonoBehaviour
{
    public static string RANK_PATH = "scripts/player/system/getRankListInfo.php";
    public static RankListManager Instance { get; set; }
    public enum RANK_TYPE
    {
        MONEY, //星币排行榜
        LEVEL,
        EXPEDITION,
        MACHINEMATCH
    }

    public Text totalPageText;
    public Text currPageText;
    public Text rankTitleArg3;
    public Transform rankContent;
    public GameObject rankInfoPrefab;
    [Header("分页内容")]
    public Button firstPage;
    public Button finalPage;
    public Button nextPage;
    public Button lastPage;

    private RANK_TYPE rankType = RANK_TYPE.MONEY;//当前排行榜
    private int totalPages;//总计页数
    private int currPage;//当前页数
    private static int PLAYERS_PER_PAGE = 10;//每页有多少个排名

    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 更换排行榜时重置整个排行榜
    /// </summary>
    public void ResetRankList()
    {
        currPage = 1;
        RefreshRankContent();
    }

    /// <summary>
    /// 刷新排行榜内容
    /// </summary>
    public void RefreshRankContent()
    {
        for (int i = 0; i < rankContent.childCount; i++)
        {
            Destroy(rankContent.GetChild(i).gameObject);
        }
        StartCoroutine(RefreshRankContentCor());
    }
    IEnumerator RefreshRankContentCor()
    {
        CU.ShowConnectingUI();
        WWWForm form = GenerateRefreshForm(currPage,rankType,PLAYERS_PER_PAGE);
        WWW w = new WWW(CU.ParsePath(RANK_PATH), form);
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            TempRankInfoBundle bundle = JsonUtility.FromJson<TempRankInfoBundle>(w.text);
            totalPages = bundle.totalPage;
            for (int i = 0; i < bundle.infos.Length; i++)
            {
                PlayerRankInfo rankInfo = Instantiate(rankInfoPrefab, rankContent,false).GetComponent<PlayerRankInfo>();
                rankInfo.SetInfo(bundle.infos[i]);
            }
            nextPage.interactable = true;
            lastPage.interactable = true;
            firstPage.interactable = true;
            finalPage.interactable = true;
            if (currPage == totalPages)
            {
                nextPage.interactable = false;
                finalPage.interactable = false;
            }
            if (currPage == 1)
            {
                lastPage.interactable = false;
                firstPage.interactable = false;
            }
            currPageText.text = currPage.ToString();
            totalPageText.text = totalPages.ToString();
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
        CU.HideConnectingUI();
    }
    public void NextPage()
    {
        currPage++;
        RefreshRankContent();
    }
    public void LastPage()
    {
        currPage--;
        RefreshRankContent();
    }
    public void FirstPage()
    {
        currPage = 1;
        RefreshRankContent();
    }
    public void FinalPage()
    {
        currPage = totalPages;
        RefreshRankContent();
    }


    /// <summary>
    /// 生成post表单
    /// </summary>
    /// <param name="page">查询页数</param>
    /// <returns></returns>
    public WWWForm GenerateRefreshForm(int page,RANK_TYPE rankType,int maxItems)
    {
        WWWForm form = new WWWForm();
        form.AddField("rankType", (int)rankType);
        form.AddField("maxItems", maxItems);
        form.AddField("page", page);
        return form;
    }
    #region 选择排行榜
    public void TurnToMoneyRank(bool turn)
    {
        if (turn)
        {
            rankType = RANK_TYPE.MONEY;
            rankTitleArg3.text = "星币";
            ResetRankList();
        }
    }
    public void TurnToLevelRank(bool turn)
    {
        if (turn)
        {
            rankType = RANK_TYPE.LEVEL;
            rankTitleArg3.text = "等级";
            ResetRankList();
        }
    }
    public void TurnToExpeditionRank(bool turn)
    {
        if (turn)
        {
            rankType = RANK_TYPE.EXPEDITION;
            rankTitleArg3.text = "远征里程(/光年)";
            ResetRankList();
        }
    }
    public void TurnToMachineMatchRank(bool turn)
    {
        if (turn)
        {
            rankType = RANK_TYPE.MACHINEMATCH;
            rankTitleArg3.text = "擂台积分";
            ResetRankList();
        }
    }
    #endregion
}
