namespace GameLogic
{
    /// <summary>
    /// 静态群体加速buff(buffID=10222/10223),逻辑待迁移。
    /// </summary>
    public class MoveSpeedBuff_StaticGroup : Buff
    {
        public MoveSpeedBuff_StaticGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }
    }
}
