namespace GameLogic
{
    /// <summary>
    /// 目标闪现跳跃buff(buffID=10130),逻辑待迁移。
    /// </summary>
    public class TargetFlashMoveBuff : Buff
    {
        public TargetFlashMoveBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
