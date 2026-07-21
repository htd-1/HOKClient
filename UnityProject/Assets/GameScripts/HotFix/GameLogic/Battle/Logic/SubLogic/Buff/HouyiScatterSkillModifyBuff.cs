namespace GameLogic
{
    /// <summary>
    /// 后羿主动技能强化普攻buff(buffID=10210),逻辑待迁移。
    /// </summary>
    public class HouyiScatterSkillModifyBuff : Buff
    {
        public HouyiScatterSkillModifyBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
