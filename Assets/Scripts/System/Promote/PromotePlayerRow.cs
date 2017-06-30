using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PromotePlayerRow : MonoBehaviour
{
    public Text nickNameText;
    public Text levelText;
    public Text sponsorText;
    public Text sponsorGetText;

    public void Init(TempPromoter promoter)
    {
        if (promoter == null)
        {
            return;
        }
        nickNameText.text = promoter.nickname;
        levelText.text = "Lv." + promoter.level;
        sponsorText.text = "总共赞助了 " + promoter.totalDimen + " 次元币";
        if (sponsorGetText != null)
        {
            sponsorGetText.text = "可提取 " + promoter.promoteDimen + " 次元币";

        }
    }
    void Start()
    {

    }

}
