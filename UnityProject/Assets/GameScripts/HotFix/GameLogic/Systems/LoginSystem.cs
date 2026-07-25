using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// L3 Session 域业务系统。
    /// <para>上行命令（登录/GM/选服）为 public 方法，UI 直接 <c>LoginSystem.Instance.Method()</c> 调用；登录响应由
    /// <see cref="NetSvc.HandoutMsg"/> 直调 <see cref="RspLogin"/> 接入，处理后写 <see cref="SessionState"/> 并发
    /// <see cref="IPlayerEvent"/>（下行通知）。</para>
    /// <para>对基础设施 <see cref="NetSvc"/> 走直接调用（选服/发包/网络状态）。</para>
    /// </summary>
    public sealed class LoginSystem : Singleton<LoginSystem>
    {
        private readonly SessionState _state = new SessionState();
        private GameEventMgr _events;

        public override void Active()
        {
            _events ??= new GameEventMgr();
            // 网络状态（NetSvc 发 INetworkEvent，LoginSystem 监听写 SessionState）
            _events.AddEvent<NetworkStatus>(INetworkEvent_Event.OnNetworkStatusChanged, OnNetworkStatusChanged);
        }

        public override void Release()
        {
            _events?.Clear();
            _events = null;
            base.Release();
        }

        public void Login(string account, string password)
        {
            // ④直接调用基础设施（传输底座，像 Log）
            NetSvc.Instance.Send(new HOKMsg
            {
                cmd = CMD.ReqLogin,
                reqLogin = new ReqLogin { acct = account, pass = password }
            });
        }

        public void GmIntent()
        {
            GMService.Instance.StartSimulate();
        }

        public void ServerSelect(bool isPublicServer)
        {
            _state.SetServerSelection(isPublicServer);
            // 选服交传输层持有（NetSvc.Connect 据此选 IP）；不再双写 RuntimeData
            NetSvc.Instance.SetServerSelection(isPublicServer);
        }

        public void RequestSnapshot()
        {
            // UI 打开时补推当前快照
            GameEvent.Get<IPlayerEvent>().Changed(_state.UserData);
        }

        private void OnNetworkStatusChanged(NetworkStatus status)
        {
            // NetSvc.SetStatus 发 INetworkEvent.OnNetworkStatusChanged(status)。
            // message/ping/error 经现有签名未携带，阶段1仅镜像 status；RuntimeData 由 NetSvc 直写（同源），阶段4网络域统一接入时补全。
            _state.SetNetworkStatus(status);
        }

        /// <summary>登录响应入口（NetSvc.HandoutMsg 直调；与其它 Sys 包处理方法统一，不再有越界特例）。</summary>
        public void RspLogin(RspLogin data)
        {
            _state.SetUserData(data.userData);
            // ③数据订阅推送（UI 据此刷新）+ ②结果通知（Procedure 据此流转）
            GameEvent.Get<IPlayerEvent>().Changed(_state.UserData);
            GameEvent.Get<IPlayerEvent>().LoginResult(true, "登录成功");
        }

        /// <summary>只读查询入口（兜底：即时读取当前 UserData 快照）。</summary>
        public UserData GetUserData() => _state.UserData;
    }
}
