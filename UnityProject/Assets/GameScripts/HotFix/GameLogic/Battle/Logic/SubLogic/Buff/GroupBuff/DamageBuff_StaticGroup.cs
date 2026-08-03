using System.Collections.Generic;
using GameConfig.hok;
using HOKProtocol;
using PEMath;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 静态群体伤害buff(buffID=10133/10220/10221/10231),逻辑待迁移。
    /// </summary>
    public class DamageBuff_StaticGroup : Buff
    {
        private PEInt _damage;
        public DamageBuff_StaticGroup(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            _targetList = new List<MainLogicUnit>();
            DamageStaticGroupBuffCfg gdbc = Cfg as DamageStaticGroupBuffCfg;
            _damage = gdbc.Damage;
            
            switch (gdbc.StaticPosType)
            {
                case StaticPosType.SkillCasterPos:
                    LogicPos = Source.LogicPos;
                    break;
                case StaticPosType.SkillLockTargetPos:
                    LogicPos = Skill.LockTarget.LogicPos;
                    break;
                case StaticPosType.BulletHitTargetPos:
                    LogicPos = (PEVector3)args[1];
                    break;
                case StaticPosType.UIInputPos:
                    LogicPos = Source.LogicPos + Skill.SkillArgs;
                    
                    break;
                case StaticPosType.None:
                default:
                    Log.Error("static buff pos error");
                    break;
            }
            
            // #region DEBUG测试
            //
            // #if UNITY_EDITOR
            // GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            //
            // go.transform.position = LogicPos.ConvertViewVector3();
            // go.transform.localScale = new Vector3(Cfg.Impacter.SelectRange*2
            //     ,Cfg.Impacter.SelectRange*2
            //     ,Cfg.Impacter.SelectRange*2);
            //         
            // #endif
            // #endregion
        }

        protected override void Start()
        {
            base.Start();
            CalcGroupDamage();
        }

        private void CalcGroupDamage()
        {
            _targetList.Clear();
            _targetList.AddRange(
                CalcRule.
                    FindMultipleTargetByRUle(
                        Owner,Cfg.Impacter,LogicPos));
            // Log.Info(_targetList.Count);
            for (int i = 0; i < _targetList.Count; i++)
            {
                // Log.Info($"第{i}个实体受伤{_damage}");
                _targetList[i].GetDamageByBuff(_damage,this);
            }
        }
    }
}
