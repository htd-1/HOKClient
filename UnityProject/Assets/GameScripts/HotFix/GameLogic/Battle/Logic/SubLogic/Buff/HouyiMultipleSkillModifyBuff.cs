namespace GameLogic
{
    /// <summary>
    /// 后羿被动普攻修改buff(buffID=10201),逻辑待迁移。
    /// </summary>
    public class HouyiMultipleSkillModifyBuff : Buff
    {
        public HouyiMultipleSkillModifyBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
