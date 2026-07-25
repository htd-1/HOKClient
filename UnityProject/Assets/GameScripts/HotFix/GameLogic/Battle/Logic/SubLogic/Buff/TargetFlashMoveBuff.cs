using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// 目标闪现跳跃buff(buffID=10130),逻辑待迁移。
    /// </summary>
    public class TargetFlashMoveBuff : Buff
    {
        private PEInt _offset;
        public TargetFlashMoveBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            TargetFlashMoveBuffCfg tfmbc=Cfg as TargetFlashMoveBuffCfg;
            _offset = (PEInt)tfmbc.Offset;
        }

        protected override void Start()
        {
            base.Start();

            MainLogicUnit target = CalcRule.FindSingleTargetByRule(
                Owner,Skill.Cfg.TargetCfg,PEVector3.zero);
            if (target == null)
            {
                UnitState = SubUnitState.End;
                return;
            }
            PEVector3 disVec =target.LogicPos-Owner.LogicPos;

            Owner.LogicPos += disVec.normalized * (disVec.magnitude - _offset);
            
        }
    }
}
