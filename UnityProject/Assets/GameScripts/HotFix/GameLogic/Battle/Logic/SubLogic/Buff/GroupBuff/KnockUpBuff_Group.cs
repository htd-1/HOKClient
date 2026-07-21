namespace GameLogic
{
    /// <summary>
    /// 群体击飞buff(buffID=10132),逻辑待迁移。
    /// </summary>
    public class KnockUpBuff_Group : Buff
    {
        public KnockUpBuff_Group(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
