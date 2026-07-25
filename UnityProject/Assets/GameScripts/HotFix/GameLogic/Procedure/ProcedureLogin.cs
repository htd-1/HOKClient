using GameLogic;
using TEngine;

public class ProcedureLogin : TEngine.ProcedureBase
{
    private IFsm<IProcedureModule> _procedureOwner;
    private GameEventMgr _mgr;

    protected override void OnInit(IFsm<IProcedureModule> procedureOwner)
    {
        base.OnInit(procedureOwner);
        _procedureOwner=procedureOwner;
        RegisterEvents();
    }

    protected override void OnDestroy(IFsm<IProcedureModule> procedureOwner)
    {
        base.OnDestroy(procedureOwner);
        ClearEvents();
    }

    protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        NetSvc.Instance.Connect();
        AudioSvc.Instance.PlayBGM(ConfigService.Instance.GetAudio(AudioKey.MainBgm));
        GameModule.UI.ShowUIAsync<LoginUI>();
    }

    protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);
        GameModule.UI.CloseUI<LoginUI>();
    }

    private void RegisterEvents()
    {
        _mgr ??= new GameEventMgr();
        // 纯编排：仅听登录结果驱动状态流转。登录/选服/GM 意图已迁 LoginSystem（经 IPlayerCmd）。
        _mgr.AddEvent<bool, string>(IPlayerEvent_Event.LoginResult, OnLoginResult);
    }

    private void ClearEvents()
    {
        _mgr?.Clear();
        _mgr = null;
    }

    private void OnLoginResult(bool success, string message)
    {
        GameEvent.Get<ITipsUI>().AddTips(message);
        if (success)
        {
            ChangeState<ProcedureStart>(_procedureOwner);
        }
    }


}
