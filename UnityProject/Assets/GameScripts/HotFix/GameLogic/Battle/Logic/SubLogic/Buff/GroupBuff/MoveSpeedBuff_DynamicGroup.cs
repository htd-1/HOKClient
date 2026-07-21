namespace GameLogic
{
    /// <summary>
    /// 动态群体加速buff(buffID=10142),逻辑待迁移。
    /// </summary>
    public class MoveSpeedBuff_DynamicGroup : Buff
    {
        public MoveSpeedBuff_DynamicGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
