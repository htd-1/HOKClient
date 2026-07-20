using TEngine;

namespace GameLogic
{
    /// <summary>
    /// ①命令（上行·意图·L1 UI → L3 BattleSystem）。战斗快照拉取。
    /// <para>UI 经 <c>GameEvent.Get&lt;IBattleCmd&gt;().Method()</c> 触发，BattleSystem 单 handler 监听。</para>
    /// <para>战斗操作（移动/技能）走 <see cref="BattleInputSvc"/> 直接发包（④基础设施直接调用），不经命令通道。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IBattleCmd
    {
        /// <summary>UI 打开时拉当前战斗快照（BattleSystem 补推 IBattleData.Changed）。</summary>
        void RequestSnapshot();
    }
}
