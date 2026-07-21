namespace GameLogic
{
    /// <summary>
    /// 动态群体伤害buff(buffID=10120),逻辑待迁移。
    /// </summary>
    public class DamageBuff_DynamicGroup : Buff
    {
        public DamageBuff_DynamicGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
