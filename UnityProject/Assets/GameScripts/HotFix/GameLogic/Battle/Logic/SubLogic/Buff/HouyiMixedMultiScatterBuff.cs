namespace GameLogic
{
    /// <summary>
    /// 后羿混合多重散射buff(buffID=10260),逻辑待迁移。
    /// </summary>
    public class HouyiMixedMultiScatterBuff : Buff
    {
        public HouyiMixedMultiScatterBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
