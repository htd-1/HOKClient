using System.Collections.Generic;
using GameConfig.hok;
using PEMath;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 静态群体加速buff(buffID=10222/10223),逻辑待迁移。
    /// </summary>
    public class MoveSpeedBuff_StaticGroup : Buff
    {
        private PEInt _speedOffset; 
        public MoveSpeedBuff_StaticGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
            _targetList = new List<MainLogicUnit>();
        }

        public override void LogicInit()
        {
            base.LogicInit();
            // Log.Info("被创建了一次");
            
            MoveSpeedBuffCfg msbc=Cfg as MoveSpeedBuffCfg;
            _speedOffset = msbc.Amount;
            _targetList.AddRange(CalcRule.FindMultipleTargetByRUle(Owner,Cfg.Impacter,Skill.SkillArgs));

            switch (msbc.StaticPosType)
            {
                case StaticPosType.SkillCasterPos:
                    LogicPos = Source.LogicPos;
                    break;
                case StaticPosType.SkillLockTargetPos:
                    LogicPos=Skill.LockTarget.LogicPos;
                    break;
                case StaticPosType.BulletHitTargetPos:
                    LogicPos = (PEVector3)args[1];
                    break;
                case StaticPosType.UIInputPos:
                    LogicPos=Source.LogicPos+Skill.SkillArgs;
                    break;
                case StaticPosType.None:
                    break;
                default:
                    Log.Error("static buff pos error");
                    break;
                
            }
        }

        protected override void Start()
        {
            base.Start();
            _targetList.AddRange(CalcRule.FindMultipleTargetByRUle(
                Source,Cfg.Impacter,LogicPos));
            
            ModifyMoveSpeed(_speedOffset,true);
        }

        protected override void Tick()
        {
            base.Tick();
            ModifyMoveSpeed(-_speedOffset);
            
            _targetList.Clear();
            _targetList.AddRange(CalcRule.FindMultipleTargetByRUle(
                Source,Cfg.Impacter,LogicPos));
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
