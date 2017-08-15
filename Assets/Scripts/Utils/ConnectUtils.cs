using UnityEngine;
using System.Collections;

/// <summary>
/// ConnectUtils类，专门用于处理链接等。
/// </summary>
public class CU
{
    public const string FAILED = "failed";
    public static string server = "http://localhost";
    public static string port = "85";
    //public static bool home = false;
    /// <summary>
    /// 当部署为服务器的时候自动消除server和port
    /// </summary>
    /// <param name="fileName">目标文件的path名(例如assets/r.php)</param>
    /// <returns></returns>
    public static string ParsePath(string fileName)
    {
        /*
        bool home = false;
        if(Application.platform == RuntimePlatform.WebGLPlayer)
        {
             home = true;
        }
        if (home)
        {
            return fileName;
        }
        else
        {

            return server + ":" + port + "/" + fileName;
        }
        */
        if (port != null && port != "")
        {
            return server + ":" + port + "/" + fileName;
        }else
        {
            return server + "/" + fileName;
        }

    }

    public static void ShowConnectingUI()
    {
        LoadingUIManager.Instance.Begin();
    }
    public static void HideConnectingUI()
    {
        LoadingUIManager.Instance.End();
    }
    public static void ShowConnectFailed()
    {
        Debug.LogWarning("错误");
        MessageBox.Show("网络连接中断！", "警告");
    }

    /// <summary>
    /// 判断WWW下载完成且没有错误。
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static bool IsDownloadCompleted(WWW w)
    {
        return w.isDone && string.IsNullOrEmpty(w.error);
    }
    /// <summary>
    /// 判断WWW是否请求完成且没有返回失败。
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static bool IsPostSucceed(WWW w)
    {
        return w.isDone && w.text != FAILED;
    }
}
