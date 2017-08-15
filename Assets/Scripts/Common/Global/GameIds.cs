using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameId
{
    public static class Effects
    {
        public const int ATTR = 0;
    }
    /// <summary>
    /// 通用的ItemId。
    /// </summary>
    public static class Items
    {
        public const string MONEY = "money";
        public const string DIMEN = "dimen";
        /// <summary>
        /// 再塑秘石
        /// </summary>
        public const string REHANCE_STONE = "rehance_stone";
        /// <summary>
        /// 再塑灵石
        /// </summary>
        public const string REHANCE_SPAR = "rehance_spar";
        public const string MONEY_CHEST = "money_chest";
        public const string DEEP_GOOD = "deep_good";
        public const string STAR_ASH = "star_ash";
        /// <summary>
        /// 原灵碎片
        /// </summary>
        public const string SPB_PIECE = "spb_piece";
        public const string CARD_EXP_DOUBLE = "card_for_expDouble";
        public const string CARD_DROP_DOUBLE = "card_for_dropDouble";
        public const string EXTREME_CRYSTAL_LV1 = "extreme_crystal_lv1";
        public const string EXTREME_CRYSTAL_LV2 = "extreme_crystal_lv2";
        public const string MM_TICKET = "mm_ticket";
    }
    /// <summary>
    /// 常用的游戏字符串.
    /// </summary>
    public static class Infos
    {
        /// <summary>
        /// 当前血量的百分比
        /// </summary>
        public const string NOW_HP_PERCENT = "now_hp_percent";
        /// <summary>
        /// 概率
        /// </summary>
        public const string CHANCE = "chance";
        public const string COST_ENERGY_FLAT = "cost_energy_flat";
        public const string COST_CURR_ENERGY_PERCENT = "cost_curr_energy_percent";
        public const string COST_MAX_ENERGY_PERCENT = "cost_max_energy_percent";
        public const string ADD_ATK_PERCENT = "add_atk_percent";
        /// <summary>
        /// 消除暴击伤害
        /// </summary>
        public const string ELIMATE_CRI_HIT_PERCENT = "elimate_cri_hit_percent";
    }
    /// <summary>
    /// SyncRequest请求使用的标识id。
    /// </summary>
    public static class Requests
    {
        public const string ID = "id";
        public const string ECKEY = "eckey";
        public const string PLAYER_DATA = "playerData";
        public const string RECORD_DATA = "recordData";
        public const string ITEM_DATA = "itemData";
        public const string EQ_TO_DELETE_DATA = "deleteEqData";
        public const string RND_EQ_GENA_DATA = "rndEquipData";
        public const string EQ_ENHANCE_DATA = "enhanceEquipData";
        public const string EX_LEVEL_DATA = "exLevelAddData";
        public const string GIFT_PACK_DATA = "giftPackData";
        public const string CHIP_HANDLER_DATA = "chipHandlerData";
        public const string MACHINE_MATCH_DATA = "machineMData";
    }
    /// <summary>
    /// 常用的技能id。
    /// </summary>
    public static class Skills
    {
        public const string ANTICRI_SHIELD = "anticri_shield";
        public const string DYING_BREAK = "dying_break";
    }
    /// <summary>
    /// 常用的称号id。
    /// </summary>
    public static class Designations
    {
        public const int NOTHING = -1;
        public const int CXZ = 1;//初心者
        public const int CJZL = 2;//初见真理
        public const int GWZR = 3;//过往之人
        public const int JXTXZ = 4;//极限探寻者
        public const int CZYH = 5;//持之以恒
        public const int JZZ = 6;//集资者
        public const int SYJ = 7;//商业家
        /// <summary>
        /// 极限研究者
        /// </summary>
        public const int JXYJZ = 9;
        public const int XZCS = 10;
    }
    public static class Stages
    {
        public const string BUNDLE_1_STAGE_1 = "origin";
        public const string BUNDLE_1_STAGE_2 = "prologue";
        public const string BUNDLE_1_STAGE_3 = "uavcastle";
        public const string BUNDLE_1_STAGE_4 = "uavrefuge";
    }
    /// <summary>
    /// 游戏属性集合
    /// </summary>
    public static class Attrs
    {
        public static readonly Attr ATK = new Attr("atk", "攻击");
        public static readonly Attr DEF = new Attr("def", "防御");
        public static readonly Attr LOG = new Attr("log", "逻辑");
        public static readonly Attr LCK = new Attr("lck", "幸运");
        public static readonly Attr CRI = new Attr("cri", "暴击倍率");
        public static readonly Attr MHP = new Attr("mhp", "最大生命");
        public static readonly Attr MMP = new Attr("mmp", "最大能量");
    }
    public class Attr
    {
        public string Id;
        public string Name;
        public Attr(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public override string ToString()
        {
            return Id;
        }
    }
}
public enum ItemPacks
{
    SIGN_IN = 0,
    VIP_NORMAL = 1
}