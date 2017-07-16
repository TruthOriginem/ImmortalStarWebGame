using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComToolTip : MonoBehaviour
{
    public static ComToolTip Instance { get; set; }
    public Canvas mainCanvas;
    public RectTransform rectTransform;
    public Text content;
    private RectTransform canvasRT;
    const float DEFAULT_WIDTH = 180;
    const int TEXT_SIZE = 13;
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        SetWidth();
        SetTextSize();
    }
    private void Awake()
    {
        Instance = this;
        canvasRT = mainCanvas.GetComponent<RectTransform>();
        //SetWidth(120f);
        Hide();
    }
    /// <summary>
    /// 根据不同的调用方式设置不同的宽度以容纳字体。
    /// </summary>
    /// <param name="width"></param>
    public void SetWidth(float width = DEFAULT_WIDTH)
    {
        rectTransform.sizeDelta = new Vector2(width, 0);
    }
    /// <summary>
    /// 设置默认字体大小。
    /// </summary>
    /// <param name="size"></param>
    public void SetTextSize(int size = TEXT_SIZE)
    {
        content.fontSize = size;
    }
    public void SetText(string text)
    {
        content.text = text;
    }

    public void SetRelLocation(float xoffSet, float yoffSet, Vector2 position)
    {
        Vector2 localPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, position, Camera.main, out localPosition))
        {
            localPosition.x += xoffSet;
            localPosition.y += yoffSet;
            rectTransform.anchoredPosition = localPosition;
        }
    }

}
