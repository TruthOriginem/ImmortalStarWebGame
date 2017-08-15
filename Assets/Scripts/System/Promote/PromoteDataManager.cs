using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PromoteDataManager : MonoBehaviour
{
    public PromotePlayerRow RowPrefab;

    public PromotePlayerRow LeaderRow;

    public Transform PromoterContent;

    public Text PlayerUid;
    public Text PromoteNumber;

    public Button DrawMoneyButton;

    private static string GET_PROMOTERS_PATH = "scripts/player/system/getPromotersAndLeader.php";
    private static string UPDATE_DIVIDES_PATH = "scripts/player/system/updatePromoteDivided.php";
    public static PromoteDataManager Instance { get; set; }

    void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        //删掉
        Transform[] trans = PromoterContent.GetComponentsInChildren<Transform>();
        for (int i = 0; i < trans.Length; i++)
        {
            if (PromoterContent == trans[i])
            {
                continue;
            }
            Destroy(trans[i].gameObject);
        }
        PlayerUid.text = PlayerInfoInGame.uid.ToString();
        StartCoroutine(LoadAllPromoters());
    }
    /// <summary>
    /// 领取所有分成
    /// </summary>
    public void DrawYourDivides()
    {
        StartCoroutine(DrawDivides());
    }


    IEnumerator LoadAllPromoters()
    {
        CU.ShowConnectingUI();
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        WWW w = new WWW(CU.ParsePath(GET_PROMOTERS_PATH), form);
        yield return w;
        if (w.isDone && w.text != "nothing")
        {
            bool hasLeft = false;
            PromoterBundle bundle = JsonUtility.FromJson<PromoterBundle>(w.text);
            LeaderRow.Init(bundle.leader);
            for (int i = 0; i < bundle.promoters.Length; i++)
            {
                PromotePlayerRow row = Instantiate(RowPrefab);
                row.transform.SetParent(PromoterContent);
                row.transform.localScale = new Vector3(1f, 1f, 1f);
                row.Init(bundle.promoters[i]);
                if (bundle.promoters[i].promoteDimen > 0)
                {
                    hasLeft = true;
                }
            }
            PromoteNumber.text = bundle.promoters.Length.ToString();
            if (hasLeft)
            {
                DrawMoneyButton.gameObject.SetActive(true);
            }
            else
            {
                DrawMoneyButton.gameObject.SetActive(false);
            }
        }
        else
        {
            DrawMoneyButton.gameObject.SetActive(false);
        }
        CU.HideConnectingUI();
    }
    IEnumerator DrawDivides()
    {
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerInfoInGame.Id);
        form.AddField("uid", PlayerInfoInGame.uid);
        WWW w = new WWW(CU.ParsePath(UPDATE_DIVIDES_PATH), form);
        yield return w;
        yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        Init();
    }
    [System.Serializable]
    public class PromoterBundle
    {
        public TempPromoter leader;
        public TempPromoter[] promoters;
    }
}
