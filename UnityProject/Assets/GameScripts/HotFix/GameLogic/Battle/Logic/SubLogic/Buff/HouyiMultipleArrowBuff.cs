using GameConfig.hok;
using PEMath;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 后羿被动多重射击buff(buffID=10250),逻辑待迁移。
    /// </summary>
    public class HouyiMultipleArrowBuff : Buff
    {
        private int _arrowCount;
        private int _arrowDelay;
        private PEInt _posOffset;
        
        public HouyiMultipleArrowBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        private MainLogicUnit _targetHero;
        public override void LogicInit()
        {
            base.LogicInit();

            HouyiMultipleArrowBuffCfg hmab = Cfg as HouyiMultipleArrowBuffCfg;
            Log.Info($"[10250] LogicInit CfgType={Cfg?.GetType().Name} arrowCount={hmab?.ArrowCount} targetHero={Skill.LockTarget?.UnitName}");
            _arrowCount=hmab.ArrowCount;
            _arrowDelay = hmab.ArrowDelay;
            _posOffset=(PEInt)hmab.PosOffset;

            _targetHero = Skill.LockTarget;
            
        }

        protected override void Start()
        {
            base.Start();
            
            for (int i = 0; i < _arrowCount; i++)
            {
                TargetBullet bullet=Source.CreateSkillBullet(Source,_targetHero,Skill) as TargetBullet;
                
                bullet.SetDelayData((i+1)*_arrowDelay);

                if (i % 2 == 0)
                {
                    bullet.SetOffsetPos(PEVector3.up*_posOffset);   
                }
                else
                {
                    bullet.SetOffsetPos(PEVector3.up*-_posOffset);
                }
                bullet.HitTargetCB += (MainLogicUnit unit,object [] args) =>
                {  
                    unit.GetDamageByBuff(Skill.Cfg.Damage,this);
                    
                };
            }
        }
    }
}
