using HOKProtocol;

namespace GameLogic
{
    public enum SubUnitState
    {
        None,
        Delay,
        Start,
        Tick,
        End,
    }
    public abstract class SubLogicUnit:LogicUnit
    {
        public MainLogicUnit Source;

        protected Skill Skill;

        /// <summary>
        /// 延迟生效时间
        /// </summary>
        protected int _delayTime;
        /// <summary>
        /// 延迟时间计数
        /// </summary>

        protected int _delayCounter;

        /// <summary>
        /// 辅助单元状态
        /// </summary>
        public SubUnitState UnitState;
        
        public SubLogicUnit(MainLogicUnit source, Skill skill)
        {
            Source = source;
            Skill = skill;
        }
        public override void LogicInit()
        {
            if (_delayTime == 0)
            {
                UnitState = SubUnitState.Start;
            }
            else
            {
                _delayCounter = _delayTime;
                UnitState = SubUnitState.Delay;
            }
        }

        public override void LogicTick()
        {
            switch (UnitState)
            {
                case SubUnitState.Delay:
                    _delayCounter -= ServerConfig.ServerLogicFrameIntervelMs;
                    if (_delayCounter <= 0)
                    {
                        UnitState = SubUnitState.Start;
                    }
                    break;
                case SubUnitState.End:
                    End();
                    UnitState = SubUnitState.None;
                    break;
                case SubUnitState.None:
                default:
                    break;
            }
        }

        public override void LogicUnInit()
        {
            
        }

        protected abstract void Start();
        protected abstract void Tick();
        protected abstract void End();
    }
}