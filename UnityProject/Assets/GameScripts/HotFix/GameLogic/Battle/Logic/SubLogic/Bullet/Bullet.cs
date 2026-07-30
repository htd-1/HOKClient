using dnlib.DotNet.Writer;
using GameConfig.hok;
using PEMath;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 子弹逻辑类
    /// </summary>
    public abstract class Bullet:SubLogicUnit
    {
        /// <summary>
        /// 上一个逻辑帧位置
        /// </summary>
        protected PEVector3 LastPos;
        /// <summary>
        /// 子弹半径
        /// </summary>
        protected PEInt BulletSize;
        /// <summary>
        /// 子弹配置
        /// </summary>
        protected BulletCfg Cfg;
        
        private BulletView _bulletView;
        
        
        protected Bullet(MainLogicUnit source, Skill skill) 
            : base(source, skill)
        {
        }

        public override void LogicInit()
        {

            Cfg = Skill.Cfg.BulletCfg;
            BulletSize =(PEInt) Cfg.BulletSize;

            LogicMoveSpeed = (PEInt)Cfg.BulletSpeed;
            //子弹逻辑位置初始化
            LogicPos = Source.LogicPos + new PEVector3(0, (PEInt)Cfg.BulletHeight, 0);
            LastPos=LogicPos;
            _delayTime = Cfg.BulletDelay;
            
            base.LogicInit();
        }

        public override void LogicTick()
        {
            base.LogicTick();

            switch (UnitState)
            {
                case SubUnitState.Start:
                    Start();
                    UnitState = SubUnitState.Tick;
                    break;
                case SubUnitState.Tick:
                    Tick();
                    break;
                default:
                    break;
            }
        }

        protected override void Start()
        {
            GameObject go = GameModule.Resource.LoadGameObject(Cfg.ResPath);

            go.name = Source.UnitName + "_" + Cfg.BulletName;
            
            _bulletView =go.GetComponent<BulletView>();

            if (_bulletView == null)
            {
                Log.Error("Get bulletview error"+UnitName);
            }
            else
            {
                _bulletView.Init(this);
            }
        }
        
        protected override void Tick()
        {
            
        }
        
        protected override void End()
        {
            _bulletView.DestroyBullet();
        }
    }
}