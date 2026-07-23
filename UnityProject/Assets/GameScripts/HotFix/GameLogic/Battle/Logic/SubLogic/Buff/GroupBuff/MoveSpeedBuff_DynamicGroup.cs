using System.Collections.Generic;
using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// 动态群体加速buff(buffID=10142),逻辑待迁移。
    /// </summary>
    public class MoveSpeedBuff_DynamicGroup : Buff
    {
        private PEInt _speedOffset; 
        public MoveSpeedBuff_DynamicGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();

            _targetList = new List<MainLogicUnit>();
            
            _targetList.AddRange(CalRule.FindMultipleTargetByRUle(Owner,Cfg.Impacter,Skill.SkillArgs));
            
            MoveSpeedBuffCfg msbc=Cfg as MoveSpeedBuffCfg;
            _speedOffset = msbc.Amount;
        }

        protected override void Start()
        {
            base.Start();
            
            ModifyMoveSpeed(_speedOffset,true);
        }


        protected override void Tick()
        {
            base.Tick();
            ModifyMoveSpeed(-_speedOffset);
            
            _targetList.Clear();
            
            _targetList.AddRange(CalRule.
                FindMultipleTargetByRUle
                (Owner,Cfg.Impacter,Skill.SkillArgs));
            ModifyMoveSpeed(_speedOffset);
            
        }

        protected override void End()
        {
            base.End();
            ModifyMoveSpeed(-_speedOffset);
            _targetList.Clear();
            _targetList = null;
        }

        private void ModifyMoveSpeed(PEInt value, bool show=false)
        {
            for (int i = 0; i < _targetList.Count; i++)
            {
                PEInt offset = _targetList[i].MoveSpeedBase * (value / 100);
                _targetList[i].ModifyMoveSpeed(offset, this, show);
            }
        }
    }
}
