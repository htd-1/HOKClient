using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 
    /// </summary>
    [EventInterface(EEventGroup.GroupUI)]
    public interface IBattlePlayUI
    {
        void OnSkillCancel(bool state);

        /// <summary>
        /// 技能进入 CD：逻辑层施法成功后发送，PlayUI 接收并驱动匹配的 SkillItem.EnterCDState。
        /// </summary>
        /// <param name="skillID">技能配置 ID（SkillCfg.SkillID）。</param>
        /// <param name="cdTime">CD 时长，毫秒。</param>
        void OnSkillEnterCD(int skillID, int cdTime);
    }
}
