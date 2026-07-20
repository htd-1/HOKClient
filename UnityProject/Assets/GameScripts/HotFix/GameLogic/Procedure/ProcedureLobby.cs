using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 大厅流程状态（纯编排）：进大厅加载场景 + 显示 LobbyUI；听 <see cref="ILobbyEvent"/> 驱动流转。
    /// <para>匹配请求/显示业务已迁 <see cref="LobbySystem"/>（经 ILobbyCmd/ILobbyUI）。本类仅留 ChangeState + ShowUI/CloseUI + LoadScene。</para>
    /// </summary>
    public class ProcedureLobby : TEngine.ProcedureBase
    {
        private IFsm<IProcedureModule> _procedureOwner;
        private GameEventMgr _mgr = new();

        protected override void OnInit(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnInit(procedureOwner);
            _procedureOwner = procedureOwner;
            RegisterEvents();
        }

        protected override void OnDestroy(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnDestroy(procedureOwner);
            ClearEvents();
        }

        private void RegisterEvents()
        {
            _mgr.AddEvent<NtfConfirm>(ILobbyEvent_Event.OnNtfConfirm, OnNtfConfirm);
        }

        private void ClearEvents()
        {
            _mgr.Clear();
        }

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            ClearFlowState();
            GameModule.Scene.LoadScene("lobby", callBack: _ =>
            {
                GameModule.UI.CloseUI<StartUI>();
                GameModule.UI.ShowUI<LobbyUI>();
            });
        }

        protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameModule.UI.CloseUI<LobbyUI>();
        }

        private void ClearFlowState()
        {
            // 进大厅新一轮匹配前重置大厅流程数据（LobbySystem 清 LobbyState）
            GameEvent.Get<ILobbyCmd>().ClearFlow();
        }

        private void OnNtfConfirm(NtfConfirm confirm)
        {
            // 非首次确认 → 进 Match；dismiss 留 Lobby（LobbySystem 已关匹配浮层）
            if (confirm.dissmiss) return;
            ChangeState<ProcedureMatch>(_procedureOwner);
        }
    }
}
