using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理礼包ui类
/// </summary>
public class GiftPackUnitManager : MonoBehaviour
{
    static bool HasAddAction = false;
    ComToolTip toolTip;
    List<GiftPackUnit> units = new List<GiftPackUnit>();
    bool isShowingTips = false;
    private void Start()
    {
        if (!HasAddAction)
        {
            GiftPackUnitTips.onPointerEnter += OnEnterGPTips;
            GiftPackUnitTips.onPointerExit += OnExitGPTips;
            HasAddAction = true;
        }
    }

    /// <summary>
    /// 在GiftPackUnit的Start里调用
    /// </summary>
    /// <param name="unit"></param>
    public void AddUnit(GiftPackUnit unit)
    {
        units.Add(unit);
    }
    /// <summary>
    /// 进入签到界面即刷新
    /// </summary>
    public void RefreshAll()
    {
        toolTip = ComToolTip.Instance;
        StartCoroutine(_RefreshAll());
    }
    public IEnumerator _RefreshAll()
    {
        yield return ItemPackManager.RequestUpdateIPBundle();
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Refresh();
        }
    }
    void OnEnterGPTips(GiftPackUnit gp)
    {
        toolTip.Show();
        toolTip.SetWidth(200);
        toolTip.SetTextSize(12);
        toolTip.SetText(gp.GetDescription());
        isShowingTips = true;
    }

    void OnExitGPTips()
    {
        toolTip.Hide();
        isShowingTips = false;
    }
    const float X_OFFSET = 10f;
    const float Y_OFFSET = -20f;
    private void Update()
    {
        if (isShowingTips)
        {
            toolTip.SetRelLocation(X_OFFSET, Y_OFFSET, Input.mousePosition);
        }
    }
}
