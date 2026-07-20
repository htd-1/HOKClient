using System;
using System.Collections.Generic;
using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 网络消息路由薄封装。
    /// <para>以 TEngine <see cref="GameEvent"/>（string-keyed）为路由器，key = "NetMsg." + cmd，
    /// 经 <c>RuntimeId.ToRuntimeId</c> 映射为无撞车自增 ID——避免与 <c>(int)cmd</c> 整数值、
    /// 接口事件 ID 冲突（RuntimeId 是自增计数器，CMD 枚举值与之重叠）。</para>
    /// <para><c>_known</c> 仅是 Set（回答"有没有人订阅"），不做路由——真正路由归 GameEvent 内部表。
    /// 存在的唯一目的：未注册 CMD 的运行时 Warning log（GameEvent.Send 在无人监听时静默 return）。</para>
    /// </summary>
    public static class NetMsg
    {
        private const string Prefix = "NetMsg.";

        private static readonly HashSet<CMD> _known = new HashSet<CMD>();

        /// <summary>业务点注册某 CMD 的监听者。底层挂到 GameEvent（string-keyed）。</summary>
        public static void Subscribe(CMD cmd, Action<HOKMsg> handler)
        {
            GameEvent.AddEventListener<HOKMsg>(Prefix + cmd, handler);
            _known.Add(cmd);
        }

        /// <summary>NetSvc Pump 分发收到的消息。错误消息（msg.error）已在 NetSvc 拦截，不走到这里。</summary>
        public static void Route(HOKMsg msg)
        {
            GameEvent.Send<HOKMsg>(Prefix + msg.cmd, msg);
            if (!_known.Contains(msg.cmd))
            {
                Log.Warning($"[NetMsg] unhandled CMD: {msg.cmd}");
            }
        }
    }
}
