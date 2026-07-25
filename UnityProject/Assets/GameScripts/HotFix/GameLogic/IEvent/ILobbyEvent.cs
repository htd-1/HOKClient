using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 大厅域下行事件（L3 LobbySystem → L1 UI / Procedure / 同域 Sys）。
    /// <para>合并自旧 <c>ILobbyData</c>/<c>ILobbyUI</c>：状态推送 + UI 指令 + 原始包下行合一。</para>
    /// <para><b>上行命令</b>（ReqMatch/SndConfirm/SndSelect 等）不经本接口，直接 <c>LobbySystem.Instance.Method()</c>。</para>
    /// <para><b>原始包下行</b>（OnRspMatch/OnNtfXxx）：NetSvc.HandoutMsg 直调 LobbySystem 公开方法处理后，由 LobbySystem 重广播本接口，
    /// 供 Procedure(FSM)/BattleSystem(跨域)响应；RspMatch/NtfLoadPrg 无下游消费者不重广播。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface ILobbyEvent
    {
        // === 原始包下行（NetSvc.HandoutMsg 直调 LobbySystem 处理后，由 LobbySystem 重广播供 Procedure/BattleSystem 响应）===
        void OnRspMatch(RspMatch data);
        void OnNtfConfirm(NtfConfirm data);
        void OnNtfSelect();
        void OnNtfLoadRes(NtfLoadRes data);
        void OnNtfLoadPrg(NtfLoadPrg data);

        // === Sys→UI 下行通知（LobbySystem 发，UI 订阅刷新）===

        /// <summary>LobbyState 变更推送（含 RequestSnapshot 触发的首次快照）。合并自旧 ILobbyData。</summary>
        void Changed(LobbyState state);

        /// <summary>匹配浮层显隐（predictTime 为预计匹配时间）。合并自旧 ILobbyUI。</summary>
        void ShowMatchInfo(bool isActive, int predictTime = 0);
    }
}
