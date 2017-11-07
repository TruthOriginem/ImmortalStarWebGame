using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ItemContainerSuite
{
    public class ItemContainer : MonoBehaviour
    {
        public Text title;
        public Text tooltip;
        public GridLayoutGroup gridLayoutGroup;
        public RectTransform containerRect;
        public RectTransform viewPortRect;

        private Transform contentTransform;
        private static bool ifSyncNeeded = false;
        private static int targetCol = 0;
        private static int targetRow = 0;
        private static float extraWidth = 0;
        private static float extraHeight = 0;
        private static Action OnSpawn;
        private static Action OnClear;

        private CanvasGroup canvasGroup;
        private float targetAlpha = 0f;
        private bool toggleVisiableChange = false;


        public static ItemContainer Instance { get; set; }

        private void Awake()
        {
            Instance = this;
            canvasGroup = GetComponent<CanvasGroup>();
            extraWidth = containerRect.rect.width - viewPortRect.rect.width;
            extraHeight = containerRect.rect.height - viewPortRect.rect.height;
            contentTransform = gridLayoutGroup.transform;
            //Debug.Log(viewPortRect.rect.width);
            SetDefault();
        }

        IEnumerator _VisiableChange()
        {
            toggleVisiableChange = true;
            while (toggleVisiableChange)
            {
                float originalAlpha = canvasGroup.alpha;
                float currentAlpha = Mathf.Lerp(originalAlpha, targetAlpha, 15f * Time.deltaTime);
                if (Mathf.Abs(currentAlpha - targetAlpha) < 0.05f)
                {
                    currentAlpha = targetAlpha;
                    toggleVisiableChange = false;
                }
                canvasGroup.alpha = currentAlpha;
                yield return 0;
            }
        }
        /// <summary>
        /// 展示能容纳游戏UI的窗口。
        /// </summary>
        /// <param name="items">向容器里摆放的物体</param>
        /// <param name="onSpawn">在这里设置容器的相关属性</param>
        /// <param name="onClear">首先执行上次的Clear，再赋值这次的</param>
        /// <param name="title">窗口标题</param>
        /// <param name="tooltip">窗口提示</param>
        public static void ShowContainer(List<Transform> items, Action onSpawn, Action onClear, string title = "", string tooltip = "")
        {
            foreach (var item in items)
            {
                item.SetParent(Instance.contentTransform, false);
            }
            OnSpawn = onSpawn;
            if (onSpawn != null) onSpawn();
            OnClear = onClear;
            SetTitleAndToolTip(title, tooltip);
            Instance.Show();
        }

        public static void SetTitleAndToolTip(string title, string tooltip)
        {
            Instance.title.text = title;
            Instance.tooltip.text = tooltip;
        }
        /// <summary>
        /// 设置单位格大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetCellSize(float width, float height)
        {
            Instance.gridLayoutGroup.cellSize = new Vector2(width, height);
            ifSyncNeeded = true;
        }
        /// <summary>
        /// 设置单位格间隔
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SetSpacing(float x, float y)
        {
            Instance.gridLayoutGroup.spacing = new Vector2(x, y);
            ifSyncNeeded = true;
        }
        /// <summary>
        /// 设置每行每列可以容纳多少单位格
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public static void SetColAndRow(int col, int row)
        {
            targetCol = col;
            targetRow = row;
            ifSyncNeeded = true;
        }
        /// <summary>
        /// 设置默认单位格间隔与行列
        /// </summary>
        public static void SetDefault()
        {
            SetSpacing(10, 10);
            SetColAndRow(8, 4);
            SyncIfNeeded();
        }
        /// <summary>
        /// 更新UI。
        /// </summary>
        public static void SyncIfNeeded()
        {
            if (ifSyncNeeded)
            {
                var grid = Instance.gridLayoutGroup;
                float gridW = grid.cellSize.x;
                float gridH = grid.cellSize.y;
                float spacingW = grid.spacing.x;
                float spacingH = grid.spacing.y;
                float top = grid.padding.top;
                float bottom = grid.padding.bottom;
                float left = grid.padding.left;
                float right = grid.padding.right;
                float width = left + (targetCol - 1) * spacingW + targetCol * gridW + right + extraWidth;
                float height = top + (targetRow - 1) * spacingH + targetRow * gridH + bottom + extraHeight;
                Instance.containerRect.sizeDelta = new Vector2(width, height);
                ifSyncNeeded = false;
            }
        }

        public void Show()
        {
            targetAlpha = 1f;
            if (!toggleVisiableChange)
            {
                StartCoroutine(_VisiableChange());
            }
            canvasGroup.blocksRaycasts = true;
        }
        public void Close()
        {
            targetAlpha = 0f;
            if (!toggleVisiableChange)
            {
                StartCoroutine(_VisiableChange());
            }
            canvasGroup.blocksRaycasts = false;
            if (OnClear != null) OnClear();
        }

    }
    /// <summary>
    /// 调用ItemContainer的方法类。
    /// </summary>
    public static class ItemContainerParam
    {
        /// <summary>
        /// 设置当前物体容器的具体UI参数
        /// </summary>
        /// <param name="width">指定物体宽度</param>
        /// <param name="height">指定物体高度</param>
        /// <param name="col">一列有几个</param>
        /// <param name="row">一行有几个</param>
        /// <param name="spacingX"></param>
        /// <param name="spacingY"></param>
        public static void SetParam(float width, float height, int col = 8, int row = 4, float spacingX = 10, float spacingY = 10)
        {
            ItemContainer.SetCellSize(width, height);
            ItemContainer.SetColAndRow(col, row);
            ItemContainer.SetSpacing(spacingX, spacingY);
            ItemContainer.SyncIfNeeded();
        }
    }
}
