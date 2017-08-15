using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

[RequireComponent(typeof(UIWindow))]
public class SceneWindow : MonoBehaviour
{
    public UIWindow window;
    [Tooltip("初始化后就禁用掉。")]
    public bool isDisable;
    public UnityEvent OnSceneShowed;
    public UnityEvent OnSceneHided;
    private CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    void Start()
    {
        //window = GetComponent<UIWindow>();
        if (isDisable)
        {
            StartCoroutine(SetDisable());
        }
    }
    IEnumerator SetDisable()
    {
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 如果选择了这个场景需要做的事
    /// </summary>
    /// <param name="show"></param>
    public void OnSceneChanged(bool show)
    {
        gameObject.SetActive(true);
        if (show)
        {
            window.Show();
            if (OnSceneShowed != null)
            {
                OnSceneShowed.Invoke();
            }
            GameSceneManager.Instance.SetupNowScene(transform);
        }
        else
        {
            if (OnSceneHided != null)
            {
                OnSceneHided.Invoke();
            }
            window.Hide();
            StartCoroutine(HideWindow(window));
            //window.gameObject.SetActive(false);
        }
        BattleManager.Instance.ShouldHide = true;
    }
    IEnumerator HideWindow(UIWindow window)
    {
        canvasGroup.blocksRaycasts = false;
        int waitFrame = 0;//6秒（360帧）缓冲，如果超过则取消这个
        while (true)
        {
            if (!window.IsVisible)
            {
                gameObject.SetActive(false);
                yield break;
            }
            waitFrame++;
            if (waitFrame >= 360)
            {
                yield break;
            }
            yield return 0;
        }
    }
}
