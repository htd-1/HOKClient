using GameConfig.hok;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 动态时间眩晕buff(buffID=10230)。
    /// </summary>
    public class StunBuff_DynamicTime : Buff
    {

        public StunBuff_DynamicTime(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            StunDynamicTimeBuffCfg sdtb=Cfg as StunDynamicTimeBuffCfg;
            int argsTime = (int)args[0];
            argsTime=Mathf.Clamp(argsTime,sdtb.MinStunTime,sdtb.MaxStunTime);

            BuffDuration=argsTime;
        }

        protected override void Start()
        {
            base.Start();
            Owner.StunnedCount += 1;
        }

        protected override void End()
        {
            base.End();
            Owner.StunnedCount -= 1;
        }
    }
}
