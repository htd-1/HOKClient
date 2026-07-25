using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HOKProtocol;
using PENet;
using TEngine;

namespace GameLogic
{
    /// <summary>网络连接状态。</summary>
    public enum NetworkStatus
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectionFailed,
        PingTimeout
    }

    /// <summary>
    /// 网络服务
    /// </summary>
    public class NetSvc : Singleton<NetSvc>, IUpdate
    {
        private readonly object _lock = new object();
        private readonly Queue<HOKMsg> _messageQueue = new Queue<HOKMsg>();

        private KCPNet<ClientSession, HOKMsg> _client;
        private Task<bool> _checkTask;
        private int _connectFailCount;

        private uint _sendPingID;
        private int _pingCounter;
        private readonly Dictionary<uint, DateTime> _pingDic = new Dictionary<uint, DateTime>();
        private int _pingTimerId = -1;

        public bool IsConnected => _client?.clientSession != null && _client.clientSession.IsConnected();
        public int Ping { get; private set; }

        // === L4 自持网络/选服状态 ===
        private bool _isPublicServer;
        public NetworkStatus NetworkStatus { get; private set; }
        public ErrorCode NetworkError { get; private set; }
        public string NetworkMessage { get; private set; }

        
        public void SetServerSelection(bool isPublicServer) => _isPublicServer = isPublicServer;

        private void SetNetworkStatusCore(NetworkStatus status, string message)
        {
            NetworkStatus = status;
            NetworkMessage = message;
        }

        private void SetNetworkErrorCore(ErrorCode error, string message)
        {
            NetworkError = error;
            NetworkMessage = message;
        }

        private void ClearNetworkState()
        {
            NetworkStatus = NetworkStatus.Disconnected;
            NetworkError = ErrorCode.None;
            NetworkMessage = null;
        }

        protected override void OnInit()
        {
            KCPTool.LogFunc = msg => Log.Info(msg);
            KCPTool.WarnFunc = msg => Log.Warning(msg);
            KCPTool.ErrorFunc = msg => Log.Error(msg);
            KCPTool.ColorLogFunc = (color, msg) => Log.Info(msg);

            // Ping 应答由 HandoutMsg 直调 HandleRspPing（与其它包统一分发路径），不再单独 Subscribe。
        }

        protected override void OnRelease()
        {
            StopPing();
            _client = null;
            _checkTask = null;
            _pingDic.Clear();
            lock (_lock)
            {
                _messageQueue.Clear();
            }
            ClearNetworkState();
        }

        #region Transport

        public void Connect()
        {
            _client = new KCPNet<ClientSession, HOKMsg>();
            try
            {
                string srvIP = _isPublicServer
                    ? ServerConfig.RemoteGateIP
                    : ServerConfig.LocalDevInnerIP;
                _client.StartAsClient(srvIP, ServerConfig.UdpPort);
                _checkTask = _client.ConnectServer(100);
                _connectFailCount = 0;
                SetStatus(NetworkStatus.Connecting, "连接服务器");
                Log.Info($"[NetSvc] connecting to {srvIP}:{ServerConfig.UdpPort}");
            }
            catch (Exception e)
            {
                string message = $"[NetSvc] connect failed: {e.Message}";
                SetStatus(NetworkStatus.ConnectionFailed, message);
                Log.Warning(message);
                _client = null;
                _checkTask = null;
            }
        }

        public void Disconnect()
        {
            StopPing();
            SetStatus(NetworkStatus.Disconnected, "断开服务器连接");
            _client = null;
            _checkTask = null;
            lock (_lock)
            {
                _messageQueue.Clear();
            }
        }

        /// <summary>KCP 会话回调：服务器/网络侧主动断开。</summary>
        public void HandleSessionDisconnected()
        {
            StopPing();
            SetStatus(NetworkStatus.Disconnected, "断开服务器连接");
            lock (_lock)
            {
                _messageQueue.Clear();
            }
        }

        public void Send(HOKMsg msg)
        {
#if UNITY_EDITOR
            // GM 离线模拟：临时调试工具，仅编辑器生效，打包运行时不编译。
            if (GMService.Instance.IsActive)
            {
                GMService.Instance.SimulateServerRcvMsg(msg);
                return;
            }
#endif
            if (!IsConnected)
            {
                const string message = "服务器未连接，消息发送失败";
                SetNetworkStatusCore(NetworkStatus.Disconnected, message);
                GameEvent.Get<ITipsUI>().AddTips(message);
                Log.Warning($"[NetSvc] {message}");
                return;
            }
            _client.clientSession.SendMsg(msg);
        }

        public void AddMsgQue(HOKMsg msg)
        {
            lock (_lock)
            {
                _messageQueue.Enqueue(msg);
            }
        }

        #endregion

        #region Update / Pump

        public void OnUpdate()
        {
            CheckConnection();
            Pump();
        }

        private void CheckConnection()
        {
            if (_checkTask == null || !_checkTask.IsCompleted)
            {
                return;
            }

            if (_checkTask.Result)
            {
                Log.Info("ConnectServer Success.");
                _checkTask = null;
                SetStatus(NetworkStatus.Connected, "连接服务器成功");
                StartPing();
            }
            else
            {
                _connectFailCount++;
                if (_connectFailCount > 4)
                {
                    string message = $"Connect Failed {_connectFailCount} Times, Check Network.";
                    SetStatus(NetworkStatus.ConnectionFailed, message);
                    Log.Error(message);
                    _checkTask = null;
                }
                else
                {
                    Log.Warning($"Connect Failed {_connectFailCount} Times, Retry...");
                    _checkTask = _client.ConnectServer(100);
                }
            }
        }

        private void Pump()
        {

            if (_messageQueue.Count == 0)
            {
                return;
            }

            HOKMsg msg;
            lock (_lock)
            {
                if (_messageQueue.Count == 0)
                {
                    return;
                }
                msg = _messageQueue.Dequeue();
            }


            if (msg.error != ErrorCode.None)
            {
                HandleProtocolError(msg.error);
                return;
            }

            try { HandoutMsg(msg); }
            catch (Exception e) { Log.Error($"[NetSvc] handout cmd={msg.cmd} throw: {e}"); }
        }

        /// <summary>
        /// 处理网络包信息
        /// </summary>
        /// <param name="msg"></param>
        private void HandoutMsg(HOKMsg msg)
        {
            switch (msg.cmd)
            {
                case CMD.RspLogin:
                    LoginSystem.Instance.RspLogin(msg.rspLogin);
                    break;
                case CMD.RspMatch:
                    LobbySystem.Instance.RspMatch(msg.rspMatch);
                    break;
                case CMD.NtfConfirm:
                    LobbySystem.Instance.NtfConfirm(msg.ntfConfirm);
                    break;
                case CMD.NtfSelect:
                    LobbySystem.Instance.NtfSelect();
                    break;
                case CMD.NtfLoadRes:
                    LobbySystem.Instance.NtfLoadRes(msg.ntfLoadRes);
                    break;
                case CMD.NtfLoadPrg:
                    LobbySystem.Instance.NtfLoadPrg(msg.ntfLoadPrg);
                    break;
                case CMD.RspBattleStart:
                    BattleSystem.Instance.RspBattleStart(msg.rspBatlleStart);
                    break;
                case CMD.NtfOpKey:
                    BattleSystem.Instance.NtfOpKey(msg.ntfOpKey);
                    break;
                case CMD.NtfChat:
                    BattleSystem.Instance.NtfChat(msg.ntfChat);
                    break;
                case CMD.RspBattleEnd:
                    BattleSystem.Instance.RspBattleEnd(msg.rspBattleEnd);
                    break;
                case CMD.RspPing:
                    HandleRspPing(msg.rspPing);
                    break;
                default:
                    Log.Warning($"[NetSvc] unhandled CMD: {msg.cmd}");
                    break;
            }
        }
        
        private void HandleProtocolError(ErrorCode error)
        {
            switch (error)
            {
                case ErrorCode.AcctIsOnline:
                    const string onlineMsg = "当前账号已经上线";
                    SetNetworkErrorCore(error, onlineMsg);
                    GameEvent.Get<ITipsUI>().AddTips(onlineMsg);
                    break;
                default:
                    Log.Warning($"[NetSvc] unhandled protocol error: {error}");
                    break;
            }
        }

        #endregion

        #region Ping

        public void HandleRspPing(RspPing rsp)
        {
            if (_pingDic.ContainsKey(rsp.pingID))
            {
                TimeSpan ts = DateTime.Now - _pingDic[rsp.pingID];
                Ping = (int)ts.TotalMilliseconds;
                _pingDic.Clear();
                _pingCounter = 0;
            }
        }

        private void StartPing()
        {
            StopPing();
            _pingTimerId = GameModule.Timer.AddTimer(OnPingTick, time: 5f, isLoop: true);
        }

        private void StopPing()
        {
            if (_pingTimerId != -1)
            {
                GameModule.Timer.RemoveTimer(_pingTimerId);
                _pingTimerId = -1;
            }
        }

        private void OnPingTick(object[] args)
        {
            _sendPingID++;
            Send(new HOKMsg
            {
                cmd = CMD.ReqPing,
                reqPing = new ReqPing
                {
                    pingID = _sendPingID,
                    sendTime = KCPTool.GetUTCStartMilliseconds()
                }
            });

            if (_pingDic.Count > 0)
            {
                _pingCounter++;
                if (_pingCounter >= 3)
                {
                    const string message = "网络异常，检测网络环境";
                    SetStatus(NetworkStatus.PingTimeout, message);
                    _pingCounter = 0;
                }
            }

            _pingDic[_sendPingID] = DateTime.Now;
        }

        #endregion

        private void SetStatus(NetworkStatus status, string message)
        {
            SetNetworkStatusCore(status, message);
            GameEvent.Get<INetworkEvent>().OnNetworkStatusChanged(status);
            if (!string.IsNullOrEmpty(message))
            {
                GameEvent.Get<ITipsUI>().AddTips(message);
            }
        }
    }
}
