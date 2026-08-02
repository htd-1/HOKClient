using System;
using PEMath;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 属性相关
    /// </summary>
    public partial class MainLogicUnit
    {
        #region 属性状态数据
        private PEInt _hp;
        public PEInt Hp
        {
            get => _hp;
            private set=>_hp = value;
        }

        private PEInt _def;

        public PEInt Def
        {
            get => _def;
            private set => _def = value;
        }

        private PEInt _attackSpeedRateBase;

        public PEInt AttackSpeedRateBase
        {
            get=>_attackSpeedRateBase;
            set=>_attackSpeedRateBase=value;
        }

        private PEInt _attackSpeedRate;

        public PEInt AttackSpeedRate
        {
            get => _attackSpeedRate;
            set
            {
                _attackSpeedRate = value;
                Skill skill = GetNormalSkill();
                if (skill != null)
                {
                    skill.SkillTime = skill.Cfg.SkillTime *
                        AttackSpeedRateBase / AttackSpeedRate;
                    skill.SpellTime = skill.Cfg.SpellTime *
                        AttackSpeedRateBase / AttackSpeedRate;
                }
            }
        }

        public int SilenceCount
        {
            get => _silenceCount;
            set
            {
                _silenceCount = value;
                if (IsSilenced())
                {
                    OnStateChange?.Invoke(StateEnum.Silenced,true);
                }
                else
                {
                    OnStateChange?.Invoke(StateEnum.Silenced,false);
                }
            }
        }
        private int _silenceCount;
        private bool IsSilenced() => _silenceCount != 0;
        
        private int _stunnedCount;

        public int StunnedCount
        {
            get => _stunnedCount;
            set
            {
                _stunnedCount = value;
                if (IsStunned())
                {
                    InputFakeMoveKey(PEVector3.zero);
                    OnStateChange?.Invoke(StateEnum.Stunned,true);
                }
                else
                {
                    OnStateChange?.Invoke(StateEnum.Stunned,false);
                }
            }
        }
        private bool IsStunned() => _stunnedCount != 0;
        
        private int _knockupCount;

        public int KnockupCount
        {
            get => _knockupCount;
            set
            {
                _knockupCount = value;
                if (IsKnocked())
                {
                    InputFakeMoveKey(PEVector3.zero);
                    OnStateChange?.Invoke(StateEnum.Knockup,true);

                    LogicPos += new PEVector3(0, (PEInt)(0.5), 0);
                }
                else
                {
                    OnStateChange?.Invoke(StateEnum.Knockup,false);
                    LogicPos += new PEVector3(0, (PEInt)(-0.5), 0);
                }
            }
        }
        private bool IsKnocked() =>_knockupCount != 0;
        #endregion
        
        private void InitProperties()
        {
            Hp = UnitData.UnitCfg.Hp;
            Def = UnitData.UnitCfg.Def;
            
        }

        public void InitAttackSpeedRate(PEInt rate)
        {
            AttackSpeedRateBase = rate;
            AttackSpeedRate = rate;//每秒钟进行rate次攻击
        }

        #region API Functions

        public void GetDamageBySKill(PEInt damage, Skill skill)
        {
            OnHurt?.Invoke(); //受伤状态标记
            PEInt hurt = damage - Def;
            if (hurt > 0)
            {
                Hp-=hurt;
                if (Hp <= 0)
                {
                    Hp = 0;
                    UnitState = UnitState.Dead;
                    InputFakeMoveKey(PEVector3.zero);
                    OnDeath?.Invoke(skill.Owner);
                    PlayAni("death");
                    Log.Info($"{UnitName} hp=0,Died");
                }
                // Log.Info($"{UnitName} hp={Hp.RawInt}");

                JumpUpdateInfo jui=null;
                if (IsPlayerSelf()||skill.Owner.IsPlayerSelf())
                {
                    jui = new JumpUpdateInfo
                    {
                        JumpVal = damage.RawInt,
                        JumpType = JumpType.SkillDamage,
                        JumpAni = JumpAni.LeftCurve
                    };
                }
                OnHpChange?.Invoke(Hp.RawInt,jui);
            }
        }

        public void GetDamageByBuff(PEInt damage, Buff buff,bool calcCB=true)
        {
            // Log.Info("1ci");
            if(calcCB)OnHurt?.Invoke();

            if (!string.IsNullOrEmpty(buff.Cfg.HitTickAudio))
            {
                PlayAudio(buff.Cfg.HitTickAudio);
            }
            PEInt hurt = damage - Def;
            if (hurt > 0)
            {
                Hp-=hurt;
                if (Hp <= 0)
                {
                    Hp = 0;
                    UnitState = UnitState.Dead;
                    InputFakeMoveKey(PEVector3.zero);
                    OnDeath?.Invoke(buff.Source);
                    PlayAni("death");
                    
                }

                JumpUpdateInfo jui=null;
                if (IsPlayerSelf()||buff.Source.IsPlayerSelf()||buff.Owner.IsPlayerSelf())
                {
                    jui = new JumpUpdateInfo
                    {
                        JumpVal = damage.RawInt,
                        JumpType = JumpType.BuffDamage,
                        JumpAni = JumpAni.RightCurve
                    };
                }
                OnHpChange?.Invoke(Hp.RawInt,jui);
            }
        }
        public void GetCureByBuff(PEInt cure,Buff buff)
        {
            if (Hp >= UnitData.UnitCfg.Hp)
            {
                // Log.Info("血量已经回复");
            }

            Hp += cure;
            PEInt trueCure = cure;
            if (Hp > UnitData.UnitCfg.Hp)
            {
                trueCure -= (Hp - UnitData.UnitCfg.Hp);
                Hp = UnitData.UnitCfg.Hp;
            }
            
            JumpUpdateInfo jui=null;
            //作用目标

            if (IsPlayerSelf() || buff.Source.IsPlayerSelf())
            {
                jui = new JumpUpdateInfo
                {
                    JumpVal = trueCure.RawInt,
                    JumpType = JumpType.Cure,
                    JumpAni = JumpAni.CenterUp,
                };
            }
            
            OnHpChange?.Invoke(Hp.RawInt,jui);
                
        }
        public void ModifyMoveSpeed(PEInt value, Buff buff, bool jumpInfo)
        {
            Log.Info($"移动offset{value.ScaledValue}");
            LogicMoveSpeed += value;
            Log.Info($"当前速度{LogicMoveSpeed}");
            if (value < 0 && jumpInfo)
            {
                //减速
                JumpUpdateInfo jui=null;
                if (IsPlayerSelf())
                {
                    jui = new JumpUpdateInfo
                    {
                        JumpType = JumpType.SlowSpeed,
                        JumpAni = JumpAni.CenterUp,
                    };
                }
                OnSlowDown?.Invoke(jui);
            }
        }

        public void ModifyAttackSpeed(PEInt value)
        {
            AttackSpeedRate+=value;            
        }
        
        #endregion
        public bool IsTeam(Team team)
        {
            return UnitData.Team==team;
        }

        #region 事件回调函数

        public Action OnHurt;
        public Action<MainLogicUnit> OnDeath;

        public Action<int,JumpUpdateInfo> OnHpChange;
       
        public Action<JumpUpdateInfo> OnSlowDown;

        
        public Action<StateEnum,bool>OnStateChange;
        
        #endregion
    }
    public enum StateEnum
    {
        None,
        Silenced,
        Knockup,
        Stunned,
        Invincible,
        Restricted,
    }
}