using HOKProtocol;

namespace GameLogic
{
    /// <summary>
    /// L5 域状态：会话级（用户/账号/选服）+ 网络状态。由 <see cref="LoginSystem"/> 私有持有。
    /// <para>纯数据 POCO，不耦合事件总线；变更通知由 LoginSystem 写入后发 <c>IPlayerData.Changed</c>。</para>
    /// <para>本类为会话域唯一状态源（由 LoginSystem 私有持有，经 IPlayerData.Changed 推送）。</para>
    /// </summary>
    public sealed class SessionState
    {
        // === 会话级（用户/账号）===
        public UserData UserData { get; private set; }
        public uint AccountID { get; private set; }
        public string AccountName { get; private set; }
        public bool IsPublicServer { get; set; }
        public bool IsOfflineMode { get; set; }

        // === 网络状态（会话级；由 NetSvc 经 INetworkEvent 产生，LoginSystem 写入；阶段1暂留字段，网络写入在 tasks 2.4 接入）===
        public int NetworkPing { get; private set; }
        public NetworkStatus NetworkStatus { get; private set; }
        public ErrorCode NetworkError { get; private set; }
        public string NetworkMessage { get; private set; }

        public void SetUserData(UserData data)
        {
            UserData = data;
            AccountName = data?.name;
            AccountID = data?.id ?? 0;
        }

        public void SetServerSelection(bool isPublicServer) => IsPublicServer = isPublicServer;

        public void SetNetworkPing(int ping) => NetworkPing = ping;

        public void SetNetworkStatus(NetworkStatus status, string message = null)
        {
            NetworkStatus = status;
            NetworkMessage = message;
        }

        public void SetNetworkError(ErrorCode error, string message)
        {
            NetworkError = error;
            NetworkMessage = message;
        }

        public void ClearNetwork()
        {
            NetworkPing = 0;
            NetworkStatus = NetworkStatus.Disconnected;
            NetworkError = ErrorCode.None;
            NetworkMessage = null;
        }

        public void ClearAccount()
        {
            UserData = null;
            AccountID = 0;
            AccountName = null;
            IsPublicServer = false;
        }
    }
}
