using HOKProtocol;
using PEMath;
using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IBattleEvent
    {
        void OnRspBattleStart(RspBatlleStart data);
        void OnNtfOpKey(NtfOpKey data);
        void OnNtfChat(NtfChat data);
        void OnRspBattleEnd(RspBattleEnd data);

        /// <summary>自身英雄全部加载完成(FightMgr.Init 末尾发送,传入 self hero 的 MainLogicUnit;接收方据此取 MainViewUnit as HeroView)。</summary>
        void OnSelfHeroLoaded(MainLogicUnit selfHero);

        
    }
}
