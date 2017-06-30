using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 在屏幕里显示一条持续一段时间的信息
/// </summary>
public class ConTipsManager : MonoBehaviour
{
    public CanvasGroup canvas;
    public Text titleText;
    public Text contentText;

    public const float DEFAULT_FADEIN = 0.4f;
    public const float DEFAULT_FADEOUT = 0.6f;
    public const float DEFAULT_DURATION = 2.5f;

    public static ConTipsManager Instance { get; set; }
    public static Queue<ConMessage> messages;

    private static bool isShowing = true;
    void Awake()
    {
        Instance = this;
        messages = new Queue<ConMessage>();
    }
    /// <summary>
    /// 处理UI
    /// </summary>
    void Update()
    {

        if (isShowing)
        {
            if (messages.Count == 0)
            {
                isShowing = false;
                canvas.alpha = 0;
                gameObject.SetActive(false);
                return;
            }
            ConMessage msg = messages.Peek();
            float elapsed = msg.elapsed;
            float alpha;
            if (elapsed < msg.fadeIn)
            {
                if (elapsed == 0f)
                {
                    titleText.text = msg.title;
                    contentText.text = msg.content;
                }
                alpha = elapsed / msg.fadeIn;
            }
            else if (elapsed < msg.fadeIn + msg.duration)
            {
                alpha = 1f;
            }
            else if (elapsed < msg.fadeIn + msg.duration + msg.fadeOut)
            {
                alpha = (msg.fadeIn + msg.duration + msg.fadeOut - elapsed) / msg.fadeOut;
            }
            else
            {
                //结束并检查有没有下一个message
                alpha = 0f;
                messages.Dequeue();
            }
            canvas.alpha = alpha;
            msg.Advance();
        }
    }
    /// <summary>
    /// 在屏幕下方显示一条信息，多个信息将会排队显示。
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    public static void AddMessage(string title, string content)
    {
        AddMessage(title, content, DEFAULT_FADEIN, DEFAULT_FADEOUT, DEFAULT_DURATION);
    }
    /// <summary>
    /// 在屏幕下方显示一条信息，多个信息将会排队显示。可以自定义彩色。
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    public static void AddMessage(string title, string content, Color t_color, Color c_color)
    {
        AddMessage(TextUtils.GetColoredText(title, t_color), TextUtils.GetColoredText(content, c_color), DEFAULT_FADEIN, DEFAULT_FADEOUT, DEFAULT_DURATION);
    }
    public static void AddMessage(string title, string content, float fadeIn, float fadeOut, float duration)
    {
        messages.Enqueue(new ConMessage(title, content, fadeIn, fadeOut, duration));
        isShowing = true;
        Instance.gameObject.SetActive(true);
    }
    /// <summary>
    /// 信息类
    /// </summary>
    public class ConMessage
    {
        public string title;
        public string content;
        public float fadeIn;
        public float fadeOut;
        public float duration;
        public float elapsed = 0f;
        public ConMessage(string title, string content, float fadeIn, float fadeOut, float duration)
        {
            this.title = title;
            this.content = content;
            this.fadeIn = fadeIn;
            this.fadeOut = fadeOut;
            this.duration = duration;
        }
        public void Advance()
        {
            elapsed += Time.deltaTime;
        }
    }
}

