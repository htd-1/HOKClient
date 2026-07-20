using System;
using HOKProtocol;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>
    /// 选英雄列表项（UIWidget，逻辑分部）。
    /// <para>迁自原版 SelectWnd 中内联的英雄项逻辑：头像 imgIcon / 选中高亮 state / 名字 txtName + 整项点击选择。</para>
    /// <para>由 <see cref="SelectUI"/> 经 CreateWidget&lt;ItemHero&gt;(go) 创建，<see cref="Setup"/> 注入数据与选中回调。</para>
    /// </summary>
    public partial class ItemHero
    {
        private int _heroId;
        private string _resName;
        private Action<int, string> _onSelected;
        private Button _btn;

        /// <summary>该项英雄 ID（供 SelectUI 比对当前选中态）。</summary>
        public int HeroId => _heroId;

        protected override void OnCreate()
        {
            base.OnCreate();
            // 整项可点击：Button 挂在根或子节点（prefab 约定），此处兜底获取
            _btn = gameObject.GetComponentInChildren<Button>();
        }

        /// <summary>
        /// 初始化数据与点击回调。
        /// <para>头像/名字经 GameServices.Config 按 heroID 查（resName + "_head" / GetHeroName）。</para>
        /// </summary>
        public void Setup(HeroSelectData data, Action<int, string> onSelected)
        {
            _heroId = data.heroID;
            _resName = GameServices.Config.GetHeroResName(_heroId);
            _onSelected = onSelected;

            // 原版 SetSprite(imgIcon, ResName_head) / SetText(txtName, unitName)
            m_img_icon.SetSprite(_resName + "_head", setNativeSize: true);
            m_tmp_name.text = GameServices.Config.GetHeroName(_heroId);

            SetSelected(false);

            if (_btn != null)
            {
                _btn.onClick.AddListener(OnClick);
            }
            else
            {
                Log.Warning($"[ItemHero] heroID={_heroId} 未找到 Button，点击选择不可用，请检查 prefab。");
            }
        }

        /// <summary>选中态高亮切换（原版 selectGlow / frame_normal）。</summary>
        public void SetSelected(bool selected)
        {
            m_img_state.SetSprite(selected ? "selectGlow" : "frame_normal", setNativeSize: false);
        }

        private void OnClick()
        {
            _onSelected?.Invoke(_heroId, _resName);
        }

        protected override void OnDestroy()
        {
            if (_btn != null)
            {
                _btn.onClick.RemoveListener(OnClick);
            }
            _onSelected = null;
            base.OnDestroy();
        }
    }
}
