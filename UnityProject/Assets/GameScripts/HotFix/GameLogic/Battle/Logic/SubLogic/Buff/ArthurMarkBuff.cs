using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// Arthur标记buff(buffID=10141),逻辑待迁移。
    /// </summary>
    public class ArthurMarkBuff : Buff
    {
        private PEInt _damagePct;
        private MainLogicUnit _target;
        public ArthurMarkBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            
            ArthurMarkBuffCfg ambc=Cfg as ArthurMarkBuffCfg;
            _damagePct = ambc.DamagePct;

            _target = Skill.LockTarget;
        }

        protected override void Start()
        {
            base.Start();
            _target.OnHurt += GetHurt;
        }

        private void GetHurt()
        {
            _target.GetDamageByBuff(_damagePct/100*_target.UnitData.UnitCfg.Hp,this,false);
        }

        protected override void End()
        {
            base.End();
            _target.OnHurt -= GetHurt;
        }
    }
}
