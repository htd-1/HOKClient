using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 选英雄状态（纯编排）：显示 SelectUI；听 <see cref="ILobbyEvent"/> 驱动流转。
    /// <para>选英雄发包/倒计时业务已迁 <see cref="LobbySystem"/>（SelectUI 订阅 ILobbyData.Changed 刷新 + 超时自动选）。</para>
    /// </summary>
    public class ProcedureSelect : ProcedureBase
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
            _mgr.AddEvent<NtfLoadRes>(ILobbyEvent_Event.OnNtfLoadRes, OnNtfLoadRes);
        }

        private void ClearEvents()
        {
            _mgr.Clear();
        }

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameModule.UI.ShowUI<SelectUI>();
        }

        protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameModule.UI.CloseUI<SelectUI>();
        }

        private void OnNtfLoadRes(NtfLoadRes data)
        {
            // Loading 数据写入由 LobbySystem.OnNtfLoadRes 处理，此处仅切到加载流程
            ChangeState<ProcedureLoad>(_procedureOwner);
        }
    }
}
