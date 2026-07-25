using HOKProtocol;
using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IBattleEvent
    {
        void OnRspBattleStart(RspBatlleStart data);
        void OnNtfChat(NtfChat data);
        void OnRspBattleEnd(RspBattleEnd data);

        /// <summary>自身英雄全部加载完成(FightMgr.Init 末尾发送,传入 self hero 的 MainLogicUnit;接收方据此取 MainViewUnit as HeroView)。</summary>
        void OnSelfHeroLoaded(MainLogicUnit selfHero);

        /// <summary>战斗状态变更推送（L3 BattleSystem → L1 UI）。合并自旧 IBattleData：UI 订阅接收 BattleState，首次打开调 BattleSystem.RequestSnapshot 补推。</summary>
        void OnBattleDataChanged(BattleState state);

        
    }
}
