using System.Collections.Generic;

using HOKProtocol;
using PEMath;
using PEPhysx;
using TEngine;

using AudioType = TEngine.AudioType;

namespace GameLogic
{
    /// <summary>
    /// 战斗流程状态机节点。
    /// </summary>
    public class ProcedureBattle : ProcedureBase
    {
        private IFsm<IProcedureModule> _procedureOwner;
        private GameEventMgr _mgr = new();
        private bool _isTickFight;
      
        private FightMgr _fightMgr;
        
        protected override void OnInit(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnInit(procedureOwner);
            _procedureOwner = procedureOwner;
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _mgr.AddEvent<NtfOpKey>(IBattleEvent_Event.OnNtfOpKey, OnNtfOpKey);
            _mgr.AddEvent<RspBattleEnd>(IBattleEvent_Event.OnRspBattleEnd, OnRspBattleEnd);
            // _mgr.AddEvent<int, int>(IBattleEndUI_Event.OnBattleEnd, OnOfflineBattleEnd);
        }
        
        private void ClearEvents()
        {
            _mgr.Clear();
        }
        protected override void OnDestroy(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnDestroy(procedureOwner);
            ClearEvents();
        }

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameModule.UI.CloseUI<LoadUI>();
            GameModule.UI.ShowUI<PlayUI>();
            GameModule.UI.ShowUI<HPUI>();
            GameModule.Audio.Play(AudioType.Music,GameServices.Config.GetAudio(AudioKey.BattleBgm));

            GameModule.Audio.Play(AudioType.Sound, GameServices.Config.GetAudio(AudioKey.Welcombattle));
            
            _fightMgr=new FightMgr();
            // 启动数据从域 BattleState 读（BattleSystem.OnNtfLoadRes 已流入）
            MapCfg mapCfg = GameServices.Config.GetMap(BattleSystem.Instance.BattleMapID);
            _fightMgr.Init(BattleSystem.Instance.BattleHeroList, mapCfg, BattleSystem.Instance.BattleSelfIndex);
            _isTickFight = true;

            // roomID 由 BattleSystem 跨域流入（OnNtfConfirm）；selfIndex 用 FightMgr 自身值。FSM 收口豁免读 Instance。
            BattleInputSvc.Instance.Active(_fightMgr, _fightMgr.SelfIndex, BattleSystem.Instance.MatchRoomID);
        }

        protected override void OnLeave(IFsm<IProcedureModule> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameModule.UI.CloseUI<PlayUI>();
            GameModule.UI.CloseUI<HPUI>();
            BattleInputSvc.Instance.Release();
            BattleSystem.Instance.ClearBattle(); // 清 BattleState（FSM 收口豁免 Instance）；加载进度数据由下次进大厅 ClearFlow 清
        }


        protected override void OnUpdate(IFsm<IProcedureModule> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            _fightMgr?.Update();
        }

        private void OnNtfOpKey(NtfOpKey data)
        {
            // 战斗就绪前（OnInit 已注册监听，但 OnEnter 未跑、_fightMgr 为空）也会收到 GM 模拟下发的 NtfOpKey，需判空跳过。
            if (_fightMgr == null) return;
            _fightMgr.InputKey(data.keyList);
            if(_isTickFight)_fightMgr.Tick();
        }

        private void OnRspBattleEnd(RspBattleEnd data)
        {
            
            ExitToResult();
        }
        

      
        private void ExitToResult()
        {
            if (!_isTickFight) return;
            _isTickFight = false;
           
            GameModule.UI.ShowUI<ResultUI>();
            
        }
        
        public List<PEColliderBase> GetEnvColliders()
        {
            return _fightMgr.GetEnvColliders();
        }

        
    }
}
