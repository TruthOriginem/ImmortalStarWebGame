using PathologicalGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SerializedClassForJson.Chip;
using System.Text;
using GameId;

public class ChipUIManager : MonoBehaviour
{
    public static ChipUIManager Instance { get; set; }
    [Header("稀有度Sprite")]
    public List<Sprite> raritySprites = new List<Sprite>();
    [Header("装备项")]
    public Dropdown dropdown_Equipments;
    public Text text_EquipmentDescription;
    [Header("具体芯片信息项")]
    public Button button_Upgrade;
    public Button button_Drop;
    public Image targetChipIcon;
    public Text targetChipName;
    public Text targetChipInfo1;
    public Text targetChipInfo2;
    [Header("芯片池子")]
    public SpawnPool spawnPool;
    public ToggleGroup group;
    public Transform chipItemPrefab;
    public Transform eqContainer;
    public Transform bagContainer;
    [Header("芯片装配")]
    public Text maxInstallText;
    public Button installButton;
    public Button unstallButton;

    private ChipData targetData;
    private int currentTargetChipId = -1;
    private int currentWeaponIndex = -1;
    private int maxChipEmploy = 0;
    private int nowEquippedAmount = 0;
    private List<EquipmentBase> currentEqList;
    void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 界面初始化/刷新。
    /// </summary>
    public void OnShowAndRefresh()
    {
        SelectAndShowInPanel(null);
        StartCoroutine(_OnShowAndRefresh());
    }
    IEnumerator _OnShowAndRefresh()
    {
        yield return RequestBundle.RequestUpdateItemsInPack();
        spawnPool.DespawnAll();
        RefreshEqDropDown();
        var curEquip = GetCurrentEquipment();
        for (int i = 0; i < PlayerInfoInGame.CurrentChips.Count; i++)
        {
            var data = PlayerInfoInGame.CurrentChips[i];
            if (!data.IsEquipped())
            {
                var item = Spawn(data, bagContainer);
                if (data.GetId() == currentTargetChipId)
                {
                    item.SetToggle(true);
                }
            }
        }
    }
    public void RefreshEqDropDown()
    {
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        dropdown_Equipments.ClearOptions();
        currentEqList = new List<EquipmentBase>(PlayerInfoInGame.GetAllEquipments(true));
        for (int i = 0; i < currentEqList.Count; i++)
        {
            var equip = currentEqList[i];
            var optionData = new Dropdown.OptionData(string.Format("{0}{1} ({2})",
                                                     equip.GetModifyiedName(), equip.IsEquipped ? "(E)" : "",
                                                     equip.LinkedChips.Count == 0 ? "空" : equip.LinkedChips.Count.ToString()),
                                                     SpriteLibrary.GetSprite(equip.GetIconPath()));
            optionDatas.Add(optionData);
        }
        if (optionDatas.Count != 0)
        {
            if (currentWeaponIndex >= optionDatas.Count || currentWeaponIndex == -1)
            {
                currentWeaponIndex = 0;
            }
            dropdown_Equipments.AddOptions(optionDatas);
        }
        else
        {
            currentWeaponIndex = -1;
        }
        SelectEq(currentWeaponIndex);
    }
    public void SelectEq(int index)
    {
        if (index == -1)
        {
            text_EquipmentDescription.text = "";
            return;
        }
        currentWeaponIndex = index;
        var equip = currentEqList[index];
        nowEquippedAmount = 0;
        maxChipEmploy = 0;
        //清空装备芯片池
        DespawnAllInContainer(eqContainer);
        //Debug.Log(currentTargetChipId);
        for (int i = 0; i < PlayerInfoInGame.CurrentChips.Count; i++)
        {
            var chipData = PlayerInfoInGame.CurrentChips[i];
            if (!chipData.IsEquipped()) continue;
            //如果这个芯片的装备id等于当前装备id
            if (chipData.GetEquippedId().ToString() == equip.item_id)
            {
                var item = Spawn(chipData, eqContainer);
                if (chipData.GetId() == currentTargetChipId)
                {
                    item.SetToggle(true);
                }
                nowEquippedAmount++;
                //如果之前选中了上一个武器的芯片
            }
            else if (chipData.GetId() == currentTargetChipId)
            {//可以知道，如果之前选中的不是现在这个武器的芯片的话，前面那个是必然不会触发的
                //Debug.Log(currentTargetChipId);
                SelectAndShowInPanel(null);
            }
        }

        text_EquipmentDescription.text = GetEquipmentDescription();
        maxChipEmploy = equip.GetMaxChipDeployment();
        maxInstallText.text = string.Format("当前装备已安装芯片：{0}/{1}", nowEquippedAmount, maxChipEmploy);
    }
    /// <summary>
    /// 选择一个 ChipUIItem 时。刷新芯片介绍与是否可以升级、装卸。
    /// </summary>
    /// <param name="data"></param>
    void SelectAndShowInPanel(ChipUIItem item)
    {
        var data = item != null ? item.GetLinkedData() : null;
        targetData = data;
        bool dataExists = data != null;
        currentTargetChipId = !dataExists ? -1 : data.GetId();
        button_Upgrade.interactable = !dataExists ? false : data.CanUpgrade();
        button_Drop.interactable = !dataExists ? false : true;
        targetChipName.text = !dataExists ? "无" : data.GetFullName(true);
        targetChipInfo1.text = !dataExists ? "" : data.GetInfo1();
        targetChipInfo2.text = !dataExists ? "" : data.GetInfo2();
        targetChipIcon.gameObject.SetActive(dataExists);
        targetChipIcon.sprite = !dataExists ? null : raritySprites[data.GetRaritySpriteIndex()];
        installButton.interactable = dataExists;
        unstallButton.interactable = dataExists;
        if (dataExists)
        {
            Transform trans = item.transform;
            if (trans.IsChildOf(eqContainer))
            {
                installButton.interactable = false;
            }
            if (trans.IsChildOf(bagContainer))
            {
                unstallButton.interactable = false;
                if (maxChipEmploy <= nowEquippedAmount)
                {
                    installButton.interactable = false;
                }
            }
        }
    }
    public void Install()
    {
        string equipId = GetCurrentEquipment().item_id;
        MessageBox.Show("您确定您要安装这个芯片吗？这将导致除非您卸载该装备所有芯片，这个装备将无法销毁和出售。", "温馨提示", (result) =>
        {
            if (result == DialogResult.Yes)
            {
                StartCoroutine(_Install(targetData, equipId));
            }
        }, MessageBoxButtons.YesNo);
    }
    IEnumerator _Install(ChipData data, string id)
    {
        yield return ChipManager.RequestInstallChip(data, id);
        yield return _OnShowAndRefresh();
    }
    public void Unstall()
    {
        //string equipId = GetCurrentEquipment().item_id;
        MessageBox.Show("您确定您要卸载这个芯片吗？这将导致您的芯片<color=yellow>等级下降一半，当前杀敌数清空。</color>", "温馨提示", (result) =>
        {
            if (result == DialogResult.Yes)
            {
                StartCoroutine(_Unstall(targetData));
            }
        }, MessageBoxButtons.YesNo);
    }
    IEnumerator _Unstall(ChipData data)
    {
        yield return ChipManager.RequestUnstallChip(data);
        yield return _OnShowAndRefresh();
    }
    public void Drop()
    {
        //string equipId = GetCurrentEquipment().item_id;
        MessageBox.Show("您确定您要(卸载并)丢弃这个芯片吗？", "温馨提示", (result) =>
        {
            if (result == DialogResult.Yes)
            {
                StartCoroutine(_Drop(targetData));
            }
        }, MessageBoxButtons.YesNo);
    }
    IEnumerator _Drop(ChipData data)
    {
        yield return ChipManager.RequestDropChip(data);
        SelectAndShowInPanel(null);
        yield return _OnShowAndRefresh();
    }
    public void Upgrade()
    {
       // string equipId = GetCurrentEquipment().item_id;
        StringBuilder sb = new StringBuilder();
        var piecesAmount = ItemDataManager.GetItemAmount(Items.SPB_PIECE);
        bool canUpgrade = piecesAmount >= targetData.GetNextLevelNeedSpbPieces();
        sb.AppendLine("升级这个芯片需要：");
        sb.AppendFormat("   ·原灵碎片 x {0}", targetData.GetNextLevelNeedSpbPieces());
        if (!canUpgrade)
        {
            sb.AppendLine();
            sb.Append("你当前没有这么多的原灵碎片。");
        }
        MessageBox.Show(sb.ToString(), "温馨提示", (result) =>
        {
            if (result == DialogResult.Yes)
            {
                StartCoroutine(_Upgrade(targetData));
            }
        }, canUpgrade ? MessageBoxButtons.YesNo : MessageBoxButtons.OK);
    }
    IEnumerator _Upgrade(ChipData data)
    {
        yield return ChipManager.RequestUpgradeChip(data);
        yield return _OnShowAndRefresh();
    }
    /// <summary>
    /// 获取装备描述，将会根据其所装载的芯片进行调整。
    /// </summary>
    /// <returns></returns>
    string GetEquipmentDescription()
    {
        var equipment = GetCurrentEquipment();
        StringBuilder contentSb = new StringBuilder();
        contentSb.AppendLine("<size=14><b>" + equipment.GetEqTypeName() + "</b></size>");
        contentSb.AppendLine(TextUtils.GetSpbText("灵基 -- " + equipment.GetSpb().ToString("0.0")));
        contentSb.AppendLine(equipment.description);
        EquipmentValue value = equipment.GetAttrs();
        var srcAttrs = value.values;
        var chips = equipment.LinkedChips;
        foreach (var attr in AttributeCollection.GetAllAttributes())
        {
            if (srcAttrs.GetValue(attr) != 0f)
            {
                contentSb.AppendLine();
                contentSb.AppendFormat("<size=12>{0} {1}</size>", attr.Name, srcAttrs.GetValueToString(attr));
                if (chips.Count != 0)
                {
                    for (int i = 0; i < chips.Count; i++)
                    {
                        var chip = chips[i];
                        if (chip.IsEffect(Effects.ATTR))
                        {
                            var chipAttr = chip.GetAttrColl();
                            if (chipAttr[attr] != 0f)
                            {
                                contentSb.AppendFormat("<color=lime>(+{0})</color>", (int)(srcAttrs[attr] * chipAttr[attr]));
                            }
                        }
                    }
                }
            }
        }
        return contentSb.ToString();
    }
    EquipmentBase GetCurrentEquipment()
    {
        return (currentEqList.Count > currentWeaponIndex && currentWeaponIndex != -1) ? currentEqList[currentWeaponIndex] : null;
    }
    #region 对象池相关
    /// <summary>
    /// 生成ChipUIItem到指定地点，并且设置应当设置的东西。
    /// </summary>
    /// <param name="data"></param>
    /// <param name="parent"></param>
    ChipUIItem Spawn(ChipData data, Transform parent)
    {
        ChipUIItem item = spawnPool.Spawn(chipItemPrefab, parent).GetComponent<ChipUIItem>();
        item.SetLinkedData(data, raritySprites[data.GetRaritySpriteIndex()]);
        item.toggle.group = group;
        item.onToggled += SelectAndShowInPanel;
        return item;
    }
    /// <summary>
    /// 将指定容器里的对象Despawn掉。
    /// </summary>
    /// <param name="container"></param>
    void DespawnAllInContainer(Transform container)
    {
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            if (spawnPool.IsSpawned(child)) spawnPool.Despawn(container.GetChild(i));
        }
    }
    #endregion
}
