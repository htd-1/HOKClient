using System;
using System.Collections.Generic;
using HOKProtocol;

using PEMath;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 技能相关
    /// </summary>
    public partial class MainLogicUnit
    {
        protected Skill[] SkillArr;

        private List<Buff> _buffList;
        private List<LogicTimer> _timeList; 
        
        private void InitSkill()
        {
            int len = UnitData.UnitCfg.SkillArr.Length;
            SkillArr=new Skill[len];
            for (int i = 0; i < len; i++)
            {
                SkillArr[i] = new Skill(UnitData.UnitCfg.SkillArr[i], this);
            }
            _timeList=new List<LogicTimer>();
            _buffList=new List<Buff>();

            var pasvBuffArr = UnitData.UnitCfg.PasvBuff;

            if (pasvBuffArr != null)
            {
                for (int i = 0; i < pasvBuffArr.Length; i++)
                {
                    CreateSkillBuff(this, null, pasvBuffArr[i]);
                }
            }

            OnDirChanged += ClearFreeAniCallBack;
        }

        private void TickSkill()
        {
            for (int i = _buffList.Count - 1; i >= 0; i--)
            {
                if (_buffList[i].UnitState == SubUnitState.None)
                {
                    _buffList[i].LogicUnInit();
                    _buffList.RemoveAt(i);
                }
                else
                {
                    _buffList[i].LogicTick();
                }
            }
            
            
            for (int i = _timeList.Count - 1; i >= 0; i--)
            {
                var timer=_timeList[i];
                if (timer.IsActive)
                {
                    timer.TickTimer();
                }
                else
                {
                    _timeList.RemoveAt(i);
                }
            }
        }

        private void InputSkillKey(SkillKey key)
        {
            for (int i = 0; i < SkillArr.Length; i++)
            {
                if (SkillArr[i].SkillId == key.skillID)
                {
                    PEInt x = PEInt.zero;
                    PEInt z = PEInt.zero;
                    x.ScaledValue = key.x_value;
                    z.ScaledValue = key.z_value;
                    PEVector3 skillArgs=new PEVector3(x,0,z);
                    SkillArr[i].ReleaseSkill(skillArgs);
                    return;
                }
            } 
            Log.Error($"skillID{key.skillID} not found");
        }


        public void CreateLogicTimer(Action cb, PEInt waitTime)
        {
            LogicTimer timer=new LogicTimer(cb, waitTime);
            _timeList.Add(timer);
        }

        public Buff CreateSkillBuff(MainLogicUnit source, Skill skill, int buffID, object[] args = null)
        {
            Buff buff=BuffRegistry.Create(GameServices.Config.GetBuff(buffID),source,this,skill,buffID, args);
            buff.LogicInit();
            _buffList.Add(buff);
            
            return buff;
        }
        public Skill GetNormalSkill()
        {
            if (SkillArr != null && SkillArr[0] != null)
            {
                return SkillArr[0];
            }
            return null;
        }

        public Skill GetSkillByID(int skillID)
        {
            for (int i = 0; i < SkillArr.Length; i++)
            {
                if (SkillArr[i].SkillId == skillID)
                {
                    return SkillArr[i];
                }
            }
            Log.Error($"skillID{skillID} not found");
            return null;
        }

        public Buff GetBuffByID(int buffID)
        {
            for (int i = 0; i < _buffList.Count; i++)
            {
                if (_buffList[i].Cfg.BuffID == buffID)
                {
                    return _buffList[i];
                }
            }
            return null;
        }
        private void UnInitSkill()
        {
            OnDirChanged -= ClearFreeAniCallBack;
        }

        public void ClearFreeAniCallBack()
        {
            for (int i = 0; i < SkillArr.Length; i++)
            {
                SkillArr[i].FreeAniCallback = null;
            }
        }
        public bool IsSkillSpelling()
        {
            for (int i = 0; i < SkillArr.Length; i++)
            {
                if (SkillArr[i].SkillState == SkillState.SpellStart)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsSkillReady(int skillID)
        {
            for (int i = 0; i < SkillArr.Length; i++)
            {
                if (SkillArr[i].SkillId == skillID)
                {
                    return SkillArr[i].SkillState == SkillState.None;
                }
            }
            return false;
        }

        public bool CanReleaseSkill(int skillID)
        {
            return !IsSilenced()&&
                   !IsStunned()&&
                   !IsKnocked()&&
                   !IsSkillSpelling()&&
                   IsSkillReady(skillID);
        }
        public bool IsForbidReleaseSkill()
        {
            return IsSilenced()||IsStunned()||IsKnocked();
        }
    }
}