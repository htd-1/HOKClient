using System.Collections.Generic;
using GameConfig.hok;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// 后羿散射箭buff(buffID=10240),逻辑待迁移。
    /// </summary>
    public class HouyiScatterArrowBuff : Buff
    {
        private int _scatterCount;
        private TargetCfg _targetCfg;
        private PEInt _damagePct;
        private MainLogicUnit _lockTarget;
        public HouyiScatterArrowBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            HouyiScatterArrowBuffCfg hsab=Cfg as HouyiScatterArrowBuffCfg;
            _scatterCount = hsab.ScatterCount;
            _damagePct = hsab.DamagePct;
            _targetCfg=hsab.TargetCfg;

            _targetList = new List<MainLogicUnit>();
            _lockTarget = Skill.LockTarget;

            var findList = CalcRule.FindMultipleTargetByRUle(Owner, _targetCfg, PEVector3.zero);

            int count = 0;
            for (int i = 0; i < findList.Count; i++)
            {
                if (count < _scatterCount)
                {
                    if(findList[i].Equals(_lockTarget))continue;
                    else
                    {
                        _targetList.Add(findList[i]);
                        count++;
                    }
                }
            }

            for (int i = 0; i < _targetList.Count; i++)
            {
                TargetBullet bullet = Source.CreateSkillBullet(Source, _targetList[i], Skill)as TargetBullet;

                bullet.HitTargetCB += (unit, objects) =>
                {
                    unit.GetDamageByBuff(Skill.Cfg.Damage,this);
                };
            }

        }
    }
}
