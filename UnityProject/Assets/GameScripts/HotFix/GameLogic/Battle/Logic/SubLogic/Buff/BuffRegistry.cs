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

            Reg(BuffType.MoveSpeed_Single,
                (src, owner, skill, id, args)
                    => new MoveSpeedBuff_Single(src, owner, skill, id, args));

            Reg(BuffType.HPCure,
                (src,owner,skill,id,args)
                =>new HPCureBuff_Single(src, owner, skill, id, args));
            // 其余 BuffType(HPCure/ArthurMark/Stun/...)的子类逻辑尚未迁移,
            // 每补一个 XxxBuff.cs 后,在此追加一行 Reg(...)。
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
            Log.Info($"{cfg}");
            if (!_ctors.TryGetValue(cfg.BuffType, out var ctor))
            {
                Log.Error($"[BuffRegistry] 未注册的 BuffType:{cfg.BuffType}(buffID={buffID})");
                return null;
            }
            return ctor(source, owner, skill, buffID, args);
        }
    }
}
