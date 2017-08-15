using GameId;
using SerializedClassForJson;
using SerializedClassForJson.Chip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit
{
    private const string GET_CHIP_PATH = "scripts/player/equipment/chip/getAllChips.php";
    private const string UPDATE_ITEMS_FILEPATH = "scripts/player/item/loaditems.php";
    private const string GET_SKILLS_FILEPATH = "scripts/player/skill/getSkillDatas.php";
    private static string UPDATE_PLAYERINFO_FILE_PATH = "scripts/player/getPlayerInfo.php";

    string id;
    public string nickname;
    public lint level;
    public lint challRank;
    public int designId;

    List<EquipmentBase> equippedEqs;
    List<ChipData> equippedChips;
    Dictionary<BaseSkill, int> equippedSkills;
    AttributeCollection attrColl;

    public string Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
        }
    }
    public IEnumerator InitUnitProperty()
    {
        equippedEqs = new List<EquipmentBase>();
        equippedChips = new List<ChipData>();
        equippedSkills = new Dictionary<BaseSkill, int>();
        attrColl = new AttributeCollection();
        CU.ShowConnectingUI();
        //先获取芯片
        WWWForm form = new WWWForm();
        form.AddField("id", id);
        WWW w = new WWW(CU.ParsePath(GET_CHIP_PATH), form);
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            TempChipRawData[] rawDatas = JsonHelper.GetJsonArray<TempChipRawData>(w.text);
            if (!EArray.IsNullOrEmpty(rawDatas))
            {
                for (int i = 0; i < rawDatas.Length; i++)
                {
                    if (rawDatas[i].equip_id != -1)
                    {
                        equippedChips.Add(new ChipData(rawDatas[i]));
                    }
                }
            }
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
        //装备
        form = new WWWForm();
        form.AddField("playerId", id);
        form.AddField("type", 1);
        w = new WWW(CU.ParsePath(UPDATE_ITEMS_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != null)
        {
            TempEquipment[] tempEquips = JsonHelper.GetJsonArray<TempEquipment>(w.text);

            if (!EArray.IsNullOrEmpty(tempEquips))
            {
                Dictionary<string, EquipmentBase> idToEq = new Dictionary<string, EquipmentBase>();
                foreach (TempEquipment temp in tempEquips)
                {
                    if (temp.isEquipped && !temp.isInStorage)
                    {
                        var eq = EquipmentFactory.CreateEquipment(temp);
                        equippedEqs.Add(eq);
                        idToEq.Add(eq.item_id, eq);
                    }
                }
                foreach (var data in equippedChips)
                {
                    var dataId = data.GetEquippedId().ToString();
                    if (idToEq.ContainsKey(dataId))
                    {
                        idToEq[dataId].AddChip(data);
                    }
                }
            }
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
        //技能
        form = new WWWForm();
        form.AddField("id", id);
        w = new WWW(CU.ParsePath(GET_SKILLS_FILEPATH), form);
        yield return w;
        if (w.isDone && w.text != "failed")
        {
            TempSkills skills = JsonUtility.FromJson<TempSkills>(w.text);
            if (!EArray.IsNullOrEmpty(skills.datas))
            {
                for (int i = 0; i < skills.datas.Length; i++)
                {
                    TempSkillData data = skills.datas[i];
                    if (!data.equipped) continue;
                    BaseSkill skill = SkillDataManager.GetSkillByKey(data.idkey);
                    equippedSkills.Add(skill, data.level);
                }
            }
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
        //玩家属性
        TempPlayerAttribute tempPlayerAttri = null;
        form = new WWWForm();
        form.AddField("id", id);
        form.AddField("type", 0);
        form.AddField("notSelf", 1);
        w = new WWW(CU.ParsePath(UPDATE_PLAYERINFO_FILE_PATH), form);
        yield return w;
        if (CU.IsPostSucceed(w))
        {
            string jsonText = w.text;
            tempPlayerAttri = JsonUtility.FromJson<TempPlayerAttribute>(jsonText);
        }
        else
        {
            CU.ShowConnectFailed();
            yield break;
        }
        //开始设置属性
        //原始属性
        attrColl.SetValues(tempPlayerAttri);
        //装备属性
        for (int i = 0; i < equippedEqs.Count; i++)
        {
            EquipmentBase equipment = equippedEqs[i];
            var attrCollt = equipment.GetActualAttrs();
            attrColl += attrCollt;
        }
        //称号属性
        var dedata = DesignationManager.GetDesignationData(designId);
        if (dedata != null)
        {
            attrColl.MultValues(dedata);
        }
        CU.HideConnectingUI();
    }

    public BattleUnit CreateBattleUnit(BattleUnit.SIDE side)
    {
        TempPropertyRecord record = new TempPropertyRecord(attrColl);
        Dictionary<BeforeBattleModiSkill, int> beforeSkills = new Dictionary<BeforeBattleModiSkill, int>();
        Dictionary<BaseSkill, int> duringSkills = new Dictionary<BaseSkill, int>();
        foreach (var kv in equippedSkills)
        {
            if (kv.Key is BeforeBattleModiSkill)
            {
                if (kv.Value > 0)
                {
                    beforeSkills.Add(kv.Key as BeforeBattleModiSkill, kv.Value);
                }
            }
            if (kv.Key is DuringBattleTriggerSkill)
            {
                if (kv.Value > 0)
                {
                    duringSkills.Add(kv.Key, kv.Value);
                }
            }
        }
        UnitModifyManager.ModifyRecordBeforeBattle(beforeSkills, record);
        BattleUnit unit = new BattleUnit(id, nickname, level, side, record);
        record.hp = record.GetValue(Attrs.MHP);
        record.mp = record.GetValue(Attrs.MMP);
        unit.SetSkills(duringSkills);
        return unit;
    }
}
