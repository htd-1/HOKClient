using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GameLogic;
#if ENABLE_OBFUZ
using Obfuz;
#endif
using TEngine;
#pragma warning disable CS0436


/// <summary>
/// 游戏App。
/// </summary>
#if ENABLE_OBFUZ
[ObfuzIgnore(ObfuzScope.TypeName | ObfuzScope.MethodName)]
#endif
public partial class GameApp
{
    private static List<Assembly> _hotfixAssembly;

    /// <summary>
    /// 热更域App主入口。
    /// </summary>
    /// <param name="objects"></param>
    public static void Entrance(object[] objects)
    {
        GameEventHelper.Init();
        _hotfixAssembly = (List<Assembly>)objects[0];
        Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
        Log.Warning("======= Entrance GameApp =======");
        Utility.Unity.AddDestroyListener(Release);
        Log.Warning("======= StartGameLogic =======");
        StartGameLogic().Forget();
    }

    private static async UniTaskVoid StartGameLogic()
    {
        if (GameLogic.Constant.DebugMuteAudio)
        {
            GameModule.Audio.Enable = false;
            GameModule.Audio.MusicEnable = false;
            GameModule.Audio.SoundEnable = false;
            GameModule.Audio.UISoundEnable = false;
            GameModule.Audio.StopAll(false);
        }

        // 基础设施启动
        await ConfigService.Instance.LoadAsync();
        NetSvc.Instance.Active();
        GMService.Instance.Active();
        // 业务系统启动
        LoginSystem.Instance.Active();
        LobbySystem.Instance.Active();
        BattleSystem.Instance.Active();

        var fsm = GameModule.Fsm.CreateFsm("GameFlow", GameModule.Procedure,
            new ProcedureLogin(),
            new ProcedureStart(),
            new ProcedureLobby(),
            new ProcedureMatch(),
            new ProcedureSelect(),
            new ProcedureLoad(),
            new ProcedureBattle()
        );
        fsm.Start<ProcedureLogin>();
        Log.Info("GameFlow FSM Started");
    }

    private static void Release()
    {
        if (NetSvc.IsValid) NetSvc.Instance.Release();
        if (ConfigService.IsValid) ConfigService.Instance.Release();
        if (BattleSystem.IsValid) BattleSystem.Instance.Release();
        if (LobbySystem.IsValid) LobbySystem.Instance.Release();
        if (LoginSystem.IsValid) LoginSystem.Instance.Release();
        SingletonSystem.Release();
        Log.Warning("======= Release GameApp =======");
    }
}