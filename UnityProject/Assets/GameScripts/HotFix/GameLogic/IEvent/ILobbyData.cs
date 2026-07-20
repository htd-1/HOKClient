using TEngine;

namespace GameLogic
{
    /// <summary>
    /// ③数据订阅（下行·状态变更·L3 LobbySystem → L1 UI）。大厅流程状态推送。
    /// <para>UI 订阅 <c>Changed</c> 刷新（匹配信息/确认/倒计时/加载进度），不再主动拉 <c>RuntimeDataService</c>。</para>
    /// <para>倒计时由 LobbySystem 内部 Timer 驱动递减 + 每秒推送；UI 仅据推送刷新显示。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface ILobbyData
    {
        /// <summary>LobbyState 变更推送（含 RequestSnapshot 触发的首次快照）。</summary>
        void Changed(LobbyState state);
    }
}
