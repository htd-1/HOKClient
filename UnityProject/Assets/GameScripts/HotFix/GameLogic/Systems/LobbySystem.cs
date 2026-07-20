using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// L3 Lobby 域业务系统。
    /// <para>监听 <see cref="ILobbyCmd"/>（①命令）处理匹配/确认/选英雄/加载进度发包；监听 <see cref="ILobbyEvent"/>（②原始包下行，
    /// 由 <see cref="NetMessageBindings"/> 发）写 <see cref="LobbyState"/> 发 <see cref="ILobbyData"/>（③推送）/ <see cref="ILobbyUI"/>。</para>
    /// <para>倒计时（MatchConfirm/Select）由内部 <c>GameModule.Timer</c> 驱动递减 + 每秒推送 Changed，取代 UI 自持 Timer。</para>
    /// <para>对 <see cref="NetSvc"/> 走 ④直接调用。<see cref="LobbyState"/> 为本域唯一状态源（旧 RuntimeData 双写已删）。</para>
    /// <para><b>纪律</b>：业务代码不接触 <c>LobbySystem.Instance</c>，只经 <c>GameEvent.Get&lt;ILobbyCmd&gt;()</c> 触发。</para>
    /// </summary>
    public sealed class LobbySystem : GameSystem<LobbySystem>
    {
        private readonly LobbyState _state = new LobbyState();
        private int _matchConfirmTimerId = -1;
        private int _selectTimerId = -1;

        protected override void RegisterCommands(GameEventMgr events)
        {
            // ①命令（UI/Procedure 发）
            events.AddEvent<PVPEnum>(ILobbyCmd_Event.ReqMatch, OnReqMatch);
            events.AddEvent(ILobbyCmd_Event.SndConfirm, OnSndConfirm);
            events.AddEvent<int>(ILobbyCmd_Event.SndSelect, OnSndSelect);
            events.AddEvent(ILobbyCmd_Event.RequestSnapshot, OnRequestSnapshot);
            events.AddEvent<int>(ILobbyCmd_Event.ReportLoadProgress, OnReportLoadProgress);
            events.AddEvent(ILobbyCmd_Event.LoadComplete, OnLoadComplete);
            events.AddEvent(ILobbyCmd_Event.ClearFlow, OnClearFlow);
            // ②原始包事件（NetMessageBindings 发，下行；Procedure 亦听以驱动 FSM 流转）
            events.AddEvent<RspMatch>(ILobbyEvent_Event.OnRspMatch, OnRspMatch);
            events.AddEvent<NtfConfirm>(ILobbyEvent_Event.OnNtfConfirm, OnNtfConfirm);
            events.AddEvent(ILobbyEvent_Event.OnNtfSelect, OnNtfSelect);
            events.AddEvent<NtfLoadRes>(ILobbyEvent_Event.OnNtfLoadRes, OnNtfLoadRes);
            events.AddEvent<NtfLoadPrg>(ILobbyEvent_Event.OnNtfLoadPrg, OnNtfLoadPrg);
        }

        public override void Release()
        {
            StopMatchConfirmTimer();
            StopSelectTimer();
            base.Release();
        }

        // === ①命令处理 ===
        private void OnReqMatch(PVPEnum pvpMode)
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.ReqMatch, reqMatch = new ReqMatch { pvpEnum = pvpMode } });
        }

        private void OnSndConfirm()
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.SndConfirm, sndConfirm = new SndConfirm { roomID = _state.MatchRoomID } });
        }

        private void OnSndSelect(int heroID)
        {
            _state.SetSelectedHero(heroID);
            StopSelectTimer();
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.SndSelect, sndSelect = new SndSelect { roomID = _state.MatchRoomID, heroID = heroID } });
        }

        private void OnRequestSnapshot()
        {
            GameEvent.Get<ILobbyData>().Changed(_state);
        }

        // === 加载进度上报 / 加载完成（ProcedureLoad 场景加载驱动；原 BattleSys.SceneLoadProgress/SceneLoadDone 逻辑迁此）===
        private void OnReportLoadProgress(int percent)
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.SndLoadPrg, sndLoadPrg = new SndLoadPrg { roomID = _state.MatchRoomID, percent = percent } });
        }

        private void OnLoadComplete()
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.ReqBattleStart, reqBattleStart = new ReqBattleStart { roomID = _state.MatchRoomID } });
        }

        /// <summary>清空大厅流程数据（匹配预测/确认/选英雄/加载进度；ProcedureLobby 进大厅新一轮匹配前调，重置 LobbyState）。</summary>
        private void OnClearFlow()
        {
            StopMatchConfirmTimer();
            StopSelectTimer();
            _state.SetMatching(false);
            _state.SetMatchPredictTime(0);
            _state.ClearMatchConfirm();
            _state.ClearSelect();
            _state.ClearLoading();
        }

        // === ②原始包下行处理 ===
        private void OnRspMatch(RspMatch data)
        {
            _state.SetMatchPredictTime(data.predictTime);
            _state.SetMatching(true, data.predictTime);
            GameEvent.Get<ILobbyUI>().ShowMatchInfo(true, data.predictTime);
            GameEvent.Get<ILobbyData>().Changed(_state);
        }

        private void OnNtfConfirm(NtfConfirm confirm)
        {
            if (confirm.dissmiss)
            {
                _state.ClearMatchConfirm();
                _state.SetMatching(false);
                StopMatchConfirmTimer();
                GameEvent.Get<ILobbyUI>().ShowMatchInfo(false, 0);
                GameEvent.Get<ILobbyData>().Changed(_state);
                return;
            }

            if (_state.MatchConfirmData == null)
            {
                _state.StartMatchConfirm(confirm, ServerConfig.SelectCountDown);
                StartMatchConfirmTimer();
            }
            else
            {
                _state.UpdateMatchConfirm(confirm);
            }
            GameEvent.Get<ILobbyData>().Changed(_state);
        }

        private void OnNtfSelect()
        {
            StopMatchConfirmTimer();
            _state.StartSelect(ServerConfig.SelectCountDown);
            StartSelectTimer();
            GameEvent.Get<ILobbyData>().Changed(_state);
        }

        private void OnNtfLoadRes(NtfLoadRes data)
        {
            StopSelectTimer();
            _state.StartLoading(data);
            GameEvent.Get<ILobbyData>().Changed(_state);
        }

        private void OnNtfLoadPrg(NtfLoadPrg data)
        {
            _state.UpdateLoadingProgress(data);
            GameEvent.Get<ILobbyData>().Changed(_state);
        }

        // === 倒计时 Timer（内部驱动，取代 UI 自持 Timer 读 RuntimeData）===
        private void StartMatchConfirmTimer()
        {
            StopMatchConfirmTimer();
            _matchConfirmTimerId = GameModule.Timer.AddTimer(OnMatchConfirmTick, time: 1f, isLoop: true);
        }

        private void StopMatchConfirmTimer()
        {
            if (_matchConfirmTimerId != -1)
            {
                GameModule.Timer.RemoveTimer(_matchConfirmTimerId);
                _matchConfirmTimerId = -1;
            }
        }

        private void OnMatchConfirmTick(object[] args)
        {
            _state.TickMatchConfirm();
            GameEvent.Get<ILobbyData>().Changed(_state);
        }

        private void StartSelectTimer()
        {
            StopSelectTimer();
            _selectTimerId = GameModule.Timer.AddTimer(OnSelectTick, time: 1f, isLoop: true);
        }

        private void StopSelectTimer()
        {
            if (_selectTimerId != -1)
            {
                GameModule.Timer.RemoveTimer(_selectTimerId);
                _selectTimerId = -1;
            }
        }

        private void OnSelectTick(object[] args)
        {
            _state.TickSelect();
            GameEvent.Get<ILobbyData>().Changed(_state);
        }
    }
}
