using GameId;
using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 礼包UI
/// </summary>
public class GiftPackUnit : MonoBehaviour
{
    public GiftPackUnitManager gp_manager;
    public Image gp_icon;
    public Text gp_name;
    public Button gp_receiveButton;
    public ItemPacks packIdEnum;
    private ItemPack pack;
    int packId;
    /// <summary>
    /// 根据链接的ItemPack状态刷新状态
    /// </summary>
    public void Refresh()
    {
        pack = ItemPackManager.GetItemPack(packIdEnum);
        packId = pack.GetPackId();
        gp_name.text = pack.GetPackName();

        gp_receiveButton.interactable = pack.CanBeRecievedNow() && pack.HaveAccessToReceive();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(pack.GetPackName());
        string color = "red";
        string t = "已";
        if (!pack.HaveAccessToReceive())
        {
            t = "无法";
        }
        else if (pack.CanBeRecievedNow())
        {
            color = "lime";
            t = "未";
        }
        sb.AppendFormat("<color={0}><size=14>等级{1} {2}领取</size></color>", color, pack.GetPackLevel(), t);
        gp_name.text = sb.ToString();
        StartCoroutine(_DownLoadIcon());
    }
    /// <summary>
    /// 点击领取调用
    /// </summary>
    public void Recieve()
    {
        StartCoroutine(_Recieve());
    }
    IEnumerator _Recieve()
    {
        yield return gp_manager._RefreshAll();
        if (!pack.CanBeRecievedNow())
        {
            MessageBox.Show("你现在不能领取该礼包！", "提示");
            yield break;
        }
        var dictionary = pack.GetItemToAmounts();
        var packNowString = pack.GetItemsString();
        long money = 0;
        long dimen = 0;
        if (dictionary.ContainsKey(Items.MONEY))
        {
            money = dictionary[Items.MONEY];
            dictionary.Remove(Items.MONEY);
        }
        if (dictionary.ContainsKey(Items.DIMEN))
        {
            money = dictionary[Items.DIMEN];
            dictionary.Remove(Items.DIMEN);
        }
        TempPlayerAttribute attr = new TempPlayerAttribute();
        attr.money += money;
        attr.dimenCoin += dimen;
        IIABinds bind = new IIABinds(dictionary);
        TempGPUpdate gpu = new TempGPUpdate();
        gpu.targetId = packId;
        gpu.targetLevel = pack.GetPackLevel();
        //Debug.Log(JsonUtility.ToJson(gpu));
        SyncRequest.AppendRequest(Requests.PLAYER_DATA, attr);
        SyncRequest.AppendRequest(Requests.ITEM_DATA, bind.GenerateJsonString(false));
        SyncRequest.AppendRequest(Requests.GIFT_PACK_DATA, gpu);
        yield return PlayerRequestBundle.RequestSyncUpdate();
        MessageBox.Show("成功领取了:\n" + packNowString, "恭喜");
        gp_manager.RefreshAll();
    }
    /// <summary>
    /// 对该礼包的说明
    /// </summary>
    /// <returns></returns>
    public string GetDescription()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("<size=18>{0}</size>", pack.GetPackName());
        sb.AppendLine();
        string color = "red";
        string t = "已";
        if (!pack.HaveAccessToReceive())
        {
            t = "无法";
        }
        else if (pack.CanBeRecievedNow())
        {
            color = "lime";
            t = "未";
        }
        sb.AppendFormat("<color={0}><size=14>等级{1} {2}领取</size></color>", color, pack.GetPackLevel(), t);
        sb.AppendLine();
        sb.AppendLine(pack.GetPackDescription());
        if (pack.HaveAccessToReceive())
        {
            sb.AppendLine();
            sb.AppendLine(pack.GetItemsString());
        }
        return sb.ToString();
    }

    IEnumerator _DownLoadIcon()
    {
        var path = pack.GetPackIconPath();
        if (!SpriteLibrary.IsSpriteDownLoading(path))
        {
            WWW w = new WWW(ConnectUtils.ParsePath(pack.GetPackIconPath()));
            ConnectUtils.ShowConnectingUI();
            SpriteLibrary.SetSpriteDownLoading(path);
            yield return w;
            ConnectUtils.HideConnectingUI();
            if (ConnectUtils.IsDownloadCompleted(w))
            {
                var tex = w.texture;
                tex.Compress(true);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                SpriteLibrary.AddSprite(path, sprite);
                gp_icon.sprite = sprite;
            }
            w.Dispose();
        }
    }
}
