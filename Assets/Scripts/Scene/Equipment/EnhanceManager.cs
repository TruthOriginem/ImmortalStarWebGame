using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using SerializedClassForJson;
using GameId;

/// <summary>
/// 升级规则由客户端控制。
/// </summary>
public class EnhanceManager : MonoBehaviour
{
    public enum MODE
    {
        ENHANCE,//强化
        REHANCE,//再塑
        REBUILD //重构
    }
    private static float ENHANCE_FACTOR_PER_LEVEL = 1.05f;
    private static float REHANCE_FACTOR_PER_LEVEL = 0.65f;

    public Dropdown equipDropdown;
    public Text equipNeedMaterialText;

    [Header("装备比较窗口")]
    public Text oriEqDes;//未强化武器属性
    public Text ehaEqDes;//已强化武器属性

    [Header("强化选项")]
    public Toggle remainToggle;//依旧选择的Toggle
    //100灵基武器，免除20%消耗需要100/1000 * 强化等级个原灵碎片(最低为1)
    public Toggle spbToggle1;//免除20%消耗原灵碎片
    //100灵基武器，免除40%消耗需要100/400 * 强化等级个原灵碎片(最低为2)
    public Toggle spbToggle2;//免除40%消耗原灵碎片

    [Space]
    public Button confirmButton;

    /// <summary>
    /// 当前装备选项
    /// </summary>
    List<Dropdown.OptionData> eqOptionDatas;

    List<EquipmentBase> eqLists;//武器列表

    string remainWeaponId;//强化保留武器的id
    int remainIndex = -1;//目录

    TempEquipAttr tempAttr;//暂时生成的装备升级信息
    /// <summary>
    /// 当点击并显示窗口调用。
    /// </summary>
    public void Init()
    {
        StartCoroutine(InitEnhanceScene());
    }

    /// <summary>
    /// 初始化/重载灵基强化场景
    /// </summary>
    IEnumerator InitEnhanceScene()
    {
        yield return PlayerInfoInGame.Instance.RequestUpdatePlayerInfo();
        tempAttr = null;
        eqLists = PlayerInfoInGame.GetAllEquipments(true);
        InitDropdownData();

    }

    /// <summary>
    /// 将武器添加入dropdown的列表里。将处理是否选择了“依然选择该装备”的选项
    /// </summary>
    void InitDropdownData()
    {
        //先清空Dropdown
        equipDropdown.ClearOptions();
        eqOptionDatas = new List<Dropdown.OptionData>();
        remainIndex = -1;
        for (int i = 0; i < eqLists.Count; i++)
        {
            EquipmentBase equip = eqLists[i];
            Dropdown.OptionData data = new Dropdown.OptionData();
            //强化，重构，再塑等的显示
            StringBuilder sb = new StringBuilder();
            sb.Append(equip.GetName());
            sb.Append(" +");
            sb.Append(equip.eha_level);
            if (equip.eha_reha > 0)
            {
                sb.Append("[再塑+");
                sb.Append(equip.eha_reha);
                sb.Append("]");
            }
            if (equip.IsEquipped())
            {
                sb.Append("<color=yellow>(E)</color>");
            }
            data.text = sb.ToString();
            data.image = SpriteLibrary.GetSprite(equip.GetIconPath());
            eqOptionDatas.Add(data);
            if (remainWeaponId != null && remainWeaponId == equip.item_id)
            {
                remainIndex = i;
            }
        }
        //将武器加入Dropdown
        equipDropdown.AddOptions(eqOptionDatas);
        if (remainIndex != -1 && eqOptionDatas.Count > remainIndex)
        {
            equipDropdown.value = remainIndex;

        }
        OnEquipSelected(equipDropdown.value);
    }

    /// <summary>
    /// 返回现在选择的武器
    /// </summary>
    /// <returns></returns>
    EquipmentBase GetSelectedEquip()
    {
        return eqLists[equipDropdown.value];
    }

