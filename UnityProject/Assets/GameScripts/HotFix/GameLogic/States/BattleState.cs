using System.Collections.Generic;
using HOKProtocol;

namespace GameLogic
{
    /// <summary>
    /// L5 域状态：战斗（启动数据 + 战斗中数据 + 结算）。由 <see cref="BattleSystem"/> 私有持有。
    /// <para>纯数据 POCO，不耦合事件总线；变更由 BattleSystem 写入后发 <c>IBattleEvent.OnBattleDataChanged</c>。</para>
    /// <para>启动数据（MapID/HeroList/SelfIndex）由 BattleSystem 在 <c>OnNtfLoadRes</c> 流入（早于 <c>RspBattleStart</c>，
    /// 保证 <see cref="BattleSystem"/>.EnterBattle 初始化 <see cref="FightMgr"/> 时可读，无事件顺序竞争）。</para>
    /// <para>房间号（MatchRoomID）由 BattleSystem 在 <c>OnNtfConfirm</c> 跨域流入（NtfConfirm.roomID），供 <see cref="BattleSystem"/> 战斗中发包。</para>
    /// </summary>
    public sealed class BattleState
    {
        // === 战斗启动数据（Load 阶段 NtfLoadRes 填，跨 Load→Battle→战斗结束）===
        public int BattleMapID { get; private set; }
        public List<BattleHeroData> BattleHeroList { get; private set; }
        public int BattleSelfIndex { get; private set; } = -1;

        // === 战斗房间号（NtfConfirm 建立，跨匹配→战斗；BattleSystem 发操作包用）===
        public uint MatchRoomID { get; private set; }

        // === 战斗中数据（RspBattleStart 起，战斗中持续）===
        public RspBatlleStart BattleStartData { get; private set; }
        public NtfOpKey LastOpKey { get; private set; }
        public NtfChat LastChat { get; private set; }
        public bool IsBattleTicking { get; private set; }

        // === 结算（RspBattleEnd 填）===
        public RspBattleEnd BattleResultData { get; private set; }

        /// <summary>战斗启动数据流入（BattleSystem.OnNtfLoadRes 调）。</summary>
        public void SetStartupData(int mapID, List<BattleHeroData> heroList, int selfIndex)
        {
            BattleMapID = mapID;
            BattleHeroList = heroList;
            BattleSelfIndex = selfIndex;
        }

        /// <summary>流入战斗房间号（BattleSystem.OnNtfConfirm 调；dismiss 传 0 清空）。</summary>
        public void SetMatchRoomID(uint roomID) => MatchRoomID = roomID;

        public void StartBattle(RspBatlleStart data)
        {
            BattleStartData = data;
            IsBattleTicking = true;
        }

        public void SetOpKey(NtfOpKey data) => LastOpKey = data;

        public void SetChat(NtfChat data) => LastChat = data;

        public void FinishBattle(RspBattleEnd data)
        {
            BattleResultData = data;
            IsBattleTicking = false;
        }

        /// <summary>清战斗启动 + 战斗中数据 + 房间号（不含结算；结算由 <see cref="ClearResult"/> 单独清）。</summary>
        public void ClearBattle()
        {
            BattleMapID = 0;
            BattleHeroList = null;
            BattleSelfIndex = -1;
            MatchRoomID = 0;
            BattleStartData = null;
            LastOpKey = null;
            LastChat = null;
            IsBattleTicking = false;
        }

        public void ClearResult() => BattleResultData = null;
    }
}
