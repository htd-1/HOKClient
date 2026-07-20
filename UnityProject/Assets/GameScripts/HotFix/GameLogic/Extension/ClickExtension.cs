using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic
{
    /// <summary>
    /// UGUI 输入事件扩展：为 GameObject 注册 PointerClick/Down/Up/Drag 回调。
    /// 基于 EventTrigger 封装，带参回调签名对齐原版 PEListener（携带 PointerEventData 与自定义 args）。
    /// </summary>
    public static class ClickExtension
    {
        // ===== 无参版本（简单点击场景，向后兼容）=====

        public static void AddClick(this GameObject go, Action callBack)
            => AddEntry(go, EventTriggerType.PointerClick, _ => callBack());

        // ===== 带参版本（携带 PointerEventData + 自定义参数）=====

        public static void AddClick(this GameObject go, Action<PointerEventData, object[]> cb, params object[] args)
            => AddEntry(go, EventTriggerType.PointerClick, ed => cb?.Invoke(ed, args));

        public static void AddClickDown(this GameObject go, Action<PointerEventData, object[]> cb, params object[] args)
            => AddEntry(go, EventTriggerType.PointerDown, ed => cb?.Invoke(ed, args));

        public static void AddClickUp(this GameObject go, Action<PointerEventData, object[]> cb, params object[] args)
            => AddEntry(go, EventTriggerType.PointerUp, ed => cb?.Invoke(ed, args));

        public static void AddDrag(this GameObject go, Action<PointerEventData, object[]> cb, params object[] args)
            => AddEntry(go, EventTriggerType.Drag, ed => cb?.Invoke(ed, args));

        // ===== 核心：注册一条 EventTrigger.Entry =====

        private static void AddEntry(GameObject go, EventTriggerType type, Action<PointerEventData> onEvent)
        {
            if (go == null)
            {
                return;
            }

            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry
            {
                eventID = type,
            };
            entry.callback.AddListener(ed => onEvent?.Invoke(ed as PointerEventData));
            trigger.triggers.Add(entry);
        }
    }
}
