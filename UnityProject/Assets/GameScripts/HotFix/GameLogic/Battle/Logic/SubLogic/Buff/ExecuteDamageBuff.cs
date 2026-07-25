using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// 百分比生命伤害buff(buffID=10131),逻辑待迁移。
    /// </summary>
    public class ExecuteDamageBuff : Buff
    {
        private PEInt _damagePct;
        // private 
        public ExecuteDamageBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            ExecuteDamageBuffCfg edbc=Cfg as ExecuteDamageBuffCfg;
            _damagePct = edbc.DamagePct;
        }

        protected override void Start()
        {
            base.Start();

            PEInt damage = (_damagePct / 100) * Owner.UnitData.UnitCfg.Hp;
            
            Owner.GetDamageByBuff(damage,this);
        }
    }
}
