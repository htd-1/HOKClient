using GameConfig.hok;
using PEMath;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 移动攻击buff(buffID=90000),逻辑待迁移。
    /// </summary>
    public class MoveAttackBuff : Buff
    {
        private MainLogicUnit _moveTarget;
        
        private SkillCfg _atkSkillCfg;

        private PEInt _selectRange;

        private PEInt _searchDis;

        private bool _activeSkill;
        
        private readonly GameEventMgr _eventMgr=new ();
        
        private bool _isUIInput=false;
        public MoveAttackBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            _atkSkillCfg = ConfigService.Instance.GetSkill(Skill.SkillId);
            _selectRange = (PEInt)_atkSkillCfg.TargetCfg.SelectRange;
            _searchDis = (PEInt)_atkSkillCfg.TargetCfg.SearchDis;
            _activeSkill = false;
            _eventMgr.AddEvent<bool>(IBuffEvent_Event.CheckUIInput,CheckUIInput);
        }
     
        protected override void Start()
        {
            base.Start();
            MoveToTarget();
        }

        protected override void Tick()
        {
            base.Tick();
            MoveToTarget();
        }

        private void MoveToTarget()
        {
            _moveTarget = CalcRule.FindMinDisEnemyTarget(Owner, Skill.Cfg.TargetCfg);
            if (_moveTarget == null) return;

            PEVector3 offsetDir = _moveTarget.LogicPos - Owner.LogicPos;
            PEInt sqrDis = offsetDir.sqrMagnitude;
            PEInt sumRaduis = Owner.UnitData.UnitCfg.ColliderCfg.mRadius
                              + _moveTarget.UnitData.UnitCfg.ColliderCfg.mRadius;
            if (sqrDis < (_selectRange + sumRaduis) * (_selectRange + sumRaduis))
            {
                _activeSkill = true;
                BattleSystem.Instance.SendMoveKey(PEVector3.zero);
                UnitState = SubUnitState.End;
            }
            else
            {
                if (sqrDis < (_searchDis + sumRaduis) * (_searchDis + sumRaduis))
                {
                    if (_isUIInput)
                    {
                        //有UI输入中断移动
                        UnitState = SubUnitState.End;
                    }
                    else BattleSystem.Instance.SendMoveKey(offsetDir.normalized);
                }
                else
                {
                    Log.Info("超出搜索距离");
                    BattleSystem.Instance.SendMoveKey(PEVector3.zero);
                    UnitState=SubUnitState.End;
                }
            }
        }

        protected override void End()
        {
            base.End();
            if (_activeSkill)
            {
                _activeSkill = false;
                BattleSystem.Instance.SendSkillKey(Skill.SkillId);
            }
            
            _eventMgr.Clear();
        }

        private void CheckUIInput(bool isUIInput)
        {
            _isUIInput = isUIInput;
        }
    }
}
