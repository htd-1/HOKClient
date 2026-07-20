using TEngine;

namespace GameLogic
{
    /// <summary>
    /// Logic → 战斗结束事件接口（GroupUI）。
    /// 水晶塔被毁触发,通知 ResultUI(显示胜负)+ 编排层(BattleService/ProcedureBattle 退出战斗流程)。
    /// <para>[refactor-battle-logic 2.7.3] 迁自原 Tower.LogicUnInit 内 <c>BattleSys.Instance.EndBattle(win)</c>
    /// + <c>isTickFight=false</c>(表现层 MonoBehaviour 单例直调)。逻辑层只报"胜利方阵营 + 被毁水晶 hash",
    /// 不判自身胜负(自身阵营由订阅方据 winTeam 判定,Tower/逻辑层不依赖"玩家自身"概念,更解耦)。</para>
    /// <para>[2.7.4] FightWorld 转发本事件(订阅方可经 AddUIEvent/Get 订阅)。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupUI)]
    public interface IBattleEndUI
    {
        /// <summary>战斗结束。</summary>
        /// <param name="winTeam">胜利方阵营 ((int)TeamEnum;蓝水晶 1002 毁→Red,红水晶 2002 毁→Blue)。</param>
        /// <param name="deadTowerHash">被毁水晶塔单位 hash(UI 显示/定位用)。</param>
        void OnBattleEnd(int winTeam, int deadTowerHash);
    }
}
