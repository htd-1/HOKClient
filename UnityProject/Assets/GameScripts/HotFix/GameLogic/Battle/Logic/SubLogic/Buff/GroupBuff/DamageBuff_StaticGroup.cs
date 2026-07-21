namespace GameLogic
{
    /// <summary>
    /// 静态群体伤害buff(buffID=10133/10220/10221/10231),逻辑待迁移。
    /// </summary>
    public class DamageBuff_StaticGroup : Buff
    {
        public DamageBuff_StaticGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
