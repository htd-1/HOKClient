using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// ③数据订阅（下行·状态变更·L5 SessionState → L1）。UI 订阅刷新玩家信息。
    /// <para>UI 在 <c>RegisterEvent()</c> 订阅 <c>Changed</c>，数据被"喂"进来，不再主动拉取 RuntimeDataService。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IPlayerData
    {
        /// <summary>UserData 变更推送（含 UI 打开时 RequestSnapshot 触发的首次快照）。</summary>
        void Changed(UserData data);
    }
}
