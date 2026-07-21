using System;
using GameConfig.hok;
using PEMath;
using TEngine;

namespace GameLogic
{
    public enum SkillState
    {
        None,
        SpellStart,
        SpellAfter,
    }
    public class Skill
    {
        public int SkillId;
        public SkillCfg Cfg;
        public PEVector3 SkillArgs;
        public MainLogicUnit LockTarget;
        public SkillState SkillState=SkillState.None;

        public PEInt SpellTime;
        public PEInt SkillTime;
        
        public MainLogicUnit Owner;

        public Action FreeAniCallback;

        public Action<Skill> SpellSuccCallback;
        
        
        public Skill(int skillID,MainLogicUnit owner)
        {
            SkillId = skillID;
            Cfg=GameServices.Config.GetSkill(skillID);
            SpellTime = Cfg.SpellTime;
            SkillTime = Cfg.SkillTime;
            Owner = owner;

            if (Cfg.IsNormalAttack)
            {
                Owner.InitAttackSpeedRate(1000/SkillTime);
            }
        }


        private void HitTarget(MainLogicUnit target, object[] args = null)
        {
            //音效 todo 
            if (Cfg.AudioHit != null)
            {
                target.PlayAudio(Cfg.AudioHit);
            }
            
            if (Cfg.Damage != 0)
            {
                PEInt damage = Cfg.Damage;
                target.GetDamageBySKill(damage,this);
            }
            //
            if (Cfg.BuffIDArr == null) return;

            for (int i = 0; i < Cfg.BuffIDArr.Count; i++)
            {
                int buffID=Cfg.BuffIDArr[i];
                if (buffID == 0)
                {
                    Log.Warning($"skillID{Cfg.SkillID}exist buffID==0");
                    continue;
                }
                BuffCfg buffCfg =GameServices.Config.GetBuff(buffID);

                if (buffCfg.Attacher == AttachType.Target
                    ||buffCfg.Attacher==AttachType.Bullet)
                {
                    target.CreateSkillBuff(Owner, this, buffID, args);
                }
            }
        }
         /// <summary>
         /// 技能生效
         /// </summary>
         /// <param name="lockTarget"></param>
        private void CalcSkillAttack(MainLogicUnit lockTarget)
        {
            if (Cfg.BulletCfg != null)
            {
                //todo
            }
            else
            {
                HitTarget(lockTarget);
            }
        }
        
        /// <summary>
        /// 施法前摇
        /// </summary>
        /// <param name="spellDir"></param>
        private void SkillSpellStart(PEVector3 spellDir)
        {
            SkillState=SkillState.SpellStart;
            if (Cfg.AudioStart != null)
            {
                Owner.PlayAudio(Cfg.AudioStart);
            }

            if (spellDir != PEVector3.zero)
            {
                Owner.MainViewUnit.UpdateSkillRotation(spellDir);
            }

            // aniName 可能为 null(原版 buff 触发型技能,如 1011/1012/1021)或
            // 空串(Luban 数据 skill.get("aniName","") 把缺省降级为 ""),
            // 两者都表示"无施法动画",用 IsNullOrEmpty 一并跳过,避免 CrossFade("") 报错。
            if (!string.IsNullOrEmpty(Cfg.AniName))
            {
                Owner.InputFakeMoveKey(PEVector3.zero);
                Owner.PlayAni(Cfg.AniName);
                //技能被中断或后摇移动取消需要调用动画重置
                FreeAniCallback = () =>
                {
                    Owner.PlayAni("free");
                };
            }
        }

