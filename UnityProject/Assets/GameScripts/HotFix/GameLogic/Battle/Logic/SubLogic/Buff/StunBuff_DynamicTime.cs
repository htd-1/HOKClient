namespace GameLogic
{
    /// <summary>
    /// 动态时间眩晕buff(buffID=10230),逻辑待迁移。
    /// </summary>
    public class StunBuff_DynamicTime : Buff
    {
        public StunBuff_DynamicTime(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
