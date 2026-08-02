using System.Collections.Generic;
using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// 后羿混合多重散射buff(buffID=10260),逻辑待迁移。
    /// </summary>
    public class HouyiMixedMultiScatterBuff : Buff
    {

        private int _scatterCount;

        private TargetCfg _targetCfg;

        private PEInt _damagePct;

        private int _arrowCount;

        private int _arrowDelay;

        private PEInt _posOffset;
        
        private MainLogicUnit _lockTarget;
        public HouyiMixedMultiScatterBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            HouyiMixedMultiScatterBuffCfg hmsb=Cfg as HouyiMixedMultiScatterBuffCfg;
            _scatterCount = hmsb.ScatterCount;
            _targetCfg = hmsb.TargetCfg;
            _damagePct = hmsb.DamagePct;
            _arrowCount = hmsb.ArrowCount;
            _arrowDelay = hmsb.ArrowDelay;

            _posOffset =(PEInt) hmsb.PosOffset;
            
            _targetList = new List<MainLogicUnit>();
            _lockTarget = Skill.LockTarget;
        }

        protected override void Start()
        {
            base.Start();

            MultiBullet(Skill.LockTarget,Skill.Cfg.Damage*_damagePct/100);

            var findList = CalcRule.FindMultipleTargetByRUle(Owner, _targetCfg, PEVector3.zero);

            int count = 0;
            for (int i = 0; i < findList.Count; i++)
            {
                if (count < _scatterCount)
                {
                    if(findList[i].Equals(_lockTarget))continue;
                    _targetList.Add(findList[i]);
                    count++;
                }
            }

            for (int i = 0; i < _targetList.Count; i++)
            {
                TargetBullet bullet = Source.CreateSkillBullet(Source, _targetList[i], Skill)as TargetBullet;

                bullet.HitTargetCB += (unit, objects) =>
                {
                    unit.GetDamageByBuff(Skill.Cfg.Damage,this);
                };
                MultiBullet(_targetList[i],Skill.Cfg.Damage*_damagePct/100,true);
            }

        }

        private void MultiBullet(MainLogicUnit targetHero,PEInt damage,bool isCurve=false)
        {
            for (int i = 0; i < _arrowCount; i++)
            {
                TargetBullet bullet=Source.CreateSkillBullet(Source,targetHero,Skill) as TargetBullet;
                if(isCurve)bullet.SetCurveDir();
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
                    unit.GetDamageByBuff(damage,this);
                    
                };
            }
        }
    }
}