    string GetAddLevelName(EquipmentBase equip, int level)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(equip.GetName());
        if (equip.eha_reha == 10 && level >= 10)
        {
            sb.Append(" ★");
        }
        else
        {
            sb.Append(" +");
            sb.Append(level);
        }

        return sb.ToString();
    }

    public void RefreshSelectedEquip(bool trigger)
    {
        OnEquipSelected(equipDropdown.value);
    }

    /// <summary>
    /// dropdown回调，当选中某个装备时的操作。
    /// 会改变tempAttr，并在确认强化时使用该值
    /// </summary>
    /// <param name="index"></param>
    public void OnEquipSelected(int index)
    {
        if (eqLists == null || eqLists.Count == 0 || index == -1)
        {
            return;
        }
        EquipmentBase selectedEquip = eqLists[index];
        Dropdown.OptionData data = eqOptionDatas[index];
        tempAttr = new TempEquipAttr();
        tempAttr.item_id = selectedEquip.item_id;
        MODE mode;
        tempAttr.CheckAGetEnhanceMode(selectedEquip, out mode);
        if (data.image == null)
        {
            data.image = SpriteLibrary.GetSprite(eqLists[index].GetIconPath());
        }
        //原来武器的字符串
        StringBuilder oriSb = new StringBuilder();
        //如果强化的字符串
        StringBuilder resultSb = new StringBuilder();
        //右边的升级比较
        EquipmentBase equipment = selectedEquip;
        //系数设置
        float multFactor;//强化的相乘系数
        Text buttonText = confirmButton.GetComponentInChildren<Text>();
        switch (mode)
        {
            case MODE.ENHANCE:
                multFactor = ENHANCE_FACTOR_PER_LEVEL;
                buttonText.text = "强 化 确 认";
                break;
            case MODE.REHANCE:
                multFactor = REHANCE_FACTOR_PER_LEVEL;
                buttonText.text = "再 塑 确 认";
                break;
            default:
                multFactor = 1f;
                break;
        }
        oriSb.AppendLine("<size=16>" + GetAddLevelName(equipment, equipment.eha_level) + "</size>");
        if (equipment.eha_reha != 0)
        {
            oriSb.AppendLine("[再塑 +" + equipment.eha_reha + "]");
        }

        resultSb.AppendLine("<size=16>" + GetAddLevelName(equipment, tempAttr.eha_level) + "</size>");
        if (tempAttr.eha_reha != 0)
        {
            resultSb.AppendLine("[再塑 +" + tempAttr.eha_reha + "]");
        }

        oriSb.AppendLine("<size=14><b>" + equipment.GetEqTypeName() + "</b></size>");
        resultSb.AppendLine("<size=14><b>" + equipment.GetEqTypeName() + "</b></size>");
        oriSb.AppendLine(TextUtils.GetSpbText("灵基 -- " + equipment.GetSpb().ToString("0.0")));
        resultSb.AppendLine(TextUtils.GetSpbText("灵基 -- " + (equipment.GetSpb() * multFactor).ToString("0.0")));
        //设置其他受到系数影响的
        tempAttr.spb = equipment.GetSpb() * multFactor;
        tempAttr.price = Mathf.RoundToInt(equipment.price * multFactor);
        //
        oriSb.AppendLine();
        resultSb.AppendLine();
        EquipmentValue value = equipment.GetAttrs();
        AttributeCollection attrs = value.values;
        tempAttr.SetAttrsByValue(value);
        tempAttr.MultAllProperties(multFactor);
        foreach (var attr in AttributeCollection.GetAllAttrs())
        {
            var attrValue = attrs.GetValue(attr);
            if (attrValue != 0f)
            {
                oriSb.AppendLine(attr.Name + " " + attrs.GetValueToString(attr));
                float add = attrValue * (multFactor - 1f);
                resultSb.Append(attr.Name + " " + (attrValue * multFactor).ToString("0.00"));
                resultSb.Append(add > 0f ? "<color=green>(+" : "<color=red>(");
                resultSb.Append(add.ToString("0.00"));
                resultSb.AppendLine(")</color>");
            }
        }
        //需要材料的更新与提示
        BaseEquipMod mod = EquipmentFactory.GetBaseEquipModById(equipment.GetBaseModId());
        StringBuilder needSb = new StringBuilder();
        bool canEnhance = true;
        //免除材料消耗
        int type = mode != MODE.REHANCE ? spbToggle1.isOn ? 1 : spbToggle2.isOn ? 2 : 0 : 0;
        //如果type为1，则0.8，如果不为1，如果为2，则0.6，不然为1
        float amountMult = type == 1 ? 0.8f : (type == 2 ? 0.6f : 1f);
        needSb.AppendLine("<size=20><b>强化需要材料：</b></size>");
        needSb.AppendLine();
        var demands = mod.GenerateDemandsByEquip(equipment);
        //决定基本需求
        foreach (var demand in demands)
        {
            if (demand.CanDemandCompleted(equipment))
            {
                int amount = demand.baseAmount + (int)((equipment.GetAllocatedLevel() - demand.needLevel) * demand.amountPerLevel);
                string name = ItemDataManager.GetItemName(demand.item_id);
                if (type != 0)
                {
                    amount = (int)(amount * amountMult);
                    amount = amount == 0 ? 1 : amount;
                }
                needSb.Append(name);
                needSb.Append(" * ");
                needSb.Append(amount);
                int diff = ItemDataManager.GetItemAmount(demand.item_id) - amount;
                needSb.Append(" ( ");
                needSb.Append(diff >= 0 ? "<color=green>剩余" : "<color=red>缺少");
                needSb.Append(Mathf.Abs(diff));
                needSb.AppendLine("</color> )");
                if (diff < 0)
                {
                    canEnhance = false;
                }
            }
        }
        if (type != 0)
        {
            string name = ItemDataManager.GetItemName(Items.SPB_PIECE);
            float divi = type == 1 ? 1000 : type == 2 ? 400 : 1;//除数
            int amount = Mathf.RoundToInt(equipment.GetSpb() / divi * equipment.GetAllocatedLevel());
            amount = amount >= type ? amount : type;//最低灵基碎片需要1或2
            needSb.Append(name);
            needSb.Append(" * ");
            needSb.Append(amount);
            int diff = ItemDataManager.GetItemAmount(Items.SPB_PIECE) - amount;
            needSb.Append(" ( ");
            needSb.Append(diff >= 0 ? "<color=green>剩余" : "<color=red>缺少");
            needSb.Append(Mathf.Abs(diff));
            needSb.AppendLine("</color> )");
            if (diff < 0)
            {
                canEnhance = false;
            }
        }
        //
        bool noText = false;
        //目前，只要再塑10，强化10则不能再次升级
        if (equipment.eha_reha == 10 && equipment.eha_level == 10)
        {
            canEnhance = false;
            noText = true;
        }
        //完成两者比较文本
        oriEqDes.text = oriSb.ToString();
        ehaEqDes.text = !noText ? resultSb.ToString() : "";
        //完成材料需求文本
        equipNeedMaterialText.text = !noText ? needSb.ToString() : "";
        //按钮的可操作性
        confirmButton.interactable = canEnhance;
    }

    /// <summary>
    /// Confirm按钮按下
    /// </summary>
    public void Confirm()
    {
        if (tempAttr != null)
        {
            if (GlobalSettings.SHOW_ENHANCE_DIALOG)
            {
                ToggleBox.Show("您是否确定？", "温馨提示", "本次游戏中不再出现该提示", (result, toggle) =>
                   {
                       if (result == DialogResult.Yes)
                       {
                           StartCoroutine(ConfirmEnhancement());
                       }
                       GlobalSettings.SHOW_ENHANCE_DIALOG = !toggle;
                   }, MessageBoxButtons.YesNo);
            }
            else
            {
                StartCoroutine(ConfirmEnhancement());
            }

        }
    }
    /// <summary>
    /// 确认强化的协程，并更新人物、装备状态
    /// </summary>
    IEnumerator ConfirmEnhancement()
    {
        yield return ItemDataManager.GetItemsAmount();
        EquipmentBase equipment = GetSelectedEquip();
        Dictionary<string, Currency> itemDic = new Dictionary<string, Currency>();
        TempPlayerAttribute attr = new TempPlayerAttribute();
        MODE mode;
        tempAttr.CheckAGetEnhanceMode(equipment, out mode);
        //免除材料消耗
        int type = mode != MODE.REHANCE ? spbToggle1.isOn ? 1 : spbToggle2.isOn ? 2 : 0 : 0;
        //如果type为1，则0.8，如果不为1，如果为2，则0.6，不然为1
        float amountMult = type == 1 ? 0.8f : (type == 2 ? 0.6f : 1f);
        foreach (var demand in EquipmentFactory.GetBaseEquipModById(equipment.GetBaseModId()).GenerateDemandsByEquip(equipment))
        {
            //如果装备总计等级小于需求的等级，且按钮是可以用的话，那么这个需求需要无视
            if (equipment.GetAllocatedLevel() < demand.needLevel)
            {
                continue;
            }
            if (equipment.GetAllocatedLevel() > demand.stopLevel && demand.stopLevel > demand.needLevel)
            {
                continue;
            }
            int amount = demand.baseAmount + (int)((equipment.GetAllocatedLevel() - demand.needLevel) * demand.amountPerLevel);
            if (type != 0)
            {
                amount = (int)(amount * amountMult);
                amount = amount == 0 ? 1 : amount;
            }
            int diff = ItemDataManager.GetItemAmount(demand.item_id) - amount;
            if (diff < 0)
            {
                // Debug.Log(demand.item_id + diff);
                MessageBox.Show("您没有足够的材料。");
                yield break;
            }
            if (demand.item_id == "money")
            {
                attr.money -= amount;
            }
            itemDic.Add(demand.item_id, -amount);
        }
        if (type != 0)
        {
            //string name = ItemDataManager.GetItemName(Items.SPB_PIECE);
            float divi = type == 1 ? 1000 : type == 2 ? 400 : 1;//除数
            int amount = Mathf.RoundToInt(equipment.GetSpb() / divi * equipment.GetAllocatedLevel());
            amount = amount >= type ? amount : type;//最低灵基碎片需要1或2
            int diff = ItemDataManager.GetItemAmount(Items.SPB_PIECE) - amount;
            if (diff < 0)
            {
                // Debug.Log(demand.item_id + diff);
                MessageBox.Show("您没有足够的材料。");
                yield break;
            }
            itemDic.Add(Items.SPB_PIECE, -amount);
        }
        ConnectUtils.ShowConnectingUI();
        IIABinds bind = new IIABinds(itemDic);
        SyncRequest.AppendRequest(Requests.EQ_ENHANCE_DATA, tempAttr);
        SyncRequest.AppendRequest(Requests.ITEM_DATA, bind.GenerateJsonString(false));
        SyncRequest.AppendRequest(Requests.PLAYER_DATA, attr);
        WWW w = SyncRequest.CreateSyncWWW();
        yield return w;
        if (ConnectUtils.IsPostSucceed(w))
        {
            if (remainToggle.isOn)
            {
                remainWeaponId = equipment.item_id;
            }
            else
            {
                remainWeaponId = "";
                equipDropdown.value = 0;
            }
            StartCoroutine(InitEnhanceScene());
        }
        else
        {
            ConnectUtils.ShowConnectFailed();
        }
        ConnectUtils.HideConnectingUI();
    }
}
