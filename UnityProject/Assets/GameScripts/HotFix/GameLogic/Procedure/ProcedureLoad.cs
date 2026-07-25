 using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 加载状态（纯编排）：显示 LoadUI + 加载战斗场景；听 <see cref="IBattleEvent"/> 驱动流转到战斗。
    /// <para>加载进度刷新业务已迁 <see cref="LobbySystem"/>（LoadUI 订阅 ILobbyEvent.Changed 自动刷新）。</para>
    /// <para>BattleMapID 读 <see cref="BattleSystem"/>（BattleState，OnNtfLoadRes 流入，OnEnter 时已就绪）。</para>
    /// </summary>
    public class ProcedureLoad : ProcedureBase
    {
        private IFsm<IProcedureModule> _procedureOwner;
        private readonly GameEventMgr _mgr = new();
        private int _lastLoadPercent = -1;

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
            // 战斗开始仍走旧 IBattleEvent 链（阶段 3 改为 ILobbyEvent/BattleSystem 原始包事件）
            _mgr.AddEvent<RspBatlleStart>(IBattleEvent_Event.OnRspBattleStart, OnRspBattleStart);
        }

        private void ClearEvents()
        {
            _mgr.Clear();
        }

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameModule.UI.ShowUI<LoadUI>();
            _lastLoadPercent = -1;
            // 场景加载进度 → 上报 SndLoadPrg；加载完成 → ReqBattleStart 通知服务器开战。
            // （修复卡 Load：原版 BattleSys.SceneLoadProgress/SceneLoadDone 的两个发包在重构中丢失，服务器收不到 ReqBattleStart 故不发 RspBattleStart）
            GameModule.Scene.LoadScene(
                "map_" + BattleSystem.Instance.BattleMapID,
                callBack: _ => LobbySystem.Instance.LoadComplete(),
                progressCallBack: OnSceneProgress);
        }

        private void OnSceneProgress(float p)
        {
            int percent = (int)(p * 100);
            if (percent == _lastLoadPercent) return;
            _lastLoadPercent = percent;
            LobbySystem.Instance.ReportLoadProgress(percent);
        }

        protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameModule.UI.CloseUI<LoadUI>();
        }

        private void OnRspBattleStart(RspBatlleStart data)
        {
            ChangeState<ProcedureBattle>(_procedureOwner);
        }
    }
}
