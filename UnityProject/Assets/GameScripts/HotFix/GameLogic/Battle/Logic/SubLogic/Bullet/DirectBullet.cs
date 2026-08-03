using System;
using System.Collections.Generic;
using GameConfig.hok;
using HOKProtocol;
using PEMath;
using PEPhysx;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class DirectBullet:Bullet
    {
        private  PEVector3 _hitHeight;
        
        private PEVector3 _targetPos;

        private PECylinderCollider _targetCollider;
        
        private int _bulletTime;
        
        public Action<MainLogicUnit,object[]> HitTargetCB;

        public Action ReachPosCB;
        
        public DirectBullet(MainLogicUnit source, Skill skill) : base(source, skill)
        {
            
        }

        public override void LogicInit()
        {
            base.LogicInit();

            _hitHeight =new PEVector3(0,
                (PEInt)Cfg.BulletHeight,0);
            BulletType bte = Cfg.BulletType;

            if (bte == BulletType.UIDirection)
            {
                if (Skill.SkillArgs == PEVector3.zero)
                {
                    Log.Error("input error");
                    return;
                }
                LogicDir=Skill.SkillArgs;
            }
            else if (bte == BulletType.UIPosition)
            {
                _targetPos=Source.LogicPos
                    +Skill.SkillArgs+_hitHeight;

                var targetColliderCfg = new ColliderConfig
                {
                    mPos = _targetPos,
                    mType = ColliderType.Cylinder,
                    mRadius = BulletSize,
                };
                
                _targetCollider=new PECylinderCollider(targetColliderCfg);
                
                LogicDir=(_targetPos-LogicPos).normalized;
                
            }
            else
            {
                Log.Error($"Unknown BulletType{bte}");
            }

            LogicPos += LogicDir * (PEInt)Cfg.BulletOffset;
        }

        private GameObject _ghostRoot;
        private int _tickCount;
        protected override void Start()
        {
            base.Start();
            // #region DEBUG显示
            //
            // _ghostRoot = new GameObject
            // {
            //     name="弹道GhostRoot"
            // };
            // _ghostRoot.transform.localPosition=Vector3.zero;
            // UnityEngine.Object.Destroy(_ghostRoot,5);
            //
            // #endregion
        }

        protected override void Tick()
        {
            base.Tick();
            LogicPos += LogicDir * LogicMoveSpeed;
            
            PEVector3 pos = (LogicPos + LastPos) / 2;

            PEVector3 offset = LogicPos - LastPos;

            ColliderConfig volumeCfg = new ColliderConfig
            {
                mType = ColliderType.Box,
                mPos = pos,
                mSize = new PEVector3
                {
                    x=offset.magnitude/2,
                    y=0,
                    z=BulletSize,
                },
                mAxis = new PEVector3[3]
            };
            volumeCfg.mAxis[0]=offset.normalized;
            volumeCfg.mAxis[1]=PEVector3.up;
            volumeCfg.mAxis[2]=PEVector3
                .Cross(offset,PEVector3.up)
                .normalized;

            PEBoxCollider volumeCollider=new PEBoxCollider(volumeCfg);
            
            LastPos =LogicPos ;

            // #region 弹道显示
            //
            // GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //
            // go.transform.SetParent(_ghostRoot.transform);
            // _tickCount += 1;
            // go.name = "ghost_"+_tickCount;
            // go.GetComponent<MeshRenderer>().enabled = false;
            // go.transform.position = volumeCfg.mPos.ConvertViewVector3();
            //
            // go.transform.right=volumeCfg.mAxis[0].ConvertViewVector3();
            // go.transform.up=volumeCfg.mAxis[1].ConvertViewVector3();
            // go.transform.forward=volumeCfg.mAxis[2].ConvertViewVector3();
            // go.transform.localScale = volumeCfg.mSize.ConvertViewVector3() * 2;
            //
            // #endregion
            
            List<MainLogicUnit> hitList=new List<MainLogicUnit>();
            List<MainLogicUnit> selectList = CalcRule.FindMultipleTargetByRUle(Source, Cfg.Impacter, PEVector3.zero);
            for (int i = 0; i < selectList.Count; i++)
            {
                PEVector3 normal = PEVector3.zero;
                PEVector3 adj = PEVector3.zero;
                if (selectList[i].Collider.DetectBoxContact(volumeCollider, ref normal, ref adj))
                {
                    hitList.Add(selectList[i]);
                }
            }

            if (Cfg.CanBlock)
            {
                //可被阻挡
                MainLogicUnit hitTarget=CalcRule.FindMinDisTargetInPos(LastPos ,hitList);
                if (hitTarget != null)
                {
                    HitTargetCB(hitTarget, new object[] { _bulletTime, hitTarget.LogicPos });
                    UnitState =  SubUnitState.End;
                }
            }
            else
            {
                //不可阻挡
                for (int i = 0; i < hitList.Count; i++)
                {
                    HitTargetCB(hitList[i], new object[] { _bulletTime, hitList[i].LogicPos });
                    
                }
            }

            if (Cfg.BulletType == BulletType.UIPosition)
            {
                PEVector3 normal = PEVector3.zero;
                PEVector3 adj = PEVector3.zero;
                if (_targetCollider.DetectBoxContact(volumeCollider, ref normal, ref adj))
                {
                    UnitState =  SubUnitState.End;
                }
            }
            else if(Cfg.BulletType==BulletType.UIDirection)
            {
                _bulletTime += ServerConfig.ServerLogicFrameIntervelMs;
                if (_bulletTime >= Cfg.BulletDuration)
                {
                    UnitState = SubUnitState.End;
                }
            }

            // Log.Info($"BulletTime:{_bulletTime}");
            LastPos = LogicPos;
        }

        protected override void End()
        {
            base.End();
            
            ReachPosCB?.Invoke();
        }
    }
}