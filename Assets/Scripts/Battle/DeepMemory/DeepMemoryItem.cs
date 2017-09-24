using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 记忆中枢指定Item。
/// </summary>
public class DeepMemoryItem : MonoBehaviour
{
    public Image m_iconImage;
    public Text m_text;
    public void Refresh(Sprite sprite, string content)
    {
        m_iconImage.sprite = sprite;
        m_text.text = content;
    }
}
