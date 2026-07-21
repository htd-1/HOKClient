namespace GameLogic
{
    /// <summary>
    /// 移动攻击buff(buffID=90000),逻辑待迁移。
    /// </summary>
    public class MoveAttackBuff : Buff
    {
        public MoveAttackBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
