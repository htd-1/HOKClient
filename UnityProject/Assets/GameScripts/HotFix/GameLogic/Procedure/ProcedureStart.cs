using TEngine;

namespace GameLogic
{
    public class ProcedureStart:ProcedureBase
    {
        private IFsm<IProcedureModule> _procedureOwner;
        private GameEventMgr _mgr;

        protected override void OnInit(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnInit(procedureOwner);
            _procedureOwner = procedureOwner;
            RegisterEvents();
        }
        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameModule.UI.ShowUI<StartUI>();
        }

        private void RegisterEvents()
        {
            _mgr??=new GameEventMgr();
            _mgr.AddEvent(IStartEvent_Event.OnEnterLobby,OnEnterLobby);
        }

        private void OnEnterLobby()
        {
            ChangeState<ProcedureLobby>(_procedureOwner);
        }
        protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameModule.UI.CloseUI<StartUI>();
            
        }

        protected override void OnDestroy(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnDestroy(procedureOwner);
            ClearEvents();
        }

        private void ClearEvents()
        {
            _mgr.Clear();
            _mgr=null;
        }
    }
}