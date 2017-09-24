using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 下载游戏公告\教程等。
/// </summary>
public class LogManager : MonoBehaviour
{
    public Text announcement;
    [Header("教程内容")]
    public Text characterCont;
    public Text itemCont;
    public Text enhanceCont;
    public Text vipCont;
    public static LogManager Instance { get; set; }
    private static string PATH = "scripts/logs/";
    void Start()
    {
        StartCoroutine(DownloadLog(announcement, PATH + "updateLog.log"));
        StartCoroutine(DownloadLog(characterCont, PATH + "characterCont.txt"));
        StartCoroutine(DownloadLog(itemCont, PATH + "itemCont.txt"));
        StartCoroutine(DownloadLog(enhanceCont, PATH + "enhanceCont.txt"));
    }
    IEnumerator DownloadLog(Text uiText, string path)
    {
        WWW w = new WWW(CU.ParsePath(path));
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            uiText.text = w.text;
        }
    }
    public void RefreshVIPContent(bool refresh)
    {
        if (refresh)
        {
            int vipLevel = PlayerInfoInGame.VIP_Level;
            StringBuilder sb = new StringBuilder();
            if (vipLevel == 0)
            {
                vipCont.text = "你现在还不是VIP\n" + GetVipDescription(1);
                return;
            }
            else
            {
                sb.Append(GetVipDescription(vipLevel));
                sb.AppendLine("接下来一级——");
                sb.Append(GetVipDescription(vipLevel + 1));
                vipCont.text = sb.ToString();
            }
        }
    }
    string GetVipDescription(int level)
    {
        //Debug.Log(level);
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("<color=yellow><size=18><b>VIP{0}级加成：</b></size></color>", level);
        sb.AppendLine();
        sb.AppendFormat("*一般关卡的星币获取为原来的{0:0.00}倍。", BattleAwardMult.GetMoneyMult(level));
        sb.AppendLine();
        sb.AppendFormat("*一般关卡的经验获取为原来的{0:0.00}倍。", BattleAwardMult.GetExpMult(level));
        sb.AppendLine();
        sb.AppendFormat("*一般关卡的物品掉率为原来的{0:0.00}倍。", BattleAwardMult.GetDropMult(level));
        sb.AppendLine();
        sb.AppendFormat("*远征物品掉率为原来的{0}倍。", BattleAwardMult.GetExpeditionDropMult(level));
        sb.AppendLine();
        sb.AppendLine();
        return sb.ToString();
    }
}