        private void SkillSpellAfter()
        {
            SkillState=SkillState.SpellAfter;
            if (Cfg.AudioWork != null)
            {
                Owner.PlayAudio(Cfg.AudioWork);
            }
            //施法成功 消耗对应资源 TODO
            if (Owner.IsPlayerSelf() && !Cfg.IsNormalAttack)
            {
                // 进入技能 CD：通知表现层（PlayUI 监听后驱动 SkillItem.EnterCDState）
                
                GameEvent.Get<IBattlePlayUI>().OnSkillEnterCD(Cfg.SkillID, Cfg.CdTime);
                
            }
            //技能释放成功回调 提供事件buff
            SpellSuccCallback?.Invoke(this);

            // aniName 可能为 null(原版 buff 触发型技能,如 1011/1012/1021)或
            // 空串(Luban 数据 skill.get("aniName","") 把缺省降级为 ""),
            // 两者都表示"无施法动画",用 IsNullOrEmpty 一并跳过,避免 CrossFade("") 报错。
            if (!string.IsNullOrEmpty(Cfg.AniName))
            {
                Owner.RecoverUIInput();
            }
            //定时器后摇完成后技能状态重置为None

            if (SkillTime > SpellTime)
            {
                Owner.CreateLogicTimer(SkillEnd,SkillTime-SpellTime);
            }
            else
            {
                SkillEnd();
            }
        }
        /// <summary>
        /// 技能结束
        /// </summary>
        private void SkillEnd()
        {
            if (FreeAniCallback != null)
            {
                FreeAniCallback();
                FreeAniCallback = null;
            }

            SkillState = SkillState.None;
            LockTarget = null;
        }
        
        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="skillArgs"></param>
        public void ReleaseSkill(PEVector3 skillArgs)
        {
            SkillArgs=skillArgs;

            if (Cfg.TargetCfg != null&&Cfg.TargetCfg.TargetTeam!=TargetTeam.Dynamic)
            {
                LockTarget=CalRule.FindSingleTargetByRule(Owner,Cfg.TargetCfg,skillArgs);
                if (LockTarget != null)
                {
                    PEVector3 spellDir=LockTarget.LogicPos-Owner.LogicPos;
                    SkillSpellStart(spellDir);

                    void SkillWork()
                    {
                        CalcSkillAttack(LockTarget);
                        AttachSkillBuffToCaster();
                        SkillSpellAfter();
                    }
                    
                    if (SpellTime == 0)
                    {
                        // Log.Info("瞬发技能");
                        SkillWork();
                    }
                    else
                    {
                        void DelaySkillWork()
                        {
                            LockTarget=CalRule.FindSingleTargetByRule(Owner,Cfg.TargetCfg,skillArgs);
                            if (LockTarget != null)
                            {
                                SkillWork();
                            }
                            else
                            {
                                SkillEnd();
                            }
                        }
                        //定时处理
                        Owner.CreateLogicTimer(DelaySkillWork,SpellTime);
                    }
                }
                else
                {
                    Log.Warning("没有符合条件的技能目标");
                    SkillEnd();
                }
            }
            //非目标技能
            else
            {
                SkillSpellStart(skillArgs);
                
                void DirectionBullet()
                {
                    //非目标弹道技能
                    //todo
                }
                if (SpellTime == 0)
                {
                    if (Cfg.BulletCfg != null)
                    {
                        DirectionBullet();
                    }
                    AttachSkillBuffToCaster();
                    SkillSpellAfter();
                }
                else
                {
                    Owner.CreateLogicTimer(() =>
                    {
                        if (Cfg.BulletCfg != null)
                        {
                            DirectionBullet();
                        }
                        AttachSkillBuffToCaster();
                        SkillSpellAfter();
                    },SpellTime);
                }
            }
        }

        private void AttachSkillBuffToCaster()
        {
            if (Cfg.BuffIDArr == null) return;
            for (int i = 0; i < Cfg.BuffIDArr.Count; i++)
            {
                var buffID = Cfg.BuffIDArr[i];
                if (buffID == 0)
                {
                    this.Warn($"SkillID:{Cfg.SkillID}exist: buffID==0,please chek your buffID config");
                    continue;
                }

                BuffCfg buffCfg = GameServices.Config.GetBuff(buffID);
                if (buffCfg.Attacher == AttachType.Caster||
                    buffCfg.Attacher==AttachType.Indie)
                {
                    Owner.CreateSkillBuff(Owner, this, buffID);
                }
            }
        }

        private int tempSkillID;

        public int TempSkillID
        {
            get => tempSkillID;
            set 
            {
               tempSkillID = value;
               // Log.Info($"set tempSkillID:{tempSkillID}");
            }
        }

        /// <summary>
        /// 技能替换
        /// </summary>
        /// <param name="replaceID"></param>
        public void ReplaceSkillCfg(int replaceID)
        {
            //查看是否调整成功
            if (SkillId == replaceID)
            {
                TempSkillID = 0;
            }
            else
            {
                TempSkillID= replaceID;
            }
            
            Cfg=GameServices.Config.GetSkill(replaceID);
            SpellTime = Cfg.SpellTime;
            SkillTime = Cfg.SkillTime;
            if (Cfg.IsNormalAttack)
            {
                Owner.InitAttackSpeedRate(1000/SkillTime);
            }
        }
    }
}