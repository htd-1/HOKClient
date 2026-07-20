using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// 血量回复buff
    /// </summary>
    public class HPCureBuff_Single:Buff
    {
        public PEInt CureHPpct;
        public HPCureBuff_Single(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID,
            object[] args = null) : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            
            HPCureBuffCfg hcbc=Cfg as HPCureBuffCfg;
            CureHPpct = hcbc.CureHPpct;
        }

        protected override void Tick()
        {
            base.Tick();
            if (Owner.UnitState == GameLogic.UnitState.Alive)
            {
                Owner.GetCureByBuff(Owner.UnitData.UnitCfg.Hp*CureHPpct/100,this);
                
            }
        }
    }
}