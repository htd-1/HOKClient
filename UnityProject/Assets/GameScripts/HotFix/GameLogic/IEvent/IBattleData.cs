using TEngine;

namespace GameLogic
{
    /// <summary>
    /// ③数据订阅（下行·状态推送·L3 BattleSystem → L1 UI）。战斗状态变更通知。
    /// <para>UI 订阅 <c>IBattleData.Changed</c> 接收 <see cref="BattleState"/>；首次打开发 <c>IBattleCmd.RequestSnapshot</c> 补推。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IBattleData
    {
        void Changed(BattleState state);
    }
}
