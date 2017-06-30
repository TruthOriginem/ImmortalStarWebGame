using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 用于管理“连接中”图标
/// </summary>
public class LoadingUIManager : MonoBehaviour
{

    public Image loadingImage;
    public CanvasGroup loadingCanvas;
    public static LoadingUIManager Instance { get; set; }
    private static int connectingProgress = 0;//正在连接中的玩意儿，如果连接未成功就会阻挡一切操作
    private GameObject BlockGO = null;

    //private float connectPassTime = 0f;//链接时间

    void Awake()
    {
        Instance = this;
        BlockGO = GameObject.Find("BlockImage");
        if (BlockGO != null)
        {
            BlockGO.SetActive(false);
        }
    }
    /// <summary>
    /// 设置游戏场景是否被阻隔（不能操作）
    /// </summary>
    /// <param name="blocked"></param>
    public void SetBlock(bool blocked)
    {
        BlockGO.SetActive(blocked);
        if (BlockGO.activeInHierarchy)
        {
            BlockGO.transform.SetAsLastSibling();
        }
    }

    public void Begin()
    {
        BlockGO.transform.SetAsLastSibling();
        connectingProgress++;
        if (BlockGO != null)
        {
            BlockGO.SetActive(true);
        }
    }
    public void End()
    {
        connectingProgress--;
        if (BlockGO != null && connectingProgress == 0)
        {
            BlockGO.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float alpha = loadingCanvas.alpha;
        if (connectingProgress == 0 && alpha == 0f)
        {
            return;
        }
        if (connectingProgress != 0)
        {
            alpha += 0.2f;
        }
        else
        {
            alpha -= 0.02f;
        }
        alpha = alpha >= 1f ? 1f : alpha;
        alpha = alpha <= 0f ? 0f : alpha;
        loadingCanvas.alpha = alpha;
        //loadingImage.transform.Rotate(0f, 0f, -5f);
    }
}
