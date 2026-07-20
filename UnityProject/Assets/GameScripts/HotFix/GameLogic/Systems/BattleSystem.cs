using System.Collections.Generic;
using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// L3 Battle 域业务系统。
    /// <para>监听 <see cref="IBattleCmd"/>（①命令）处理快照拉取；监听 <see cref="IBattleEvent"/>（②原始包下行，
    /// 由 <see cref="NetMessageBindings"/> 发）写 <see cref="BattleState"/> 发 <see cref="IBattleData"/>（③推送）。</para>
    /// <para><b>不持有 <see cref="FightMgr"/></b>：FightMgr 生命周期/帧驱动留 <see cref="ProcedureBattle"/>
    /// （design：仅接入事件接缝，不动 FightManager 内部逻辑）。本类仅管战斗状态 + 事件接缝。</para>
    /// <para>跨域接缝（Lobby→Battle）：<c>OnNtfConfirm</c> 流入房间号、<c>OnNtfLoadRes</c> 流入启动数据（MapID/HeroList/SelfIndex），
    /// 均早于 <c>RspBattleStart</c>，保证 <see cref="ProcedureBattle"/>.OnEnter 与 <see cref="BattleInputSvc"/> 发包时已就绪。</para>
    /// <para><see cref="BattleState"/> 为本域唯一状态源（旧 RuntimeData 双写已删）。</para>
    /// <para><b>纪律</b>：业务代码不接触 <c>BattleSystem.Instance</c>，只经 <c>GameEvent.Get&lt;IBattleCmd&gt;()</c> 触发；
    /// ProcedureBattle 读启动数据/调 ClearBattle 为启动初始化与 FSM 收口的一次性豁免。</para>
    /// </summary>
    public sealed class BattleSystem : GameSystem<BattleSystem>
    {
        private readonly BattleState _state = new BattleState();

        // 只读查询：供 ProcedureBattle.OnEnter 初始化 FightMgr + BattleInputSvc（启动数据/roomID 由跨域接缝流入，已就绪）
        public int BattleMapID => _state.BattleMapID;
        public List<BattleHeroData> BattleHeroList => _state.BattleHeroList;
        public int BattleSelfIndex => _state.BattleSelfIndex;
        public uint MatchRoomID => _state.MatchRoomID;

        protected override void RegisterCommands(GameEventMgr events)
        {
            // ①命令（UI 发）
            events.AddEvent(IBattleCmd_Event.RequestSnapshot, OnRequestSnapshot);

            // ②跨域原始包事件（NetMessageBindings 发 ILobbyEvent/IBattleEvent；ProcedureBattle 亦听 OnNtfOpKey/OnRspBattleEnd 驱动 FightMgr/FSM）
            events.AddEvent<NtfConfirm>(ILobbyEvent_Event.OnNtfConfirm, OnNtfConfirm); // 跨域接缝：捕获 roomID 供战斗发包
            events.AddEvent<NtfLoadRes>(ILobbyEvent_Event.OnNtfLoadRes, OnNtfLoadRes);  // 跨域接缝：启动数据流入
            events.AddEvent<RspBatlleStart>(IBattleEvent_Event.OnRspBattleStart, OnRspBattleStart);
            events.AddEvent<NtfOpKey>(IBattleEvent_Event.OnNtfOpKey, OnNtfOpKey);
            events.AddEvent<NtfChat>(IBattleEvent_Event.OnNtfChat, OnNtfChat);
            events.AddEvent<RspBattleEnd>(IBattleEvent_Event.OnRspBattleEnd, OnRspBattleEnd);
        }

        private void OnRequestSnapshot()
        {
            GameEvent.Get<IBattleData>().Changed(_state);
        }

        // === 跨域接缝：捕获战斗房间号（NtfConfirm.roomID；dismiss 清 0）===
        private void OnNtfConfirm(NtfConfirm confirm)
        {
            _state.SetMatchRoomID(confirm.dissmiss ? 0 : confirm.roomID);
        }

        // === 启动数据流入（Lobby→Battle 跨域接缝）===
        private void OnNtfLoadRes(NtfLoadRes data)
        {
            // 从原始包直接填 BattleState 启动数据（与 LobbySystem.OnNtfLoadRes 同源）。
            // 不发 Changed：战斗 UI 尚未打开无订阅者，且此为启动预备数据非"战斗中"变更。
            _state.SetStartupData(data.mapID, data.heroList, data.posIndex);
        }

        // === ②原始包下行处理 ===
        private void OnRspBattleStart(RspBatlleStart data)
        {
            _state.StartBattle(data);
            GameEvent.Get<IBattleData>().Changed(_state);
        }

        private void OnNtfOpKey(NtfOpKey data)
        {
            _state.SetOpKey(data);
            GameEvent.Get<IBattleData>().Changed(_state);
        }

        private void OnNtfChat(NtfChat data)
        {
            _state.SetChat(data);
            GameEvent.Get<IBattleData>().Changed(_state);
        }

        private void OnRspBattleEnd(RspBattleEnd data)
        {
            _state.FinishBattle(data);
            GameEvent.Get<IBattleData>().Changed(_state);
        }

        /// <summary>清战斗状态（ProcedureBattle.OnLeave 调；清启动 + 房间号 + 战斗中，不含结算）。</summary>
        public void ClearBattle()
        {
            _state.ClearBattle();
        }
    }
}
