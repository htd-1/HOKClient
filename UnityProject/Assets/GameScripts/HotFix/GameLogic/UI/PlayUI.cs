using System.Collections.Generic;
using System.Text;
using HOKProtocol;
using PEMath;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{

    [Window(UILayer.UI, location : "PlayUI")]
    public partial class PlayUI
    {
       private Vector2 _lastKeyDir;
       private Vector2 _lastStickDir=Vector2.zero;
       private Vector2 _startPos = Vector2.zero;
       private Vector2 _defaultPos = Vector2.zero;
       private float _pointDis = 135;

       // 推送式：缓存战斗域状态（技能图标初始化用 BattleHeroList/SelfIndex）。HP/技能倒计时等表现层接缝（IBattlePlayUI/IBattleHPUI）不变。
       private BattleState _battleState;
       private GameEventMgr _dataMgr;
       private bool _skillInited;

        #region 事件

        private partial void OnClick_chatBtn()
        {

        }
        #endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            m_img_arrow.gameObject.SetActive(false);
            _pointDis = Screen.height * 1.0f / ClientConfig.ScreenStandardHeight * GameServices.Config.ClientSetting.OpDis;

            _defaultPos = m_img_dirBg.transform.position;

            // 推送式：订阅战斗域状态 + 拉首次快照（RequestSnapshot 同步触发 Changed → OnBattleDataChanged → InitSkillInfo）
            _dataMgr = new GameEventMgr();
            _dataMgr.AddEvent<BattleState>(IBattleData_Event.Changed, OnBattleDataChanged);

            AddUIEvent<bool>(IBattlePlayUI_Event.OnSkillCancel,OnSkillCancel);
            AddUIEvent<int, int>(IBattlePlayUI_Event.OnSkillEnterCD, OnSkillEnterCD);
            RegisterMoveEvents();

            GameEvent.Get<IBattleCmd>().RequestSnapshot();
        }

        protected override void OnDestroy()
        {
            _dataMgr?.Clear();
            _dataMgr = null;
            base.OnDestroy();
        }

        private void OnBattleDataChanged(BattleState state)
        {
            _battleState = state;
            // 技能图标依赖启动数据（BattleHeroList/SelfIndex），首次就绪时初始化一次（防重复创建 SkillItem）
            if (!_skillInited && _battleState.BattleHeroList != null && _battleState.BattleSelfIndex >= 0)
            {
                InitSkillInfo();
                _skillInited = true;
            }
        }

        private void OnSkillCancel(bool state)
        {
            m_img_cancelSkill.gameObject.SetActive(state);
        }
        private void RegisterMoveEvents()
        {
            m_img_arrow.gameObject.SetActive(false);

            m_img_touch.gameObject.AddClickDown((PointerEventData eventData, object[] go) =>
            {
                _startPos=eventData.position;
                m_img_dirPoint.color=new Color(1, 1, 1, 1);
                m_img_dirBg.transform.position=eventData.position;
            });
            m_img_touch.gameObject.AddClickUp( (PointerEventData evt, object[] args) => {
                m_img_dirBg.transform.position = _defaultPos;
                m_img_dirPoint.color = new Color(1, 1, 1, 0.5f);
                m_img_dirPoint.transform.localPosition = Vector2.zero;
                m_img_arrow.gameObject.SetActive(false);

                InputMoveKey(Vector2.zero);
            });
            m_img_touch.gameObject.AddDrag( (PointerEventData evt, object[] args) => {
                Vector2 dir = evt.position - _startPos;
                float len = dir.magnitude;
                if(len > _pointDis) {
                    Vector2 clampDir = Vector2.ClampMagnitude(dir, _pointDis);
                    m_img_dirPoint.transform.position = _startPos + clampDir;
                }
                else {
                    m_img_dirPoint.transform.position = evt.position;
                }

                if(dir != Vector2.zero) {
                    m_img_arrow.gameObject.SetActive(true);

                    float angle = Vector2.SignedAngle(new Vector2(1, 0), dir);
                    m_rect_arrows.localEulerAngles = new Vector3(0, 0, angle);
                }

                InputMoveKey(dir.normalized);
            });


        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            float h=Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector2 keyDir=new Vector2(h,v);
            if (keyDir != _lastKeyDir)
            {
                if (h != 0 || v != 0)
                {
                    keyDir=keyDir.normalized;
                }
                InputMoveKey(keyDir);
                _lastKeyDir=keyDir;
            }
        }


        private void InputMoveKey(Vector2 dir)
        {
            if (!dir.Equals(_lastStickDir))
            {
                Vector3 dirVec3 = new Vector3(dir.x, 0, dir.y);

                dirVec3 = Quaternion.Euler(0, 45, 0) * dirVec3;

                PEVector3 logicDir = PEVector3.zero;
                if (dir!=Vector2.zero)
                {
                    logicDir.x = (PEInt)dirVec3.x;
                    logicDir.y = (PEInt)dirVec3.y;
                    logicDir.z = (PEInt)dirVec3.z;
                }
                bool isSend=BattleInputSvc.Instance.SendMoveKey(logicDir);
                if(isSend)_lastStickDir=dir;
            }
        }
    }
}
