using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// ①命令（上行·意图·L1 UI → L3 LobbySystem）。大厅流程操作意图。
    /// <para>UI 经 <c>GameEvent.Get&lt;ILobbyCmd&gt;().Method()</c> 触发，LobbySystem 单 handler 监听执行（发包/写状态）。</para>
    /// <para>取代旧 <see cref="ILobbyEvent"/> 的上行方法（OnReqMatch/OnSndConfirm/OnSndSelect），后者过渡期保留废弃。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface ILobbyCmd
    {
        /// <summary>请求匹配（1V1/2V2）。</summary>
        void ReqMatch(PVPEnum pvpMode);

        /// <summary>发送确认（匹配确认）。</summary>
        void SndConfirm();

        /// <summary>发送选英雄。</summary>
        void SndSelect(int heroID);

        /// <summary>UI 打开时拉当前大厅快照（LobbySystem 补推 ILobbyData.Changed）。</summary>
        void RequestSnapshot();

        /// <summary>上报本地加载进度（0-100，ProcedureLoad 场景加载进度回调触发，LobbySystem 发 SndLoadPrg）。</summary>
        void ReportLoadProgress(int percent);

        /// <summary>本地加载完成（ProcedureLoad 场景加载完成触发，LobbySystem 发 ReqBattleStart 通知服务器开战）。</summary>
        void LoadComplete();

        /// <summary>清空大厅流程数据（匹配预测/确认/选英雄/加载进度；进大厅新一轮匹配前由 Procedure 调，重置 LobbyState）。</summary>
        void ClearFlow();
    }
}
