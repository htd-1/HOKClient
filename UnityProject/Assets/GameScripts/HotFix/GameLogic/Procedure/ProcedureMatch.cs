using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 匹配确认状态（纯编排）：显示 MatchUI；听 <see cref="ILobbyEvent"/> 驱动流转。
    /// <para>确认发包/刷新业务已迁 <see cref="LobbySystem"/>（MatchUI 订阅 ILobbyData.Changed 自动刷新）。</para>
    /// </summary>
    public class ProcedureMatch : TEngine.ProcedureBase
    {
        private IFsm<IProcedureModule> _procedureOwner;
        private GameEventMgr _mgr = new();

        protected override void OnInit(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnInit(procedureOwner);
            _procedureOwner = procedureOwner;
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _mgr.AddEvent<NtfConfirm>(ILobbyEvent_Event.OnNtfConfirm, OnNtfConfirm);
            _mgr.AddEvent(ILobbyEvent_Event.OnNtfSelect, OnNtfSelect);
        }

        private void ClearEvents()
        {
            _mgr.Clear();
        }

        protected override void OnDestroy(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnDestroy(procedureOwner);
            ClearEvents();
        }

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameModule.UI.ShowUI<MatchUI>();
        }

        protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameModule.UI.CloseUI<MatchUI>();
        }

        private void OnNtfConfirm(NtfConfirm confirm)
        {
            // dismiss → 回 Lobby；非 dismiss 由 MatchUI 订阅 Changed 自动刷新，Procedure 不做业务
            if (confirm.dissmiss)
            {
                ChangeState<ProcedureLobby>(_procedureOwner);
            }
        }

        private void OnNtfSelect()
        {
            ChangeState<ProcedureSelect>(_procedureOwner);
        }
    }
}
