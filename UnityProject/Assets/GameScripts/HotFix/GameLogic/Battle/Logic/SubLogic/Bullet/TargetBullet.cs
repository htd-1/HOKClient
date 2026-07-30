using System;
using PEMath;

namespace GameLogic
{
    public class TargetBullet:Bullet
    {
        /// <summary>
        /// 目标
        /// </summary>
        protected MainLogicUnit Target;

        public Action<MainLogicUnit, object[]> HitTargetCB;

        private readonly PEVector3 _hitHeight;
        
        public TargetBullet(MainLogicUnit source, MainLogicUnit target,Skill skill)
            : base(source, skill)
        {
            Target = target;
            _hitHeight =new PEVector3(0,
                Target.UnitData.UnitCfg.HitHeight,0);
        }

        public override void LogicInit()
        {
            base.LogicInit();

            PEVector3 targetPos = Target.LogicPos + _hitHeight;
            LogicDir = (targetPos - LogicPos).normalized;

            //子弹基于中心点的偏移
            LogicPos += LogicDir * (PEInt)Cfg.BulletOffset;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Tick()
        {
            base.Tick();
            
            LogicDir=(Target.LogicPos+_hitHeight-LogicPos).normalized;

            if (LogicDir == PEVector3.zero)
            {
                UnitState = SubUnitState.End;
                return;
            }

            if (Target.UnitState == GameLogic.UnitState.Dead)
            {
                UnitState = SubUnitState.End;
                return;
            }

            LogicPos += LogicDir * LogicMoveSpeed;
        }

        protected override void End()
        {
            base.End();
        }
    }
}