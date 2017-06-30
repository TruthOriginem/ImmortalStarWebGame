﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    protected ItemBase linkedItem;
    public Text itemText;
    public Image itemImage;
    private bool isHighlight = false;
    private Sprite icon = null;
    public void LinkItem(ItemBase item)
    {
        linkedItem = item;
        UpdateItem();
    }
    public void UpdateItem()
    {
        if (linkedItem is EquipmentBase)
        {
            itemText.text = (linkedItem as EquipmentBase).GetName();
        }
        else
        {
            itemText.text = linkedItem.name;
        }
        LoadTexture();
    }
    public void BeHighLight()
    {
        if (!isHighlight)
        {
            isHighlight = true;
            StartCoroutine(HighLight());
        }
    }
    public void DeactHighLight()
    {
        isHighlight = false;
    }
    IEnumerator HighLight()
    {
        while (true)
        {
            if (isHighlight)
            {
                var rect = itemImage.rectTransform;
                float size = Mathf.Lerp(rect.sizeDelta.x, 90f, Time.deltaTime * 30f);
                rect.sizeDelta = new Vector2(size, size);
                yield return 0;
            }
            else
            {
                var rect = itemImage.rectTransform;
                float size = Mathf.Lerp(rect.sizeDelta.x, 70f, Time.deltaTime * 30f);
                rect.sizeDelta = new Vector2(size, size);
                if (Mathf.RoundToInt(size) == 70)
                {
                    yield break;
                }
                yield return 0;
            }
        }
    }
    public ItemBase GetLinkedItem()
    {
        return linkedItem;
    }
    public Sprite GetIcon()
    {
        return icon;
    }
    public void SetIcon(Sprite icon)
    {
        this.icon = icon;
        //RectTransform rect = GetComponent<RectTransform>();
        //Sprite sprite = Sprite.Create(icon.texture, new Rect(0, 0, rect.rect.width, rect.rect.height), new Vector2(0.5f, 0.5f));
        itemImage.sprite = icon;
    }

    /// <summary>
    /// 更新grid的UI的时候，请求载入图标，如果ItemModal里已经载入则返回那个sprite，
    /// 如果没有，就开启协程，下载该图标并加入ItemModal
    /// </summary>
    public void LoadTexture()
    {
        if (ItemModal.GetIconByPath(linkedItem.GetIconPath()) != null)
        {
            SetIcon(ItemModal.GetIconByPath(linkedItem.GetIconPath()));
        }
        else
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(RequestLoadTexture());
            }
        }

    }
    IEnumerator RequestLoadTexture()
    {
        ConnectUtils.ShowConnectingUI();
        if (ItemModal.GetIconByPath(linkedItem.GetIconPath()) != null)
        {
            SetIcon(ItemModal.GetIconByPath(linkedItem.GetIconPath()));
        }
        else
        {
            WWW w = new WWW(ConnectUtils.ParsePath(linkedItem.GetIconPath()));
            ConnectUtils.ShowConnectingUI();
            yield return w;
            ConnectUtils.HideConnectingUI();
            if (w.isDone && w.error == null)
            {
                //Debug.Log(linkedItem.GetIconPath());
                Texture2D iconTex = w.texture;

                Sprite _icon = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
                ///预加载
                ItemModal.AddIconByPath(linkedItem.GetIconPath(), _icon);
                ///
                SetIcon(_icon);
            }
            else
            {
                if (w.error != null)
                {
                    Debug.LogWarning(w.error);
                    w.Dispose();
                }
            }
        }
        ConnectUtils.HideConnectingUI();
    }
}