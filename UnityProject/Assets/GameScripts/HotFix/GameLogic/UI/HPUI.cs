using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig.hok;
using UnityEngine;
using TEngine;

namespace GameLogic
{
    [Window(UILayer.UI, location : "HPUI")]
    public partial class HPUI
    {
        public int JumpNumCount = 50;

        // unitHash → 血条 widget
        private readonly Dictionary<MainLogicUnit, HPItemWidget> _items = new();

        private JumpNumPool _jumpNumPool;
        protected override void OnCreate()
        {
            base.OnCreate();
            AddUIEvent<MainLogicUnit, Transform>(IBattleHPUI_Event.AddHPItemInfo, OnAddHPItem);
            AddUIEvent<MainLogicUnit,int,JumpUpdateInfo>(IBattleHPUI_Event.HPValChange,UpdateHP);
            AddUIEvent<MainLogicUnit,StateEnum,bool>(IBattleHPUI_Event.SetStateInfo,SetStateInfo);
            AddUIEvent<JumpUpdateInfo>(IBattleHPUI_Event.UpdateJumpInfo,UpdateJumpInfo);
            // Log.Warning("HPUI OnCreate");
            _jumpNumPool=new JumpNumPool(JumpNumCount,m_rect_JumpNumRoot);
        }

        /// <summary>
        /// 注册一个单位的血条：按 unitType 选对应 prefab（ItemHPHero/Soldier/Tower）
        /// → CreateWidget 挂到 m_rect_ItemRoot → InitItem。数据由调用方提供，HPUI 不碰逻辑层内部。
        /// 幂等：同 unitHash 已存在直接返回；异步加载完成再做二次校验（解并发双触发）。
        /// </summary>
        /// <summary>
        /// AddHPItemInfo 事件入口。isFriend 由发送方用 FightMgr.IsFriend 算好随事件传入
        /// （HPUI 拿不到 FightMgr，不在此判断队伍）。
        /// </summary>
        private async void OnAddHPItem(MainLogicUnit unit, Transform hpRoot)
        {
            int maxHp = unit.UnitData.UnitCfg.Hp;   // 最大血量
            await RegisterHPItemAsync(unit, hpRoot, maxHp, unit.IsFriend);   // isFriend 固化在 unit（FightMgr 设），HPUI 不碰 FightMgr
        }

        public async UniTask RegisterHPItemAsync(MainLogicUnit unit, Transform hpRoot, int maxHp, bool isFriend)
        {
            if (_items.ContainsKey(unit)) return;            // 幂等：已注册
            if (unit == null || hpRoot == null) return;

            // TEngine 标准：按单位类型加载对应血条 prefab 并挂到容器下
            HPItemWidget widget = await CreateWidgetByPathAsync<HPItemWidget>(m_rect_ItemRoot, GetHpPrefab(unit.UnitType));
            if (widget == null)
            {
                Log.Error($"[HPUI] CreateWidget failed, unit={unit} type={unit.UnitType}");
                return;
            }

            // 异步期间可能已被另一路触发注册，二次校验防重复
            if (_items.ContainsKey(unit))
            {
                widget.Destroy();
                return;
            }

            widget.InitItem(unit, hpRoot, maxHp, unit.UnitType, isFriend);
            _items[unit] = widget;
        }

        // 血条 prefab 按 unitType 选。
        // AssetBundleCollectorSetting：UI/Prefabs 组 AddressByFileName，地址 = 文件名（不带 .prefab、不带路径）。
        private static string GetHpPrefab(UnitType t) => t switch
        {
            UnitType.Hero    => "ItemHPHero",
            UnitType.Soldier => "ItemHPSoldier",
            UnitType.Tower   => "ItemHPTower",
            _                => "ItemHPSoldier",
        };

        private void UpdateJumpInfo(JumpUpdateInfo jui)
        {
            if (jui != null)
            {
                JumpNum jn = _jumpNumPool.PopOne();
                if (jn != null)
                {
                    jn.Show(jui);
                }
            }
        }
        /// <summary>刷新血量（调用方驱动，替代无效的 IBattleHPUI）。</summary>
        private void UpdateHP(MainLogicUnit unit, int hp,JumpUpdateInfo jui)
        {
            if (_items.TryGetValue(unit, out HPItemWidget w)) w.UpdateHPPrg(hp);

            if (jui != null)
            {
                JumpNum jn = _jumpNumPool.PopOne();
                if (jn != null)
                {
                    jn.Show(jui);
                }
            }
        }

        /// <summary>状态图标（眩晕/沉默/击飞）。</summary>
        public void SetState(MainLogicUnit unit, StateEnum state, bool show)
        {
            if (_items.TryGetValue(unit, out HPItemWidget w)) w.SetStateInfo(state, show);
        }

        /// <summary>移除并销毁血条（单位死亡/退场）。</summary>
        public void RemoveHPItem(MainLogicUnit unit)
        {
            if (_items.TryGetValue(unit, out HPItemWidget w))
            {
                w.Destroy();
                _items.Remove(unit);
            }
        }

        private void SetStateInfo(MainLogicUnit unit,StateEnum state,bool show=true)
        {
            if (_items.TryGetValue(unit, out HPItemWidget w))
            {
                w.SetStateInfo(state, show);
            }
        }
        // 每帧驱动血条世界→屏幕跟随（高频，不走事件）
        protected override void OnUpdate()
        {
            if (_items.Count == 0) return;
            foreach (var kv in _items)
            {
                if (kv.Value != null) kv.Value.Follow();
            }
        }

        protected override void OnDestroy()
        {
            foreach (var kv in _items)
            {
                if (kv.Value != null) kv.Value.Destroy();
            }
            _items.Clear();
            base.OnDestroy();
        }
    }
}
