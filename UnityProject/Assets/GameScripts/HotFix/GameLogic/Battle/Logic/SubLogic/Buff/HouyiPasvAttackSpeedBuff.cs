namespace GameLogic
{
    /// <summary>
    /// 后羿被动攻速叠加buff(buffID=10200),逻辑待迁移。
    /// </summary>
    public class HouyiPasvAttackSpeedBuff : Buff
    {
        public HouyiPasvAttackSpeedBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
