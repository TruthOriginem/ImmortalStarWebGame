using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using GameId;

public class GameSceneManager : MonoBehaviour
{
    private Transform nowScene;
    [Header("窗口")]
    public Transform propertyWindow;


    [Header("杂项")]
    public Text propertyText;
    public Slider playerHpSlider;
    public Slider playerMpSlider;

    public CanvasGroup togglesCanvas;

    public static bool PropertyDirty = true;
    public static GameSceneManager Instance { get; set; }
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        SetupNowScene(propertyWindow);
        RefreshPlayerInfo();
    }
    public void RefreshPlayerInfo()
    {
        StartCoroutine(refreshplayerinfo());
    }
    IEnumerator refreshplayerinfo()
    {
        yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        OCManager.Refresh();
        PropertyDirty = true;
    }
    public void LoadCategory()
    {
        CategoryManager.Instance.RequestLoad();
    }
    public void LoadBattle()
    {
        BattleLayerManager.Instance.Init();
    }
    public void LoadHangUp()
    {
        HangUpManager.Instance.LoadScene();
    }

    public void SetupNowScene(Transform nowScene)
    {
        this.nowScene = nowScene;
    }

    void Update()
    {
        if (PropertyDirty && nowScene == propertyWindow)
        {
            PlayerInfoInGame playerProperty = PlayerInfoInGame.Instance;
            playerHpSlider.value = 1;
            playerMpSlider.value = 1;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("昵称： " + PlayerInfoInGame.NickName);
            sb.Append("等级：<b> " + PlayerInfoInGame.Level);
            sb.AppendLine("</b>");
            sb.Append("称号：<b> ");
            sb.AppendLine(DesignationManager.GetDesignationName(PlayerInfoInGame.Design_NowEquipped));
            sb.Append("</b>生命值： ");
            sb.AppendLine(TextUtils.GetGreenText(playerProperty.GetDynamicAttrValue(GameId.Attrs.MHP).ToString("0")));
            sb.Append("能量值： ");
            sb.AppendLine(TextUtils.GetMpText(playerProperty.GetDynamicAttrValue(GameId.Attrs.MMP).ToString("0")));
            sb.Append("经验值： <color=#9D6EFFFF>");
            sb.Append(TextUtils.GetOmitNumberString(PlayerInfoInGame.Exp));
            sb.Append("/");
            sb.Append(TextUtils.GetOmitNumberString(PlayerInfoInGame.NextExp));
            sb.Append("<size=14>(");
            if (PlayerInfoInGame.NextExp == 0)
            {
                PlayerInfoInGame.NextExp = 1;
            }
            sb.Append(Mathf.RoundToInt((float)PlayerInfoInGame.Exp / PlayerInfoInGame.NextExp * 100f));
            sb.AppendLine("%)</size></color>");
            var attrs = PlayerInfoInGame.Instance.GetDynamicAttrs();
            foreach (var attr in AttributeCollection.GetAllAttributes())
            {
                if (attr == Attrs.MHP || attr == Attrs.MMP) continue;
                sb.Append(attr.Name + ": " + attrs.GetValueToString(attr));
                sb.Append("<color=#838383FF>(");
                sb.Append(PlayerInfoInGame.Instance.GetSourceAttrValue(attr).ToString("0.00"));
                sb.AppendLine(")</color>");
            }
            propertyText.text = sb.ToString();
            PropertyDirty = false;
        }
        if (BattleManager.Instance.IsInBattle())
        {
            togglesCanvas.interactable = false;
            togglesCanvas.alpha = 0.8f;
        }
        else
        {
            togglesCanvas.alpha = 1.0f;
            togglesCanvas.interactable = true;
        }
    }
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ExitGame()
    {
        MessageBox.Show("你确定要返回主菜单？", "温馨提示", (result) =>
        {
            if (result == DialogResult.Yes)
            {
                StartCoroutine(ReturnLoginWindow());
            }
        }, MessageBoxButtons.YesNo);
    }

    public IEnumerator ReturnLoginWindow()
    {
        LoadingUIManager.Instance.SetBlock(true);
        yield return SceneManager.LoadSceneAsync("login");
        yield return SceneManager.UnloadSceneAsync("demo");
    }
}
