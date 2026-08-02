using GameConfig.hok;

namespace GameLogic
{
    /// <summary>
    /// 后羿主动技能强化普攻buff(buffID=10210),逻辑待迁移。
    /// </summary>
    public class HouyiScatterSkillModifyBuff : Buff
    {
        private int _originalID;
        private int _powerID;
        private int _superID;
        private Skill _modifySkill;
        public HouyiScatterSkillModifyBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            HouyiScatterSkillModifyBuffCfg hsmb = Cfg as HouyiScatterSkillModifyBuffCfg;
            _originalID = hsmb.OriginalID;
            _powerID = hsmb.PowerID;
            _superID = hsmb.SuperPowerID;
            _modifySkill=Owner.GetSkillByID(_originalID);
        }

        protected override void Start()
        {
            base.Start();

            if (_modifySkill.TempSkillID == 0)
            {
                _modifySkill.ReplaceSkillCfg(_powerID);
            }
            else
            {
                _modifySkill.ReplaceSkillCfg(_superID);
            }
        }

        protected override void End()
        {
            base.End();

            if (_modifySkill.TempSkillID == _powerID)
            {
                _modifySkill.ReplaceSkillCfg(_originalID);
            }
            else
            {
                _modifySkill.ReplaceSkillCfg(1025);
            }
        }
    }
}
