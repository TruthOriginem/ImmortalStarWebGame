using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using GameId;
using UnityEngine.Events;
using SerializedClassForJson;

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
    public Button stageLevelUpButton;
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
        if (SpriteLibrary.GetSprite(path) != null)
        {
            stageImage.sprite = SpriteLibrary.GetSprite(path);
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
                SpriteLibrary.AddSprite(path, sprite);
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
    /// <summary>
    /// 点击“极限升级”会调用的方法。
    /// </summary>
    public void UpgradeExtremeLevel()
    {
        if (HangUpManager.isHanging)
        {
            MessageBox.Show("挂机状态的时候不能进行极限升级！");
            return;
        }
        List<string> optionContents = new List<string>();
        List<UnityAction> optionActions = new List<UnityAction>();
        if (ItemDataManager.GetItemAmount(Items.EXTREME_CRYSTAL_LV1) > 0)
        {
            optionContents.Add("使用 <color=red>极限水晶Lv.1</color> (+1极限等级)");
            optionActions.Add(() => { PlayerInfoInGame._StartCoroutine(_UpgradeExtremeLevel(Items.EXTREME_CRYSTAL_LV1)); });
        }
        if (optionContents.Count == 0 && optionActions.Count == 0)
        {
            MessageBox.Show("非常抱歉，你目前没有能极限升级的道具(例如极限水晶)。", "提示", null, MessageBoxButtons.OK);
        }
        else
        {
            optionContents.Add("取消");
            optionActions.Add(() => { });
            MenuBox.Show(optionContents, optionActions, "极限升级选项");
        }
    }
    /// <summary>
    /// 具体迭代器。
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    IEnumerator _UpgradeExtremeLevel(string itemId)
    {
        var exdata = new TempUpgradeExtremeLevel();
        exdata.stageId = stageId;
        List<string> gridIds = new List<string>();
        for (int i = 0; i < linkedStageData.grids.Length; i++)
        {
            var grid = linkedStageData.grids[i];
            gridIds.Add(grid.id);
        }
        exdata.gridsToRemove = gridIds.ToArray();
        exdata.levelToAdd = 1;
        //判断道具并决定升几级
        if (itemId == Items.EXTREME_CRYSTAL_LV1)
        {
            exdata.levelToAdd = 1;
        }
        IIABinds bind = new IIABinds(itemId, -1);
        SyncRequest.AppendRequest(Requests.EX_LEVEL_DATA, exdata);
        SyncRequest.AppendRequest(Requests.ITEM_DATA, bind.GenerateJsonString(false));
        yield return PlayerRequestBundle.RequestSyncUpdate();
        BattleLayerManager.Instance.Init();
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
[Serializable]
public class TempUpgradeExtremeLevel
{
    public string stageId;
    public int levelToAdd;
    public string[] gridsToRemove;
}