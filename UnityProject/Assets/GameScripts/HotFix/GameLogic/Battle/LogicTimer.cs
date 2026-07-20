using System;
using PEMath;
using HOKProtocol;
using PETimer;

namespace GameLogic
{
    /// <summary>
    /// 逻辑计时器
    /// </summary>
    public class LogicTimer
    {
        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        private PEInt _delayTime;
        
        private PEInt _loopTime;

        private PEInt _delta;
        private PEInt _callbackCount;
        private Action _cb;

        public LogicTimer(Action cb,PEInt delayTIme,int loopTime=0)
        {
            _cb=cb;
            _delayTime=delayTIme;
            _loopTime=loopTime;
            _delta = ServerConfig.ServerLogicFrameIntervelMs;
            IsActive = true;
        }

        public void TickTimer()
        {
            _callbackCount += _delta;
            if (_callbackCount >= _delayTime && _cb != null)
            {
                _cb();
                if (_loopTime == 0)
                {
                    IsActive = false;
                    _cb = null;
                }
                else
                {
                    _callbackCount -= _delayTime;
                    _delayTime = _loopTime;
                }
            }
        }
        
    }
}