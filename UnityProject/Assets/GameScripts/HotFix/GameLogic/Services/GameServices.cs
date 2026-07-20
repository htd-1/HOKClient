using Cysharp.Threading.Tasks;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 服务定位器（L4 基础设施门面）。仅保留配置查询（Config）与 GM 工具（GM）。
    /// <para>网络传输由 <see cref="NetSvc"/> Singleton 承载，消息路由由 <see cref="NetMsg"/> + <see cref="NetMessageBindings"/> 承载。</para>
    /// <para>业务数据读写已全部归位各域 <c>XxxState</c>（SessionState/LobbyState/BattleState），经 GameEvent 推送订阅——
    /// 不再有全局 RuntimeData 袋，业务代码经 <c>GameEvent.Get&lt;IXxxCmd&gt;()</c> 触发、订阅 <c>IXxxData.Changed</c> 读数据。</para>
    /// </summary>
    public static class GameServices
    {
        public static ConfigService Config { get; private set; }
        public static GMService GM { get; private set; }

        public static async UniTask InitializeAsync()
        {
            Config = new ConfigService();
            await Config.LoadAsync();

            NetSvc.Instance.Active();
            BattleInputSvc.Instance.Active();
            NetMessageBindings.Register();
            GM = GMService.Instance;
            GM.Active();

            // L3 业务系统启动。Instance 仅此处用于 Active，业务代码不接触 Instance。
            LoginSystem.Instance.Active();
            LobbySystem.Instance.Active();
            BattleSystem.Instance.Active();
        }

        public static void Release()
        {
            if (NetSvc.IsValid)
            {
                NetSvc.Instance.Release();
            }

            Config?.Release();
            Config = null;

            if (BattleSystem.IsValid) BattleSystem.Instance.Release();
            if (LobbySystem.IsValid) LobbySystem.Instance.Release();
            if (LoginSystem.IsValid) LoginSystem.Instance.Release();
        }
    }
}
