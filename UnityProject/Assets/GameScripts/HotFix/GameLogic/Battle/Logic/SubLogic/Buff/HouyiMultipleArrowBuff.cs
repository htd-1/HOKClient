namespace GameLogic
{
    /// <summary>
    /// 后羿被动多重射击buff(buffID=10250),逻辑待迁移。
    /// </summary>
    public class HouyiMultipleArrowBuff : Buff
    {
        public HouyiMultipleArrowBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
