using System.Collections.Generic;
using GameConfig.hok;
using TEngine;
using UnityEngine.PlayerLoop;

namespace GameLogic
{
    public static class BulletRegistry
    {
        private delegate Bullet BulletCtor(MainLogicUnit source, MainLogicUnit owner,
            Skill skill);
        
        private static readonly Dictionary<BulletType, BulletCtor> _ctors = new();


        public static void Init()
        {
            Reg(BulletType.SkillTarget,(source,target,skill)
                =>new TargetBullet(source,target,skill));
            
        }
        
        
        private static void Reg(BulletType type, BulletCtor ctor)
        {
            if (!_ctors.TryAdd(type, ctor))
            {
                Log.Error($"[BulletRegistry] 重复注册 BulletType:{type}");
            }
        }

        public static Bullet Create(BulletCfg cfg,
            MainLogicUnit source, MainLogicUnit owner,Skill skill)
        {
            if (cfg == null)
            {
                Log.Error("[BulletRegistry] cfg 为空 ");
                return null;
            }

            if (!_ctors.TryGetValue(cfg.BulletType, out var ctor))
            {
                Log.Error($"[BulletRegistry] {cfg.BulletType} 为空 ");
                return null;
            }
            return ctor(source, owner, skill);
        }
    }
}