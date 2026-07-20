using System.Collections.Generic;
using dnlib.DotNet.Writer;
using GameConfig.hok;
using HOKProtocol;

namespace GameLogic
{
    public class Buff:SubLogicUnit
    {
        public MainLogicUnit Owner;
        protected int BuffID;
        protected object[] args;

        protected int BuffDuration;

        private int _tickCount = 0;//Dot
        private int _durationCount = 0;//时长计时
        public BuffCfg Cfg;
        /// <summary>
        /// 群体buff作用目标
        /// </summary>
        protected List<MainLogicUnit> _targetList;
        public Buff(MainLogicUnit source, MainLogicUnit owner,Skill skill,int buffID
            ,object[] args=null) 
            : base(source, skill)
        {
            Owner = owner;
            BuffID=buffID;
            this.args = args;
        }

        public override void LogicInit()
        {
            Cfg=GameServices.Config.GetBuff(BuffID);
            BuffDuration=Cfg.BuffDuration;
            _delayTime = Cfg.BuffDelay;
            
            base.LogicInit();
            
        }

        public override void LogicTick()
        {
            base.LogicTick();

            switch (UnitState)
            {
                case SubUnitState.Start:
                    Start();
                    if (BuffDuration > 0 || BuffDuration == -1)
                    {
                        UnitState = SubUnitState.Tick;
                    }
                    else
                    {
                        UnitState = SubUnitState.End;
                    }
                    break;
                case SubUnitState.Tick:
                    if (Cfg.BuffInterval > 0)
                    {
                        _tickCount += ServerConfig.ServerLogicFrameIntervelMs;
                        if (_tickCount >= Cfg.BuffInterval)
                        {
                            _tickCount -= Cfg.BuffInterval;
                            Tick();
                        }
                    }

                    _durationCount += ServerConfig.ServerLogicFrameIntervelMs;
                    if (_durationCount >= BuffDuration && BuffDuration != -1)
                    {
                        UnitState = SubUnitState.End;
                    }
                    break;
            }
        }

        protected override void Start()
        {
            
        }

        protected override void Tick()
        {
           
        }

        protected override void End()
        {
            
        }
    }
}