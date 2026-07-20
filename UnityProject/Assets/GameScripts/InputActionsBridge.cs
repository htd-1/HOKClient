using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 主包侧输入桥接器(非热更,与 <see cref="InputSystem_Actions"/> 同属 Assembly-CSharp)。
    /// 强类型封装 InputSystem_Actions,把深层 struct 属性链(.Player.Move.ReadValue)拍平成无参标量方法,
    /// 供热更侧 <c>BattleInputService</c> 反射调用。
    /// </summary>
    /// <remarks>
    /// 设计原因:热更(GameLogic)编译时不能引用主包(Assembly-CSharp)的 InputSystem_Actions,
    /// 必须走反射跨边界;但热更大量反射有性能/AOT 风险,故在主包侧强类型拍平,
    /// 热更侧只需 Invoke 扁平 API(无参 + 标量返回),反射成本最小。
    /// </remarks>
    public class InputActionsBridge
    {
        private readonly InputSystem_Actions _actions;

        public InputActionsBridge()
        {
            _actions = new InputSystem_Actions();
        }

        /// <summary>启用 Player action map(WASD/Look/Attack 等)。UI map 留给 EventSystem,不在此启用。</summary>
        public void EnablePlayer() => _actions.Player.Enable();

        /// <summary>停用 Player action map(输入锁/眩晕)。UI map 不受影响,按钮仍可点。</summary>
        public void DisablePlayer() => _actions.Player.Disable();

        /// <summary>销毁底层 InputActionAsset。战斗退场调。</summary>
        public void Dispose()
        {
            _actions.Disable();
            _actions.Dispose();
        }

        // —— 扁平采样 API(反射友好:无参、标量返回)——

        /// <summary>WASD / 左摇杆,返回归一化方向。</summary>
        public Vector2 GetMove() => _actions.Player.Move.ReadValue<Vector2>();

        /// <summary>鼠标 delta / 右摇杆(视角/朝向)。</summary>
        public Vector2 GetLook() => _actions.Player.Look.ReadValue<Vector2>();

        /// <summary>左键 / 手柄 Attack 本帧按下(边沿触发)。</summary>
        public bool AttackPressedThisFrame() => _actions.Player.Attack.WasPressedThisFrame();

        /// <summary>Sprint 当前是否按住(持续型)。</summary>
        public bool SprintIsPressed() => _actions.Player.Sprint.IsPressed();

        /// <summary>Jump 本帧按下(边沿触发)。</summary>
        public bool JumpPressedThisFrame() => _actions.Player.Jump.WasPressedThisFrame();
    }
}
