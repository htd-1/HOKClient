using System;
using PEMath;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 主要表现控制
    /// 攻速/移速动画变化/动画播放
    /// 血条信息显示
    /// 小地图显示
    /// </summary>
    public abstract class MainViewUnit:ViewUnit
    {
        public float Fade;
        public Animation Ani;
        public Transform SkillRange;

        public Transform HpRoot;
        
        private float _aniMoveSpeedBase;
        private float _aniAttackSpeedBase;
        private MainLogicUnit _mainLogicUnit;
        

        private bool atkcount =false;
        public override void Init(LogicUnit logicUnit)
        {
            base.Init(logicUnit);
            _mainLogicUnit = logicUnit as MainLogicUnit;
            
            //移速
            _aniMoveSpeedBase = _mainLogicUnit.LogicMoveSpeed.RawFloat;
            _aniAttackSpeedBase=_mainLogicUnit.AttackSpeedRate.RawFloat;


            _mainLogicUnit.OnHpChange += UpdateHP;
            //血条显示：不在此发 AddHPItemInfo（LogicInit 跑在 FightMgr 设 IsFriend 之前，此时 unit.IsFriend 还是 false）
            // Log.Info("我已启动");
        }

        /// <summary>发 AddHPItemInfo 注册血条。由 FightMgr 在 IsFriend 固化后命令式调用，避免时序问题。</summary>
        public void SendAddHPItem()
        {
            GameEvent.Get<IBattleHPUI>().AddHPItemInfo(_mainLogicUnit, HpRoot);
        }

        protected override void Update()
        {
            if (_mainLogicUnit.IsDirChanged&&!_mainLogicUnit.IsSkillSpelling())
            {
                if (_mainLogicUnit.LogicDir.ConvertViewVector3()
                    .Equals(Vector3.zero))
                {
                    PlayAni("free");
                }
                else
                {
                    PlayAni("walk");
                }
            }
            
            base.Update();
        }

        private void OnDestroy()
        {
            _mainLogicUnit.OnHpChange -= UpdateHP;
        }

        public virtual void OnDeath(MainLogicUnit unit)
        {
            
        }

        public override void PlayAni(string aniName)
        {
            if (aniName.Equals("atk"))
            {
                aniName="atk"+(atkcount?2:1);
                atkcount = !atkcount;
            }
            if (aniName.Contains("walk"))
            {
                float moveRate=_mainLogicUnit.LogicMoveSpeed.RawFloat
                    /_aniMoveSpeedBase;
                Ani[aniName].speed = moveRate;
                Ani.CrossFade(aniName,Fade/moveRate);
            }
            else if (aniName.Contains("atk"))
            {
                if (Ani.IsPlaying(aniName))
                {
                    Ani.Stop(aniName);
                }

                float attackRate = _mainLogicUnit.AttackSpeedRate.RawFloat / _aniAttackSpeedBase;
                Ani[aniName].speed = attackRate;
                Ani.CrossFade(aniName,Fade/attackRate);
            }
            else
            {
                if(Ani==null)Log.Info("Ani is null");
                Ani.CrossFade(aniName,Fade);  
                //临时不播放防止莫名其妙的字符串进来报错
            }
        }

        public void UpdateHP(int hp, JumpUpdateInfo jui)
        {
            if(jui==null)return;
            float scaleRate = 1.0f * ClientConfig.ScreenStandardHeight / Screen.height;
            Vector3 screenPos =Camera.main.WorldToScreenPoint(transform.position+new Vector3(0,1,0));
            jui.Pos=screenPos*scaleRate;
            
            
            GameEvent.Get<IBattleHPUI>().HPValChange(_mainLogicUnit,hp,jui);
        }
        public void UpdateSkillRotation(PEVector3 skillRotation)
        {
            ViewTargetDir = skillRotation.ConvertViewVector3();
        }
        
        public void SetAtkSkillRange(bool state,float range=2.5f)
        {
            if (SkillRange != null)
            {
                range += _mainLogicUnit.UnitData.UnitCfg.
                    ColliderCfg.mRadius.RawFloat;
                SkillRange.localScale=new Vector3(range/2.5f,range/2.5f,1);
                SkillRange.gameObject.SetActive(state);
                
            }
        }
    }
}