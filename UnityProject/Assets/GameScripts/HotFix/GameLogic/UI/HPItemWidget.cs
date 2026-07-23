using GameConfig.hok;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using TMPro;

namespace GameLogic
{
   


    /// <summary>
    /// 扁平化合并到本 UIWidget：基类公共 + 三子类 override 内容，按 unitType 分支。
    /// </summary>
    public class HPItemWidget : UIWidget
    {
        // --- 原版字段（基类 + 三子类合并）---
        private Image _imgPrg;          // 血量填充
        private Image _iconState;       // 兵：状态小图标
        private Image _imgState;        // 英雄：状态大图（盖名字）
        private TextMeshProUGUI _txtName;          // 英雄：名字
        private Transform _hpMarkRoot;  // 英雄：血量标记段
        private int _markCount;

        protected bool IsFriend;
        private Transform _rootTrans;
        private int _hpVal;
        private float _designHeight;    // UIRoot CanvasScaler 


        // 节点绑定
        protected override void ScriptGenerator()
        {
            
            _imgPrg     = FindChildComponent<Image>("imgHP");
            _iconState  = FindChildComponent<Image>("iconState");
            _imgState   = FindChildComponent<Image>("imgState");
            _txtName    = FindChildComponent<TextMeshProUGUI>("txtName");
            _hpMarkRoot = FindChild("HPmarkRoot");


            var scaler = gameObject.GetComponentInParent<CanvasScaler>();
            _designHeight = scaler != null ? scaler.referenceResolution.y : Screen.height;
        }

        /// <summary>
        /// 根据Unit类型进行不同的初始化
        /// </summary>
        public void InitItem(MainLogicUnit unit, Transform root, int hp, UnitType unitType, bool friend, int markCount = 1000)
        {
            // --- base.InitItem ---
            IsFriend = friend;
            _imgPrg.fillAmount = 1;
            _rootTrans = root;
            _hpVal = hp;
            this._markCount = markCount;

            // --- Soldier / Hero（共用 teamhpfg + 关 iconState）---
            if (unitType == UnitType.Hero || unitType == UnitType.Soldier)
            {
                if (_iconState != null) _iconState.gameObject.SetActive(false);
                _imgPrg.SetSprite(IsFriend ? "selfteamhpfg" : "enemyteamhpfg");
            }
            // --- Tower ---
            else if (unitType == UnitType.Tower)
            {
                _imgPrg.SetSprite(IsFriend ? "selftowerhpfg" : "enemytowerhpfg");
            }

            // --- Hero 在 Soldier 之上追加 ---
            if (unitType == UnitType.Hero)
            {
                if (_imgState != null) _imgState.gameObject.SetActive(false);
                if (_txtName != null)
                {
                    _txtName.text = unit.UnitName;
                    _txtName.gameObject.SetActive(true);
                }
                SetHPMark(hp);
            }
        }

        
        public void UpdateHPPrg(int newVal)
        {
            gameObject.SetActive(newVal != 0);
            _imgPrg.fillAmount = newVal * 1.0f / _hpVal;
        }

        
        private void SetHPMark(int hp)
        {
            if (_hpMarkRoot == null) return;
            int count = _markCount > 0 ? hp / _markCount : 0;
            for (int i = 0; i < _hpMarkRoot.childCount; i++)
                _hpMarkRoot.GetChild(i).gameObject.SetActive(i < count);
        }

       
        public void SetStateInfo(StateEnum state, bool show)
        {
            if (!show)
            {
                if (_iconState != null) _iconState.gameObject.SetActive(false);
                if (_imgState != null) _imgState.gameObject.SetActive(false);
                if (_txtName != null) _txtName.gameObject.SetActive(true);   // 不显示状态时露名字
                return;
            }

            // Soldier：iconState
            if (_iconState != null)
            {
                switch (state)
                {
                    case StateEnum.Silenced: _iconState.SetSprite("silenceIcon"); break;
                    case StateEnum.Knockup:
                    case StateEnum.Stunned:  _iconState.SetSprite("stunIcon");    break;
                }
                _iconState.gameObject.SetActive(true);
                _iconState.SetNativeSize();
            }

            // Hero：imgState 
            if (_imgState != null)
            {
                switch (state)
                {
                    case StateEnum.Silenced: _imgState.SetSprite("silenceState"); break;
                    case StateEnum.Knockup:  _imgState.SetSprite("knockState");   break;
                    case StateEnum.Stunned:  _imgState.SetSprite("stunState");    break;
                }
                if (_txtName != null) _txtName.gameObject.SetActive(false);
                _imgState.gameObject.SetActive(true);
                _imgState.SetNativeSize();
            }
        }

        
        public void Follow()
        {
            if (_rootTrans == null) return;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(_rootTrans.position);
            float scaleRate = _designHeight / Screen.height;   
            rectTransform.anchoredPosition = screenPos * scaleRate;
        }
        
    }
}
