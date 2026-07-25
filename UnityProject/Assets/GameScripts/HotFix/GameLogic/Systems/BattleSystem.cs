using System.Collections.Generic;
using HOKProtocol;
using PEMath;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// L3 Battle 域业务系统：收编 FightMgr（创建/帧驱动）+ 战斗输入（采集/校验/发包，并入自 BattleInputSvc）。
    /// <para>战斗原始包（RspBattleStart/NtfOpKey/NtfChat/RspBattleEnd）由 <see cref="NetSvc.HandoutMsg"/> 直调本系统 public 方法（不再经 GameEvent 路由）；
    /// 处理后写 <see cref="BattleState"/> + 发 <see cref="IBattleEvent"/>.OnBattleDataChanged（下行）。
    /// 多消费者包（RspBattleStart/RspBattleEnd）重广播 <c>IBattleEvent.OnXxx</c> 供 Procedure(FSM)响应流转。</para>
    /// <para>NtfOpKey 处理后直接驱动 <see cref="FightMgr"/>.InputKey+Tick（不再经事件绕 <see cref="ProcedureBattle"/>）。</para>
    /// <para><see cref="ProcedureBattle"/> 仅做 FSM 编排：OnEnter→<see cref="EnterBattle"/>、OnUpdate→<see cref="Tick"/>、OnLeave→<see cref="ExitBattle"/>。</para>
    /// <para>跨域接缝（Lobby→Battle）：监听 <c>ILobbyEvent.OnNtfConfirm</c>/<c>OnNtfLoadRes</c>（由 LobbySystem 重广播）流入 roomID/启动数据，
    /// 均早于 <c>RspBattleStart</c>，保证 <see cref="EnterBattle"/> 与发包时已就绪。</para>
    /// <para><see cref="BattleState"/> 为本域唯一状态源。</para>
    /// </summary>
    public sealed class BattleSystem : Singleton<BattleSystem>
    {
        private readonly BattleState _state = new BattleState();
        private GameEventMgr _events;

        // === FightMgr（EnterBattle 创建，ExitBattle 释放）===
        private FightMgr _fightMgr;
        private bool _isTicking;

        // === 战斗输入（并入自 BattleInputSvc）===
        private int _selfIndex;
        private uint _roomID;
        private uint _keyID;
        private bool _inputActive;

        /// <summary>技能轮盘手感常量（SkillItem 拖拽方向缩放用）。</summary>
        public float SkillDisMultipler = 0.03f;

        // 跨阶段/跨层只读查询：ProcedureLoad 加载战斗场景读 mapID（早于 EnterBattle）；Hero.IsPlayerSelf 读 selfIndex；
        // FightMgr.Init 走 instance 直读启动英雄列表（替代原 Init 三参注入）。
        public int BattleMapID => _state.BattleMapID;
        public int BattleSelfIndex => _state.BattleSelfIndex;
        public List<BattleHeroData> BattleHeroList => _state.BattleHeroList;

        public override void Active()
        {
            _events ??= new GameEventMgr();
            // 跨域接缝：LobbySystem 重广播的 ILobbyEvent（捕获 roomID + 战斗启动数据）
            _events.AddEvent<NtfConfirm>(ILobbyEvent_Event.OnNtfConfirm, OnNtfConfirm);
            _events.AddEvent<NtfLoadRes>(ILobbyEvent_Event.OnNtfLoadRes, OnNtfLoadRes);
        }

        public override void Release()
        {
            _events?.Clear();
            _events = null;
            base.Release();
        }

        // === 跨域接缝：捕获战斗房间号（NtfConfirm.roomID；dismiss 清 0）===
        private void OnNtfConfirm(NtfConfirm confirm)
        {
            _state.SetMatchRoomID(confirm.dissmiss ? 0 : confirm.roomID);
        }

        // === 启动数据流入（Lobby→Battle 跨域接缝）===
        private void OnNtfLoadRes(NtfLoadRes data)
        {
            // 从原始包直接填 BattleState 启动数据（与 LobbySystem.NtfLoadRes 同源）。
            _state.SetStartupData(data.mapID, data.heroList, data.posIndex);
        }

        // === 战斗生命周期（ProcedureBattle 编排入口）===

        /// <summary>进入战斗：创建 FightMgr + 初始化 + 激活输入（ProcedureBattle.OnEnter 调）。启动数据/roomID 由跨域接缝流入。</summary>
        public void EnterBattle()
        {
            _fightMgr = new FightMgr();
            // FightMgr 走 instance 直读启动数据（mapID/heroList/selfIndex），无需 owner 注入。
            _fightMgr.Init();

            // 输入激活：selfIndex 取 FightMgr 解析值，roomID 由跨域接缝流入。
            _selfIndex = _fightMgr.SelfIndex;
            _roomID = _state.MatchRoomID;
            _inputActive = true;
            _isTicking = true;
        }

        /// <summary>战斗帧驱动（ProcedureBattle.OnUpdate 调；FightMgr.Update 跟随相机）。</summary>
        public void Tick()
        {
            _fightMgr?.Update();
        }

        /// <summary>离开战斗：停 Tick + 停输入 + 释放 FightMgr + 清战斗状态（ProcedureBattle.OnLeave 调）。</summary>
        public void ExitBattle()
        {
            _isTicking = false;
            _inputActive = false;
            _fightMgr?.UnInit();
            _fightMgr = null;
            _state.ClearBattle();
        }

        // === 原始包下行（NetSvc.HandoutMsg 直调，public 入口）===

        /// <summary>战斗开始（多消费者：重广播 OnRspBattleStart 供 ProcedureLoad→ProcedureBattle 流转）。</summary>
        public void RspBattleStart(RspBatlleStart data)
        {
            _state.StartBattle(data);
            GameEvent.Get<IBattleEvent>().OnBattleDataChanged(_state);
            GameEvent.Get<IBattleEvent>().OnRspBattleStart(data);
        }

        /// <summary>操作码下发：写状态 + 直接驱动 FightMgr.InputKey/Tick（迁自 ProcedureBattle.OnNtfOpKey）。</summary>
        public void NtfOpKey(NtfOpKey data)
        {
            _state.SetOpKey(data);
            GameEvent.Get<IBattleEvent>().OnBattleDataChanged(_state);
            // 战斗就绪前（GM 早到）_fightMgr 为空判空跳过；结算后 _isTicking=false 停 Tick。
            if (_fightMgr == null) return;
            _fightMgr.InputKey(data.keyList);
            if (_isTicking) _fightMgr.Tick();
        }

        /// <summary>聊天（仅写状态 + UI 下行，无下游消费者）。</summary>
        public void NtfChat(NtfChat data)
        {
            _state.SetChat(data);
            GameEvent.Get<IBattleEvent>().OnBattleDataChanged(_state);
        }

        /// <summary>战斗结算（多消费者：重广播 OnRspBattleEnd 供 ProcedureBattle→结算 UI 流转；停 Tick）。</summary>
        public void RspBattleEnd(RspBattleEnd data)
        {
            _state.FinishBattle(data);
            _isTicking = false;
            GameEvent.Get<IBattleEvent>().OnBattleDataChanged(_state);
            GameEvent.Get<IBattleEvent>().OnRspBattleEnd(data);
        }

        /// <summary>UI 打开时拉当前战斗快照（PlayUI.OnCreate 直调，同步补推 OnBattleDataChanged）。</summary>
        public void RequestSnapshot()
        {
            GameEvent.Get<IBattleEvent>().OnBattleDataChanged(_state);
        }

        // === 战斗输入（并入自 BattleInputSvc；UI/Buff 直接 .Instance 调）===

        public bool IsActive => _inputActive;
        private uint KeyID => ++_keyID;

        /// <summary>发送移动帧操作到服务器。</summary>
        public bool SendMoveKey(PEVector3 logicDir)
        {
            if (!_inputActive) return false;
            if (!CanMove()) return false;
            var msg = new HOKMsg {
                cmd = CMD.SndOpKey,
                sndOpKey = new SndOpKey {
                    opKey = new OpKey {
                        opIndex = _selfIndex,
                        keyType = KeyType.Move,
                        moveKey = new MoveKey()
                    }
                }
            };
            msg.sndOpKey.opKey.moveKey.x = logicDir.x.ScaledValue;
            msg.sndOpKey.opKey.moveKey.z = logicDir.z.ScaledValue;
            msg.sndOpKey.opKey.moveKey.keyID = KeyID;
            NetSvc.Instance.Send(msg);
            return true;
        }

        public void SendSkillKey(int skillID) => SendSkillKey(skillID, Vector3.zero);

        public void SendSkillKey(int skillID, Vector3 vec)
        {
            if (!CanReleaseSkill(skillID))
            {
                Log.Info("skill can not release ");
                return;
            }
            HOKMsg msg = new HOKMsg
            {
                cmd = CMD.SndOpKey,
                sndOpKey = new SndOpKey
                {
                    roomID = _roomID,
                    opKey = new OpKey
                    {
                        opIndex = _selfIndex,
                        keyType = KeyType.Skill,
                        skillKey = new SkillKey
                        {
                            skillID = (uint)skillID,
                            x_value = ((PEInt)vec.x).ScaledValue,
                            z_value = ((PEInt)vec.z).ScaledValue,
                        }
                    }
                }
            };
            NetSvc.Instance.Send(msg);
        }

        public bool IsForbidSelfPlayerReleaseSkill()
        {
            return _fightMgr != null && _fightMgr.IsForbidReleaseSkill(_selfIndex);
        }

        private bool CanReleaseSkill(int skillID)
        {
            return _fightMgr != null && _fightMgr.CanReleaseSkill(_selfIndex, skillID);
        }

        private bool CanMove()
        {
            return _fightMgr != null && _fightMgr.CanMove(_selfIndex);
        }
    }
}
