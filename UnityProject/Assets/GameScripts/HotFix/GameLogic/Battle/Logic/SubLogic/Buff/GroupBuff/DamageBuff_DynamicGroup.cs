using System.Collections.Generic;
using PEMath;
using GameConfig.hok;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 动态群体伤害buff(buffID=10120),逻辑待迁移。
    /// </summary>
    public class DamageBuff_DynamicGroup : Buff
    {
        private PEInt _damage;
        public DamageBuff_DynamicGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();

            _targetList = new List<MainLogicUnit>();
            
            
            DamageDynamicGroupBuffCfg gdbc=Cfg as DamageDynamicGroupBuffCfg; ;
            _damage = gdbc.Damage;
            
        }

        protected override void Tick()
        {
            base.Tick();
            CalcGroupDamage();
        }
        

        private void CalcGroupDamage()
        {
            _targetList.Clear();
            _targetList.AddRange(
                CalcRule.FindMultipleTargetByRUle(
                    Owner,Cfg.Impacter,PEVector3.zero));
            for (int i = 0; i < _targetList.Count; i++)
            {
                _targetList[i].GetDamageByBuff(_damage,this);
            }
        }
    }
}
