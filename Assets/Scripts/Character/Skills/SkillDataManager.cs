using GameId;
using SerializedClassForJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 用于管理技能的类
/// </summary>
public class SkillDataManager : MonoBehaviour
{
    public static Dictionary<string, BaseSkill> IDS_TO_SKILLS = new Dictionary<string, BaseSkill>();
    public static List<BaseSkill> SKILLS_LIST = new List<BaseSkill>();
    public static SkillDataManager Instance { get; set; }

    public Dictionary<string, Sprite> icon_name_to_sprite = new Dictionary<string, Sprite>();

    static SkillDataManager()
    {
        //不能随意改变顺序，顺序代表key
        RegisterSkill(new Skill_MachineGenerate("machine_generate"));
        RegisterSkill(new Skill_SkillfulAttack("skillful_attack"));
        RegisterSkill(new Skill_UniverseThoughts("universe_thoughts"));
        RegisterSkill(new Skill_IronWall("iron_wall"));
        RegisterSkill(new Skill_AnitcriShield(Skills.ANTICRI_SHIELD));
        RegisterSkill(new Skill_DyingBreak(Skills.DYING_BREAK));
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {

    }
    /// <summary>
    /// 获得记录的Sprite，如果没有则向网络下载
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Sprite GetSpriteById(string id)
    {
        if (!IDS_TO_SKILLS.ContainsKey(id))
        {
            return null;
        }
        string path = IDS_TO_SKILLS[id].GetIconPath();
        if (SpriteLibrary.GetSprite(path) != null)
        {
            return SpriteLibrary.GetSprite(path);
        }
        else
        {
            Instance.StartCoroutine(Instance.RequestLoadTexture(path));
            return null;
        }
    }
    /// <summary>
    /// 读取材质，就是说技能图标
    /// </summary>
    /// <param name="iconName"></param>
    /// <returns></returns>
    IEnumerator RequestLoadTexture(string path)
    {
        if (!SpriteLibrary.IsSpriteDownLoading(path))
        {
            SpriteLibrary.SetSpriteDownLoading(path);
            ConnectUtils.ShowConnectingUI();
            WWW w = new WWW(ConnectUtils.ParsePath(path));
            yield return w;
            if (ConnectUtils.IsDownloadCompleted(w))
            {
                Texture2D iconTex = w.texture;
                iconTex.Compress(true);
                Sprite _icon = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
                ///预加载
                SpriteLibrary.AddSprite(path, _icon);
                ///
                //Debug.Log(path);
            }
            else
            {
                Debug.LogWarning(w.error);
                ConnectUtils.ShowConnectFailed();
            }
            ConnectUtils.HideConnectingUI();
            w.Dispose();
        }
    }

    /// <summary>
    /// 通过技能id来识别拥有技能。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static BaseSkill GetSkillById(string id)
    {
        return IDS_TO_SKILLS.ContainsKey(id) ? IDS_TO_SKILLS[id] : null;
    }
    /// <summary>
    /// 通过技能idkey来识别
    /// </summary>
    /// <param name="idkey"></param>
    /// <returns></returns>
    public static BaseSkill GetSkillByKey(int idkey)
    {
        return SKILLS_LIST.Count > idkey ? SKILLS_LIST[idkey] : null;
    }

    /// <summary>
    /// 注册一个技能。
    /// </summary>
    /// <param name="skill"></param>
    static void RegisterSkill(BaseSkill skill)
    {
        IDS_TO_SKILLS.Add(skill.GetId(), skill);
        SKILLS_LIST.Add(skill);
    }
    /// <summary>
    /// 生成技能序列化类,内含所有技能当前数据。用于请求数据库更新
    /// </summary>
    /// <returns></returns>
    public static TempSkills GenerateTempSkills()
    {
        TempSkills skills = new TempSkills();
        skills.datas = new TempSkillData[SKILLS_LIST.Count];
        int index = 0;
        for (int i = 0; i < SKILLS_LIST.Count; i++)
        {
            BaseSkill skill = SKILLS_LIST[i];
            TempSkillData data = new TempSkillData();
            data.idkey = i;
            data.level = skill.Level;
            data.equipped = skill.Equipped;
            skills.datas[index++] = data;
        }
        return skills;
    }
    /// <summary>
    /// 用TempSkills来设置当前skills的属性,用于从数据库获得属性后更新本地
    /// </summary>
    /// <param name="skills"></param>
    public static void SetSkillsByTempSkills(TempSkills skills)
    {
        for (int i = 0; i < skills.datas.Length; i++)
        {
            TempSkillData data = skills.datas[i];
            BaseSkill skill = SKILLS_LIST[data.idkey];
            skill.Level = data.level;
            skill.Equipped = data.equipped;
        }
        SkillSceneManager.Instance.MakeDirty();
    }

    /// <summary>
    /// 获得玩家所有拥有且装备着的战前属性加成技能
    /// </summary>
    /// <returns></returns>
    public static Dictionary<BeforeBattleModiSkill, int> GetPlayersBeforeBattleSkill()
    {
        Dictionary<BeforeBattleModiSkill, int> skills = new Dictionary<BeforeBattleModiSkill, int>();
        foreach (BaseSkill skill in SKILLS_LIST)
        {
            if (skill is BeforeBattleModiSkill)
            {
                if (skill.Level > 0 && skill.Equipped)
                {
                    skills.Add(skill as BeforeBattleModiSkill, skill.Level);
                }
            }
        }
        return skills;
    }
    public static Dictionary<BaseSkill, int> GetPlayersDuringBattleSkill()
    {
        Dictionary<BaseSkill, int> skills = new Dictionary<BaseSkill, int>();
        foreach (BaseSkill skill in SKILLS_LIST)
        {
            if (skill is DuringBattleTriggerSkill)
            {
                if (skill.Level > 0 && skill.Equipped)
                {
                    skills.Add(skill, skill.Level);
                }
            }
        }
        return skills;
    }
}
