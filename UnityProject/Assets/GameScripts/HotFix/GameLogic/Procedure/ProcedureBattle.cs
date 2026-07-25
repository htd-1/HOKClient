using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 战斗流程状态机节点（纯编排）。
    /// <para>战斗生命周期/帧驱动/输入收发已归 <see cref="BattleSystem"/>（EnterBattle/Tick/ExitBattle）。</para>
    /// <para>本类仅：OnEnter 开 UI + 场景 BGM + 调 EnterBattle；OnUpdate 调 Tick；OnLeave 关 UI + 调 ExitBattle；
    /// 以及听 <see cref="IBattleEvent"/>.OnRspBattleEnd 流转到结算 UI。</para>
    /// </summary>
    public class ProcedureBattle : ProcedureBase
    {
        private IFsm<IProcedureModule> _procedureOwner;
        private GameEventMgr _mgr = new();
        private bool _ended;

        protected override void OnInit(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnInit(procedureOwner);
            _procedureOwner = procedureOwner;
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _mgr.AddEvent<RspBattleEnd>(IBattleEvent_Event.OnRspBattleEnd, OnRspBattleEnd);
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
            GameModule.UI.CloseUI<LoadUI>();
            GameModule.UI.ShowUI<PlayUI>();
            GameModule.UI.ShowUI<HPUI>();
            AudioSvc.Instance.PlayBGM(ConfigService.Instance.GetAudio(AudioKey.BattleBgm));
            AudioSvc.Instance.PlaySound(ConfigService.Instance.GetAudio(AudioKey.Welcombattle));

            _ended = false;
            BattleSystem.Instance.EnterBattle();
        }

        protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameModule.UI.CloseUI<PlayUI>();
            GameModule.UI.CloseUI<HPUI>();
            BattleSystem.Instance.ExitBattle();
        }

        protected override void OnUpdate(IFsm<IProcedureModule> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            BattleSystem.Instance.Tick();
        }

        private void OnRspBattleEnd(RspBattleEnd data)
        {
            // 防重入：结算包可能重复，仅首次显示结算 UI。Tick 已由 BattleSystem.RspBattleEnd 停止。
            if (_ended) return;
            _ended = true;
            GameModule.UI.ShowUI<ResultUI>();
        }
    }
}
