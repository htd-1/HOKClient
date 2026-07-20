using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Logic → HPUI 事件接口（GroupUI）。
    /// 单位 HP 变化、死亡、受控状态等血条 UI 更新事件。
    /// </summary>
    [EventInterface(EEventGroup.GroupUI)]
    public interface IBattleHPUI
    {
        void AddHPItemInfo(MainLogicUnit mainLogicUnit, Transform parent);

        void HPValChange(MainLogicUnit mainLogicUnit, int hp,JumpUpdateInfo jumpUpdateInfo);
    }
}
