namespace GameLogic
{
    /// <summary>
    /// 百分比生命伤害buff(buffID=10131),逻辑待迁移。
    /// </summary>
    public class ExecuteDamageBuff : Buff
    {
        public ExecuteDamageBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
