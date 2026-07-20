using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// L3 Session 域业务系统。
    /// <para>监听 <see cref="IPlayerCmd"/>（①命令）处理登录/GM/选服；登录响应由 NetMessageBindings 经
    /// <see cref="HandleRspLogin"/> 接入（切换期），处理后写 <see cref="SessionState"/> 并发
    /// <see cref="IPlayerData"/>（③推送）/ <see cref="IPlayerEvent"/>（②通知）。</para>
    /// <para>对基础设施 <see cref="NetSvc"/> 走 ④直接调用（选服/发包/网络状态）。</para>
    /// <para><b>纪律</b>：业务代码不接触 <c>LoginSystem.Instance</c>，只经 <c>GameEvent.Get&lt;IPlayerCmd&gt;()</c> 触发。</para>
    /// </summary>
    public sealed class LoginSystem : GameSystem<LoginSystem>
    {
        private readonly SessionState _state = new SessionState();

        protected override void RegisterCommands(GameEventMgr events)
        {
            // ①命令（UI 发）
            events.AddEvent<string, string>(IPlayerCmd_Event.Login, OnLogin);
            events.AddEvent(IPlayerCmd_Event.GmIntent, OnGmIntent);
            events.AddEvent<bool>(IPlayerCmd_Event.ServerSelect, OnServerSelect);
            events.AddEvent(IPlayerCmd_Event.RequestSnapshot, OnRequestSnapshot);
            // ②网络状态（NetSvc L4 发 INetworkEvent，LoginSystem L3 监听写 SessionState L5；分层修正：L4 不越界直写 L5）
            events.AddEvent<NetworkStatus>(INetworkEvent_Event.OnNetworkStatusChanged, OnNetworkStatusChanged);
        }

        private void OnLogin(string account, string password)
        {
            // ④直接调用基础设施（传输底座，像 Log）
            NetSvc.Instance.Send(new HOKMsg
            {
                cmd = CMD.ReqLogin,
                reqLogin = new ReqLogin { acct = account, pass = password }
            });
        }

        private void OnGmIntent()
        {
            GameServices.GM.StartSimulate();
        }

        private void OnServerSelect(bool isPublicServer)
        {
            _state.SetServerSelection(isPublicServer);
            // 选服交传输层持有（NetSvc.Connect 据此选 IP）；不再双写 RuntimeData
            NetSvc.Instance.SetServerSelection(isPublicServer);
        }

        private void OnRequestSnapshot()
        {
            // UI 打开时补推当前快照
            GameEvent.Get<IPlayerData>().Changed(_state.UserData);
        }

        private void OnNetworkStatusChanged(NetworkStatus status)
        {
            // NetSvc.SetStatus 发 INetworkEvent.OnNetworkStatusChanged(status)。
            // message/ping/error 经现有签名未携带，阶段1仅镜像 status；RuntimeData 由 NetSvc 直写（同源），阶段4网络域统一接入时补全。
            _state.SetNetworkStatus(status);
        }

        /// <summary>登录响应入口（NetMessageBindings 切换期调用；后续路由层改造改为发原始包事件）。</summary>
        public void HandleRspLogin(RspLogin data)
        {
            _state.SetUserData(data.userData);
            // ③数据订阅推送（UI 据此刷新）+ ②结果通知（Procedure 据此流转）
            GameEvent.Get<IPlayerData>().Changed(_state.UserData);
            GameEvent.Get<IPlayerEvent>().LoginResult(true, "登录成功");
        }

        /// <summary>只读查询入口（兜底：即时读取当前 UserData 快照）。</summary>
        public UserData GetUserData() => _state.UserData;
    }
}
