using GameConfig.hok;

namespace GameLogic
{
    /// <summary>
    /// 技能替换buff
    /// </summary>
    public class CommonModifySkillBuff:Buff
    {
        public int OriginalID;
        public int ReplaceID;
        
        private Skill _modifySkill;
        
        public CommonModifySkillBuff(MainLogicUnit source,
            MainLogicUnit owner, Skill skill, int buffID, 
            object[] args = null) : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            
            CommonModifySkillBuffCfg mabc=Cfg as CommonModifySkillBuffCfg;
            OriginalID=mabc.OriginalID;
            ReplaceID=mabc.ReplaceID;
            _modifySkill=Owner.GetSkillByID(OriginalID);
        }

        protected override void Start()
        {
            base.Start();
            
            _modifySkill.ReplaceSkillCfg(ReplaceID);
            _modifySkill.SpellSuccCallback += ReplaceSkillReleaseDone;
        }

        private void ReplaceSkillReleaseDone(Skill skill)
        {
            if (skill.Cfg.IsNormalAttack)
            {
                UnitState = SubUnitState.End;
            }
        }
        protected override void End()
        {
            base.End();
            
            _modifySkill.ReplaceSkillCfg(OriginalID);
            _modifySkill.SpellSuccCallback -= ReplaceSkillReleaseDone;
        }
    }
    
    
}