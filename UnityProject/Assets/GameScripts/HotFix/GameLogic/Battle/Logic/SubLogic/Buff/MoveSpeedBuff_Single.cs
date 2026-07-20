using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    public class MoveSpeedBuff_Single:Buff
    {
        private PEInt _speedOffset;
        public MoveSpeedBuff_Single(MainLogicUnit source, MainLogicUnit owner, 
            Skill skill, int buffID, object[] args = null) 
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            
            MoveSpeedBuffCfg msbc=Cfg as MoveSpeedBuffCfg;

            _speedOffset = Owner.MoveSpeedBase * ((PEInt)msbc.Amount / 100);
            
        }

        protected override void Start()
        {
            base.Start();
            Owner.ModifyMoveSpeed(_speedOffset,this,true);
        }

        protected override void End()
        {
            base.End();
            Owner.ModifyMoveSpeed(-_speedOffset,this,false);
        }
    }
}