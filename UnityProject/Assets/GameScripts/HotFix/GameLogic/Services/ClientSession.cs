using System;
using HOKProtocol;
using PENet;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 客户端 KCP 会话。仅做收包入队与连接回调转发，状态/分发逻辑全部交给 <see cref="NetSvc"/>。
    /// </summary>
    public class ClientSession : KCPSession<HOKMsg>
    {
        protected override void OnConnected()
        {
            // 连接成功的状态转换由 NetSvc.CheckConnection（checkTask.Result）统一处理，此处仅记录。
            Log.Info("[ClientSession] KCP connected.");
        }

        protected override void OnDisConnected()
        {
            // 服务器/网络侧主动断开，转发给 NetSvc。
            NetSvc.Instance.HandleSessionDisconnected();
        }

        protected override void OnReciveMsg(HOKMsg msg)
        {
            NetSvc.Instance.AddMsgQue(msg);
        }

        protected override void OnUpdate(DateTime now)
        {
        }
    }
}
