using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameId
{
    /// <summary>
    /// 通用的ItemId。
    /// </summary>
    public static class Items
    {
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
    }
    public static class Stages
    {
        public const string BUNDLE_1_STAGE_1 = "origin";
        public const string BUNDLE_1_STAGE_2 = "prologue";
        public const string BUNDLE_1_STAGE_3 = "uavcastle";
        public const string BUNDLE_1_STAGE_4 = "uavrefuge";
    }
}
