using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class BattleStage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string stageId;
    /// <summary>
    /// 图片文件的名字
    /// </summary>
    public string imageFileName;
    public string stageName;
    [TextArea]
    public string stageDescription;
    [Header("必须组件")]
    public Text stageButtonText;
    public Image stageImage;
    [Header("开启该幕需要通关的关卡id")]
    public string[] preGridIds;

    public CanvasGroup localCanvasGroup;
    /// <summary>
    /// 开启状态
    /// </summary>
    private bool isActable = true;
    private BatStageData linkedStageData;
    /// <summary>
    /// 图片的文件夹路径
    /// </summary>
    private static string IMAGE_FOLDER_PATH = "icons/stages/";
    /// <summary>
    /// 图片的统一后缀名
    /// </summary>
    private static string IMAGE_FILE_SUFFIX = ".jpg";

    public static Action<BattleStage> onToolTipEnter;
    public static Action onToolTipExit;

    public BatStageData LinkedStageData
    {
        get
        {
            return linkedStageData;
        }

        set
        {
            linkedStageData = value;
        }
    }

    /// <summary>
    /// 打开关卡界面
    /// </summary>
    public void OpenStage()
    {
        BattleLayerManager.Instance.Push(BattleInstanceGridPool.Instance.canvasGroup, () =>
         {
             BattleInstanceGridPool.Instance.SetTargetObjects(linkedStageData);
             BattleInstanceManager.NOW_LINKED_STAGE = linkedStageData;
         });
    }
    /// <summary>
    /// 初始化该幕
    /// </summary>
    public void InitStageImage()
    {
        StartCoroutine(DownLoadImage());
    }

    /// <summary>
    /// 用于向服务器请求图片的协程
    /// </summary>
    /// <returns></returns>
    IEnumerator DownLoadImage()
    {
        var path = IMAGE_FOLDER_PATH + imageFileName + IMAGE_FILE_SUFFIX;
        if (ItemModal.GetIconByPath(path) != null)
        {
            stageImage.sprite = ItemModal.GetIconByPath(path);
        }
        else
        {
            ConnectUtils.ShowConnectingUI();
            WWW w = new WWW(ConnectUtils.ParsePath(path));
            yield return w;
            ConnectUtils.HideConnectingUI();
            if (ConnectUtils.IsDownloadCompleted(w))
            {
                Texture2D texture = w.texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                ItemModal.AddIconByPath(path, sprite);
                stageImage.sprite = sprite;
            }
            else
            {
                if (w.error != null)
                {
                    Debug.LogWarning("传输图片有问题！");
                }
            }
            w.Dispose();
        }
    }



    void Start()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onToolTipEnter != null)
        {
            onToolTipEnter(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onToolTipExit != null)
        {
            onToolTipExit();
        }
    }
    public bool SetActable(bool ifcan)
    {
        isActable = ifcan;
        localCanvasGroup.alpha = isActable ? 1f : 0.8f;
        localCanvasGroup.interactable = isActable;
        return isActable;
    }
    public bool GetActable()
    {
        return isActable;
    }
}
