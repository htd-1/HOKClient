using System;
using PEMath;
using PEPhysx;
using UnityEngine;
using Object = System.Object;

namespace GameLogic
{
    public class TargetBullet:Bullet
    {
        /// <summary>
        /// 目标
        /// </summary>
        protected MainLogicUnit Target;
        
        protected PEVector3 CurveDir=PEVector3.zero;

        public Action<MainLogicUnit, object[]> HitTargetCB;

        private readonly PEVector3 _hitHeight;
        
        public TargetBullet(MainLogicUnit source, MainLogicUnit target,Skill skill)
            : base(source, skill)
        {
            Target = target;
            _hitHeight =new PEVector3(0,
                Target.UnitData.UnitCfg.HitHeight,0);
        }

        public override void LogicInit()
        {
            base.LogicInit();

            PEVector3 targetPos = Target.LogicPos + _hitHeight;
            LogicDir = (targetPos - LogicPos).normalized;

            //子弹基于中心点的偏移
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
            //       name="弹道GhostRoot"
            // };
            // _ghostRoot.transform.localPosition=Vector3.zero;
            // UnityEngine.Object.Destroy(_ghostRoot,5);
            //
            // #endregion
        }

        protected override void Tick()
        {
            base.Tick();
            
            LogicDir=(Target.LogicPos+_hitHeight-LogicPos).normalized;

            if (LogicDir == PEVector3.zero)
            {
                UnitState = SubUnitState.End;
                return;
            }

            if (Target.UnitState == GameLogic.UnitState.Dead)
            {
                UnitState = SubUnitState.End;
                return;
            }
            LogicDir += CurveDir;
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
            //
            // #endregion
            
            PEVector3 normal = PEVector3.zero;
            PEVector3 adj=PEVector3.zero;
            if (Target.Collider.DetectBoxContact(volumeCollider, ref normal, ref adj))
            {
                UnitState = SubUnitState.End;
            }
        }

        protected override void End()
        {
            base.End();
            //命中目标 产生效果
            if (Target.UnitState != GameLogic.UnitState.Dead)
            {
                HitTargetCB?.Invoke(Target,null);
            }
        }

        public void SetDelayData(int delay)
        {
            _delayCounter=delay;
            UnitState=SubUnitState.Delay;
        }

        public void SetOffsetPos(PEVector3 offset)
        {
            LogicPos+=offset;
            PEVector3 targetPos = Target.LogicPos + _hitHeight;
            LogicDir=(targetPos - LogicPos).normalized;
        }

        public void SetCurveDir()
        {
            PEVector3 targetPos =Target.LogicPos+_hitHeight;
            PEVector3 v1 = PEVector3.Cross((targetPos-LogicPos),PEVector3.up).normalized;

            v1 *= RandomUtils.RandomInt(-100, 100);

            PEVector3 v2 = PEVector3.up * RandomUtils.RandomInt(0, 100);
            
            CurveDir=(v1+v2).normalized/2;

            LogicDir += CurveDir;

            // RandomUtils.RandomInt();
        }
    }
}