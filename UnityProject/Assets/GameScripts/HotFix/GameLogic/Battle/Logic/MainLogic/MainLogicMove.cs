using System.Collections.Generic;
using HOKProtocol;
using PEMath;
using PEPhysx;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 移动相关
    /// </summary>
    public partial class MainLogicUnit
    {
        private PEVector3 _inputDir;
        public PEVector3 InputDir
        {
            get => _inputDir;
            set=>_inputDir=value;
        }
        /// <summary>
        /// 战斗单位碰撞
        /// </summary>
        public PECylinderCollider Collider;
        
        private List<PEColliderBase> _envColliderList;
        private void InitMove()
        {
            LogicPos = UnitData.BornPos;
            MoveSpeedBase = UnitData.UnitCfg.MoveSpeed;
            LogicMoveSpeed = UnitData.UnitCfg.MoveSpeed;
            Collider = new PECylinderCollider(UnitData.UnitCfg.ColliderCfg)
            {
                mPos=LogicPos,
                
                
            };
            
        }

        private void TickMove()
        {
            PEVector3 moveDir =InputDir;
            Collider.mPos += moveDir * LogicMoveSpeed * 
                             ConfigService.Instance.GetClientFrameTime();
            PEVector3 adj = PEVector3.zero;
            Collider.CalcCollidersInteraction(_envColliderList,ref moveDir, ref adj);
            if (LogicDir != moveDir)
            {
                LogicDir=moveDir;
            }

            if (LogicDir != PEVector3.zero)
            {
                LogicPos = Collider.mPos + adj;
            }

            Collider.mPos = LogicPos;
        }

        private void UnInitMove()
        {
            
        }
        
        private PEVector3 _uiInputDir;
        public void InputMoveKey(PEVector3 dir)
        {
            _uiInputDir = dir;
           if(!IsSkillSpelling()&&
              !IsStunned()&&
              !IsKnocked()) _inputDir = dir;
            
        }
        /// <summary>
        /// 模拟输入方向
        /// </summary>
        /// <param name="dir"></param>
        public void InputFakeMoveKey(PEVector3 dir)
        {
            _inputDir = dir;
        }

        public void RecoverUIInput()
        {
            if (InputDir != _uiInputDir)
            {
                InputDir = _uiInputDir;
            }
        }

        /// <summary>由 FightMgr.InitHero 在创建英雄后注入环境碰撞体列表，供 TickMove 碰撞计算使用。</summary>
        public void SetEnvColliders(List<PEColliderBase> list)
        {
            _envColliderList = list;
        }

        

        public bool CanMove()
        {
            return !IsStunned()&&
                   !IsKnocked()&&
                   !IsSkillSpelling();
        }
    }
}