using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 下载游戏公告\教程等。
/// </summary>
public class LogDownloader : MonoBehaviour
{
    public Text announcement;
    [Header("教程内容")]
    public Text characterCont;
    public Text itemCont;
    public static LogDownloader Instance { get; set; }
    private static string PATH = "scripts/logs/";
    void Start()
    {
        StartCoroutine(DownloadLog(announcement,PATH + "updateLog.log"));
        StartCoroutine(DownloadLog(characterCont, PATH + "characterCont.txt"));
        StartCoroutine(DownloadLog(itemCont, PATH + "itemCont.txt"));
    }
    IEnumerator DownloadLog(Text uiText,string path)
    {
        WWW w = new WWW(ConnectUtils.ParsePath(path));
        yield return w;
        if (w.isDone && w.error == null)
        {
            uiText.text = w.text;
        }
    }
}
