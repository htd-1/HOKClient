using TEngine;

namespace GameLogic
{
    // ====================================================================================
    // 架构规约（详见 openspec/changes/decouple-five-layer-architecture）
    // ====================================================================================
    // 扁平五层(依赖单向向下, 反向只走事件):
    //   L1 表现层  UIWindow/UIWidget
    //   L2 流程层  Procedure/FSM        (纯编排: ChangeState + ShowUI/CloseUI + LoadScene)
    //   L3 业务层  GameSystem<T> 派生    (本基类: 监听命令 → 执行业务 → 写域 state → 发通知)
    //   L4 基础设施 NetSvc/ConfigService/GameModule
    //   L5 数据层  域 State(SessionState/LobbyState/BattleState)
    //
    // 四通道通信:
    //   ① 命令  上行·意图   I{Domain}Cmd     (约定单 handler)     L1/L2 → L3
    //   ② 事件  下行·通知   I{Domain}Event   (广播)               L3 → L1/L2
    //   ③ 订阅  下行·变更   I{Domain}Data    (Changed)            L5 → L1
    //   ④ 调用  直接        NetSvc/GameModule (仅限基础设施)        L3 → L4
    //   层间走①②③; 对基础设施走④。
    //
    // 命名: I{Domain}Cmd / I{Domain}Event / I{Domain}Data = GroupLogic; I{Domain}UI = GroupUI
    // 纪律: 业务代码(L1/L2)零持有 System 实例, 只发①命令; Instance 仅 GameServices 启动器用于 Active/Release。
    // ====================================================================================

    /// <summary>
    /// L3 业务系统基类。组合项目 <see cref="Singleton{T}"/>（实例生命周期：OnInit/Active/Release）
    /// 与 TEngine <see cref="GameEventMgr"/>（命令监听生命周期：注册/Clear）。
    /// <para>子类 <see cref="RegisterCommands"/> 注册要监听的①命令事件；<see cref="Active"/> 注册、<see cref="Release"/> 清理。</para>
    /// <para>域 State 作为子类私有成员持有（域内聚，不全局暴露）；写入触发 ③ <c>I{Domain}Data.Changed</c>。</para>
    /// <para><b>纪律</b>：业务代码（L1 UI / L2 Procedure）MUST NOT 访问 <c>XxxSystem.Instance</c>，
    /// 只经 <c>GameEvent.Get{IXxxCmd}().Method()</c> 触发；<c>Instance</c> 仅 <see cref="GameServices"/> 启动器用于生命周期。</para>
    /// </summary>
    public abstract class GameSystem<T> : Singleton<T> where T : GameSystem<T>, new()
    {
        /// <summary>①命令监听生命周期管理器（TEngine）。<see cref="Active"/> 时创建并注册，<see cref="Release"/> 时 Clear。</summary>
        protected GameEventMgr Events;

        /// <summary>启动：创建监听管理器并交子类注册①命令事件。由 <see cref="GameServices"/> 启动器显式调用。</summary>
        public override void Active()
        {
            Events ??= new GameEventMgr();
            RegisterCommands(Events);
        }

        /// <summary>关闭：清理全部命令监听后释放单例。</summary>
        public override void Release()
        {
            Events?.Clear();
            Events = null;
            base.Release();
        }

        /// <summary>子类注册要监听的①命令事件（经 <paramref name="events"/> 统一管理生命周期，防泄漏）。</summary>
        protected abstract void RegisterCommands(GameEventMgr events);
    }
}
