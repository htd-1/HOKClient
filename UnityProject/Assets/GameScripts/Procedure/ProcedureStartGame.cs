using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Launcher;
using TEngine;

namespace Procedure
{
    public class ProcedureStartGame : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        // private IFsm<IProcedureModule> _procedureOwner;

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            // _procedureOwner = procedureOwner;
            StartGame().Forget();
        }

        private async UniTaskVoid StartGame()
        {
            await UniTask.Yield();
            LauncherMgr.HideAllUI();

            // // 通过反射获取热更 Procedure 类型
            // var loginType = FindType("ProcedureLogin");
            // var lobbyType = FindType("ProcedureLobby");
            // if (loginType == null || lobbyType == null)
            // {
            //     Log.Error("Failed to find hotfix Procedure types via reflection");
            //     return;
            // }
            //
            // // 反射创建实例
            // var loginProc = Activator.CreateInstance(loginType);
            // var lobbyProc = Activator.CreateInstance(lobbyType);
            //
            // // 创建游戏流程 FSM
            // var fsm = GameModule.Fsm.CreateFsm("GameFlow", GameModule.Procedure,
            //     (FsmState<IProcedureModule>)loginProc,
            //     (FsmState<IProcedureModule>)lobbyProc
            // );
            // fsm.Start(loginType);
            // Log.Info("GameFlow FSM Started");
        }


    }
}
