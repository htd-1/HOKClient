using PEMath;

namespace GameLogic
{
    public interface ILogic
    {
        void LogicInit();
        void LogicTick();
        void LogicUnInit();
    }
    public class LogicUnit:ILogic
    {
        /// <summary>
        /// 逻辑单位名字
        /// </summary>
        public string UnitName;

        public bool IsPosChanged = false;
        public bool IsDirChanged = false;
        /// <summary>是否本机友方（与本机英雄同队）。FightMgr 创建后固化，表现层(View/HPUI)读此字段，无需反向访问 FightMgr。</summary>
        public bool IsFriend = false;
        #region Key Properties
        private PEVector3 _logicPos;
        public PEVector3 LogicPos
        {
            set
            {
                _logicPos = value;
                IsPosChanged = true;
            }
            get=>_logicPos;
        }

        private PEVector3 _logicDir;

        public PEVector3 LogicDir
        {
            set
            {
                 _logicDir = value;
                 IsDirChanged = true;
            }
            get => _logicDir;
        }

        private PEInt _logicMoveSpeed;

        public PEInt LogicMoveSpeed
        {
            set => _logicMoveSpeed = value;
            get => _logicMoveSpeed;
        }
        
        private PEInt _moveSpeedBase;

        public PEInt MoveSpeedBase
        {
            set => _moveSpeedBase = value;
            get => _moveSpeedBase;
        }
        
        #endregion
        public virtual void LogicInit()
        {
            
        }

        public virtual void LogicTick()
        {
            
        }

        public virtual void LogicUnInit()
        {
        }
    }
}