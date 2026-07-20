using TEngine;

namespace GameLogic
{
    /// <summary>
    /// ①命令（上行·意图·L1/L2 → L3 <see cref="LoginSystem"/>）。约定单 handler。
    /// <para>UI/Procedure 经 <c>GameEvent.Get&lt;IPlayerCmd&gt;().Xxx()</c> 触发，不持有 LoginSystem 实例。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IPlayerCmd
    {
        /// <summary>发起登录请求。</summary>
        void Login(string account, string password);

        /// <summary>进入 GM 离线模拟。</summary>
        void GmIntent();

        /// <summary>选择服务器（公网/本地）。</summary>
        void ServerSelect(bool isPublicServer);

        /// <summary>UI 打开时请求当前数据快照（覆盖数据早于 UI 就绪的情况）。</summary>
        void RequestSnapshot();
    }
}
