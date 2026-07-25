using HOKProtocol;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// ②事件（下行·纯通知·L3 → L1/L2）。登录结果通知。
    /// <para>数据本身走 <see cref="IPlayerData"/>（推送式），本接口只携带"发生了什么"的轻量信号，
    /// 供 Procedure 据 FSM 驱动状态流转。</para>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IPlayerEvent
    {
        /// <summary>登录响应已处理（UserData 已写入 SessionState 并经 IPlayerEvent 推送）。</summary>
        /// <param name="success">是否登录成功。</param>
        /// <param name="message">提示信息。</param>
        void LoginResult(bool success, string message);

        /// <summary>UserData 变更推送（含 UI 打开时 RequestSnapshot 触发的首次快照）。</summary>
        void Changed(UserData data);
    }
}
