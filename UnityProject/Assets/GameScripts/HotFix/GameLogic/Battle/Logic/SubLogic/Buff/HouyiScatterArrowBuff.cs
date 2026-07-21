namespace GameLogic
{
    /// <summary>
    /// 后羿散射箭buff(buffID=10240),逻辑待迁移。
    /// </summary>
    public class HouyiScatterArrowBuff : Buff
    {
        public HouyiScatterArrowBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
