using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buff
{
    /// <summary>
    /// 战斗时的Buff。
    /// </summary>
    public abstract class CombatBuff
    {
        protected string _id;
        protected lint _remainCourses;
        protected lint _buffLevel;
        public abstract float GetEffectLevel();
        public abstract bool IsDebuff();
        public abstract void Modify();
        public abstract void Unmodify();
        public abstract CombatBuff Create(string id,lint lastRound,lint buffLevel);
        /// <summary>
        /// 是否已经Buff时间已到
        /// </summary>
        /// <returns></returns>
        public bool Elapsed()
        {
            return _remainCourses <= 0;
        }
        /// <summary>
        /// 回合数-1
        /// </summary>
        public void Advance()
        {
            _remainCourses--;
        }
    }
}
