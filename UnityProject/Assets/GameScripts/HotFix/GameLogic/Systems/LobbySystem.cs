using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// L3 Lobby 域业务系统。
    /// <para>上行命令（匹配/确认/选英雄/加载进度/清流程）为 public 方法，UI/Procedure 直接 <c>LobbySystem.Instance.Method()</c> 调用。</para>
    /// <para>原始包下行（RspMatch/NtfXxx）由 <see cref="NetSvc.HandoutMsg"/> 直调本系统 public 方法（不再经 GameEvent 路由）；
    /// 处理后写 <see cref="LobbyState"/> + 发 <see cref="ILobbyEvent"/>（Changed/ShowMatchInfo 下行）。
    /// 多消费者包（NtfConfirm/NtfSelect/NtfLoadRes）由本系统重广播 <c>ILobbyEvent.OnNtfXxx</c> 供 Procedure(FSM)/BattleSystem(跨域)响应。</para>
    /// <para>倒计时（MatchConfirm/Select）由内部 <c>GameModule.Timer</c> 驱动递减 + 每秒推送 Changed，取代 UI 自持 Timer。</para>
    /// <para>对 <see cref="NetSvc"/> 走直接调用。<see cref="LobbyState"/> 为本域唯一状态源。</para>
    /// </summary>
    public sealed class LobbySystem : Singleton<LobbySystem>
    {
        private readonly LobbyState _state = new LobbyState();
        private int _matchConfirmTimerId = -1;
        private int _selectTimerId = -1;

        public override void Release()
        {
            StopMatchConfirmTimer();
            StopSelectTimer();
            base.Release();
        }

        // === 上行命令（UI/Procedure 直接调）===

        /// <summary>请求匹配（1V1/2V2）。</summary>
        public void ReqMatch(PVPEnum pvpMode)
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.ReqMatch, reqMatch = new ReqMatch { pvpEnum = pvpMode } });
        }

        /// <summary>发送确认（匹配确认）。</summary>
        public void SndConfirm()
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.SndConfirm, sndConfirm = new SndConfirm { roomID = _state.MatchRoomID } });
        }

        /// <summary>发送选英雄。</summary>
        public void SndSelect(int heroID)
        {
            _state.SetSelectedHero(heroID);
            StopSelectTimer();
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.SndSelect, sndSelect = new SndSelect { roomID = _state.MatchRoomID, heroID = heroID } });
        }

        /// <summary>UI 打开时拉当前大厅快照（补推 ILobbyEvent.Changed）。</summary>
        public void RequestSnapshot()
        {
            GameEvent.Get<ILobbyEvent>().Changed(_state);
        }

        /// <summary>上报本地加载进度（0-100，ProcedureLoad 场景加载进度回调触发）。</summary>
        public void ReportLoadProgress(int percent)
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.SndLoadPrg, sndLoadPrg = new SndLoadPrg { roomID = _state.MatchRoomID, percent = percent } });
        }

        /// <summary>本地加载完成（ProcedureLoad 场景加载完成触发，发 ReqBattleStart 通知服务器开战）。</summary>
        public void LoadComplete()
        {
            NetSvc.Instance.Send(new HOKMsg { cmd = CMD.ReqBattleStart, reqBattleStart = new ReqBattleStart { roomID = _state.MatchRoomID } });
        }

        /// <summary>清空大厅流程数据（匹配预测/确认/选英雄/加载进度；ProcedureLobby 进大厅新一轮匹配前调）。</summary>
        public void ClearFlow()
        {
            StopMatchConfirmTimer();
            StopSelectTimer();
            _state.SetMatching(false);
            _state.SetMatchPredictTime(0);
            _state.ClearMatchConfirm();
            _state.ClearSelect();
            _state.ClearLoading();
        }

        // === 原始包下行（NetSvc.HandoutMsg 直调，public 入口）===

        /// <summary>匹配响应（无下游消费者，仅写状态 + UI 下行）。</summary>
        public void RspMatch(RspMatch data)
        {
            _state.SetMatchPredictTime(data.predictTime);
            _state.SetMatching(true, data.predictTime);
            GameEvent.Get<ILobbyEvent>().ShowMatchInfo(true, data.predictTime);
            GameEvent.Get<ILobbyEvent>().Changed(_state);
        }

        /// <summary>确认通知（多消费者：重广播 OnNtfConfirm 供 ProcedureLobby/ProcedureMatch 驱动 FSM + BattleSystem 捕获 roomID）。</summary>
        public void NtfConfirm(NtfConfirm confirm)
        {
            if (confirm.dissmiss)
            {
                _state.ClearMatchConfirm();
                _state.SetMatching(false);
                StopMatchConfirmTimer();
                GameEvent.Get<ILobbyEvent>().ShowMatchInfo(false, 0);
                GameEvent.Get<ILobbyEvent>().Changed(_state);
                GameEvent.Get<ILobbyEvent>().OnNtfConfirm(confirm);
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
            GameEvent.Get<ILobbyEvent>().Changed(_state);
            GameEvent.Get<ILobbyEvent>().OnNtfConfirm(confirm);
        }

        /// <summary>选英雄通知（多消费者：重广播 OnNtfSelect 供 ProcedureMatch→ProcedureSelect 流转）。</summary>
        public void NtfSelect()
        {
            StopMatchConfirmTimer();
            _state.StartSelect(ServerConfig.SelectCountDown);
            StartSelectTimer();
            GameEvent.Get<ILobbyEvent>().Changed(_state);
            GameEvent.Get<ILobbyEvent>().OnNtfSelect();
        }

        /// <summary>加载资源通知（多消费者：重广播 OnNtfLoadRes 供 ProcedureSelect→ProcedureLoad 流转 + BattleSystem 流入启动数据）。</summary>
        public void NtfLoadRes(NtfLoadRes data)
        {
            StopSelectTimer();
            _state.StartLoading(data);
            GameEvent.Get<ILobbyEvent>().Changed(_state);
            GameEvent.Get<ILobbyEvent>().OnNtfLoadRes(data);
        }

        /// <summary>加载进度通知（仅写状态 + UI 下行，无下游消费者）。</summary>
        public void NtfLoadPrg(NtfLoadPrg data)
        {
            _state.UpdateLoadingProgress(data);
            GameEvent.Get<ILobbyEvent>().Changed(_state);
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
            GameEvent.Get<ILobbyEvent>().Changed(_state);
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
            GameEvent.Get<ILobbyEvent>().Changed(_state);
        }
    }
}
