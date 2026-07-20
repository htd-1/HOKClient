using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 网络消息 → 业务绑定。
    /// <para>集中注册各 CMD 的监听者（原 6 个 <c>XxxMessageHandler</c> 逻辑等价迁移），
    /// 经 <see cref="NetMsg.Subscribe"/> 挂到 TEngine GameEvent（string-keyed）。</para>
    /// <para>行为与重构前逐 Handler 一致：RuntimeData 写入 + 领域 GameEvent 触发。
    /// Procedures/UI 订阅的 <c>ILoginEvent</c>/<c>ILobbyEvent</c>/<c>IBattleEvent</c> 契约不变。</para>
    /// <para>注：<c>RspPing</c> 由 <see cref="NetSvc"/> 传输层 <c>HandleRspPing</c> 直接处理，不在此注册。</para>
    /// </summary>
    public static class NetMessageBindings
    {
        public static void Register()
        {
            // 登录响应：路由层调 LoginSystem 写 SessionState（内部双写 RuntimeData）+ 发 IPlayerData/IPlayerEvent。
            // 切换期直调 HandleRspLogin；阶段 3.4 改为发"原始包事件"让 LoginSystem 监听，路由层不再持 System 引用。
            NetMsg.Subscribe(CMD.RspLogin, msg =>
            {
                LoginSystem.Instance.HandleRspLogin(msg.rspLogin);
            });

            // 匹配（路由层只发原始包事件，写状态归 LobbySystem）
            NetMsg.Subscribe(CMD.RspMatch, msg => GameEvent.Get<ILobbyEvent>().OnRspMatch(msg.rspMatch));

            // 确认
            NetMsg.Subscribe(CMD.NtfConfirm, msg => GameEvent.Get<ILobbyEvent>().OnNtfConfirm(msg.ntfConfirm));

            // 选英雄
            NetMsg.Subscribe(CMD.NtfSelect, msg => GameEvent.Get<ILobbyEvent>().OnNtfSelect());

            // 加载资源
            NetMsg.Subscribe(CMD.NtfLoadRes, msg => GameEvent.Get<ILobbyEvent>().OnNtfLoadRes(msg.ntfLoadRes));

            // 加载进度
            NetMsg.Subscribe(CMD.NtfLoadPrg, msg => GameEvent.Get<ILobbyEvent>().OnNtfLoadPrg(msg.ntfLoadPrg));

            // 战斗开始（路由层只发原始包事件，写状态归 BattleSystem）
            NetMsg.Subscribe(CMD.RspBattleStart, msg => GameEvent.Get<IBattleEvent>().OnRspBattleStart(msg.rspBatlleStart));

            // 操作码
            NetMsg.Subscribe(CMD.NtfOpKey, msg => GameEvent.Get<IBattleEvent>().OnNtfOpKey(msg.ntfOpKey));

            // 聊天
            NetMsg.Subscribe(CMD.NtfChat, msg => GameEvent.Get<IBattleEvent>().OnNtfChat(msg.ntfChat));

            // 战斗结算
            NetMsg.Subscribe(CMD.RspBattleEnd, msg => GameEvent.Get<IBattleEvent>().OnRspBattleEnd(msg.rspBattleEnd));
        }
    }
}
