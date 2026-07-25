using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>
    /// 群体击飞buff(buffID=10132),逻辑待迁移。
    /// </summary>
    public class KnockUpBuff_Group : Buff
    {
        public KnockUpBuff_Group(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        protected override void Start()
        {
            base.Start();

            _targetList = new List<MainLogicUnit>();
            _targetList = CalcRule.FindMultipleTargetByRUle(Owner, Cfg.Impacter, Skill.SkillArgs);

            for (int i = 0; i < _targetList.Count; i++)
            {
                _targetList[i].KnockupCount += 1;
            }
        }

        protected override void End()
        {
            base.End();
            for (int i = 0; i < _targetList.Count; i++)
            {
                _targetList[i].KnockupCount -= 1;
            }
        }
    }
}
