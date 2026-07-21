namespace GameLogic
{
    public class SilenceBuff_Single:Buff
    {
        public SilenceBuff_Single(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        protected override void Start()
        {
            base.Start();

            Owner.SilenceCount += 1;
            
        }

        protected override void End()
        {
            base.End();
            
            Owner.SilenceCount -= 1;
        }
    }
}