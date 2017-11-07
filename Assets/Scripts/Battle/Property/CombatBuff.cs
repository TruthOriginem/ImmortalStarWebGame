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
        private BattleUnit source;
        private BattleUnit unit;
        public BattleUnit Source
        {
            get
            {
                return source;
            }
        }
        public BattleUnit Unit
        {
            get
            {
                return unit;
            }
        }

        public abstract float GetEffectLevel();
        public abstract bool IsDebuff();
        public abstract bool IsUnique();
        public abstract void Modify();
        public abstract void Unmodify();
        public abstract CombatBuff Create(BattleUnit source, BattleUnit unit, lint lastRound, lint buffLevel);
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
    public class TestBuff : CombatBuff
    {
        public override CombatBuff Create(BattleUnit source, BattleUnit unit, lint lastRound, lint buffLevel)
        {
            return new TestBuff();
        }

        public override float GetEffectLevel()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsDebuff()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsUnique()
        {
            throw new System.NotImplementedException();
        }

        public override void Modify()
        {
            throw new System.NotImplementedException();
        }

        public override void Unmodify()
        {
            throw new System.NotImplementedException();
        }
    }
}
