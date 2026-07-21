using System;
using System.Collections.Generic;
using GameConfig.hok;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// Buff 注册工厂,替代原版 ResSvc.CreateBuff 的 switch。
    /// 所有 Buff 子类构造签名统一,用委托字典免去 Creator 样板类。
    /// </summary>
    public static class BuffRegistry
    {
        private delegate Buff BuffCtor(MainLogicUnit source, MainLogicUnit owner,
                                       Skill skill, int buffID, object[] args);

        private static readonly Dictionary<BuffType, BuffCtor> _ctors = new();

        /// <summary>
        /// 战斗启动时调一次(由 FightMgr 初始化阶段调用)。
        /// </summary>
        public static void Init()
        {
            _ctors.Clear();

            // ===== 单体/通用 buff =====
            Reg(BuffType.HPCure,                 (src, owner, skill, id, args) => new HPCureBuff_Single(src, owner, skill, id, args));
            Reg(BuffType.ModifySkill,            (src, owner, skill, id, args) => new CommonModifySkillBuff(src, owner, skill, id, args));
            Reg(BuffType.MoveSpeed_Single,       (src, owner, skill, id, args) => new MoveSpeedBuff_Single(src, owner, skill, id, args));
            Reg(BuffType.ArthurMark,             (src, owner, skill, id, args) => new ArthurMarkBuff(src, owner, skill, id, args));
            Reg(BuffType.Silense,                (src, owner, skill, id, args) => new SilenceBuff_Single(src, owner, skill, id, args));
            Reg(BuffType.TargetFlashMove,        (src, owner, skill, id, args) => new TargetFlashMoveBuff(src, owner, skill, id, args));
            Reg(BuffType.ExecuteDamage,          (src, owner, skill, id, args) => new ExecuteDamageBuff(src, owner, skill, id, args));
            Reg(BuffType.Stun_Single_DynamicTime,(src, owner, skill, id, args) => new StunBuff_DynamicTime(src, owner, skill, id, args));
            Reg(BuffType.HouyiActiveSkillModify, (src, owner, skill, id, args) => new HouyiScatterSkillModifyBuff(src, owner, skill, id, args));
            Reg(BuffType.Scatter,                (src, owner, skill, id, args) => new HouyiScatterArrowBuff(src, owner, skill, id, args));
            Reg(BuffType.HouyiPasvAttackSpeed,   (src, owner, skill, id, args) => new HouyiPasvAttackSpeedBuff(src, owner, skill, id, args));
            Reg(BuffType.HouyiPasvSkillModify,   (src, owner, skill, id, args) => new HouyiMultipleSkillModifyBuff(src, owner, skill, id, args));
            Reg(BuffType.HouyiPasvMultiArrow,    (src, owner, skill, id, args) => new HouyiMultipleArrowBuff(src, owner, skill, id, args));
            Reg(BuffType.HouyiMixedMultiScatter, (src, owner, skill, id, args) => new HouyiMixedMultiScatterBuff(src, owner, skill, id, args));
            Reg(BuffType.MoveAttack,             (src, owner, skill, id, args) => new MoveAttackBuff(src, owner, skill, id, args));

            // ===== 群体 buff(对应 GroupBuff\ 子目录) =====
            Reg(BuffType.MoveSpeed_DynamicGroup, (src, owner, skill, id, args) => new MoveSpeedBuff_DynamicGroup(src, owner, skill, id, args));
            Reg(BuffType.MoveSpeed_StaticGroup,  (src, owner, skill, id, args) => new MoveSpeedBuff_StaticGroup(src, owner, skill, id, args));
            Reg(BuffType.Knockup_Group,          (src, owner, skill, id, args) => new KnockUpBuff_Group(src, owner, skill, id, args));
            Reg(BuffType.Damage_DynamicGroup,    (src, owner, skill, id, args) => new DamageBuff_DynamicGroup(src, owner, skill, id, args));
            Reg(BuffType.Damage_StaticGroup,     (src, owner, skill, id, args) => new DamageBuff_StaticGroup(src, owner, skill, id, args));

            // DirectionFlashMove:枚举有定义但原版未实现,暂不注册。
        }

        private static void Reg(BuffType type, BuffCtor ctor)
        {
            if (!_ctors.TryAdd(type, ctor))
            {
                Log.Error($"[BuffRegistry] 重复注册 BuffType:{type}");
            }
        }

        /// <summary>
        /// 按 cfg.BuffType 创建对应 Buff 实例。cfg 由调用方(MainLogicUnit.CreateSkillBuff)查好传入。
        /// </summary>
        public static Buff Create(BuffCfg cfg, MainLogicUnit source, MainLogicUnit owner,
                                  Skill skill, int buffID, object[] args)
        {
            if (cfg == null)
            {
                Log.Error($"[BuffRegistry] cfg 为空 buffID={buffID}");
                return null;
            }
            // Log.Info($"{cfg}");
            if (!_ctors.TryGetValue(cfg.BuffType, out var ctor))
            {
                Log.Error($"[BuffRegistry] 未注册的 BuffType:{cfg.BuffType}(buffID={buffID})");
                return null;
            }
            return ctor(source, owner, skill, buffID, args);
        }
    }
}
