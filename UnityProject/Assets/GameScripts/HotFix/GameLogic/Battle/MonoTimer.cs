using System;
using PETimer;

namespace GameLogic
{
    /// <summary>
    /// 仅做示范用 通过Update每帧Tick的一个简单计时器实现 实际使用仅仅只使用Tengine框架的TimerModule模块即可
    /// </summary>
    public class MonoTimer
    {
        private bool _isActive;
        public bool IsActive
        {
            private set=>_isActive = value;
            get=> _isActive;
        }
    
        private Action<int> _cbAction;
    
        private float _intervalTime;
    
        private float _loopCount;
    
        private Action<bool, float, float> _prgAction;
    
        private Action _endAction;
        
        private float _delayTime;
        private float _prgAllTime;
    
        private float _delayCounter;
        private float _cbCounter;
        private int _loopCounter;
        private float _prgCounter;
        
        private float _prgLoopRate=0;
        private float _prgAllRate = 0;
    
        public MonoTimer(
            Action<int> cbAction,
            float intervalTime,
            int loopCount=1,
            Action<bool, float, float> prgAction = null,
            Action endAction = null,
            float delayTime = 0
        )
        {
            _cbAction = cbAction;
            _intervalTime = intervalTime;
            _loopCount = loopCount;
            _prgAction = prgAction;
            _endAction = endAction;
            _delayTime = delayTime;
            
            IsActive = true;
            _prgAllTime =delayTime+intervalTime*loopCount;
        }
    
        /// <summary>
        /// 驱动计时器运行
        /// </summary>
        /// <param name="delta">间隔时间 ms</param>
        public void TickTimer(float delta)
        {
            if (IsActive)
            {
                if (_delayTime > 0 && _delayCounter < _delayTime)
                {
                    _delayCounter += delta;
                    if (_delayCounter >= _delayTime)
                    {
                        Tick(_delayCounter-_delayTime);
                    }
                    else
                    {
                        //delkay循环进度
                        _prgLoopRate = _delayCounter / _delayTime;
                        if (_prgAllTime > 0)
                        {
                            _prgCounter += delta;
                            _prgAllRate = _prgCounter / _prgAllTime;
                        }
                        _prgAction?.Invoke(true,_prgLoopRate,_prgAllRate);
                    }
                }
                else
                {
                    Tick(delta);
                }
            }
            
        }
    
        private void Tick(float delta)
        {
            _cbCounter += delta;
            //当前循环进度
            _prgLoopRate = _cbCounter / _intervalTime;
            //所有计时进度
            if (_prgAllTime > 0)
            {
                _prgCounter+= delta;
                _prgAllRate = _prgCounter / _prgAllTime;
            }
            _prgAction?.Invoke(false,_prgLoopRate,_prgAllRate);
    
            if (_cbCounter >= _intervalTime)
            {
                ++_loopCounter;
                _cbAction(_loopCounter);
                if (_loopCounter != 0 && _loopCounter >= _loopCount)
                {
                    //达到最大循环次数
                    IsActive = false;
                    _endAction?.Invoke();
    
                    _cbAction = null;
                    _prgAction = null;
                    _endAction=null;
                    
                }
                else
                {
                    //没达到最大循环次数
                    _cbCounter -= _intervalTime;
                    
                }
            }
        }
    
        public void DisableTimer()
        {
            IsActive = false;
            _endAction?.Invoke();
            
            _cbAction = null;
            _prgAction = null;
            _endAction=null;
        }
    }
}