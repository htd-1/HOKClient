using GameConfig.hok;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 英雄表现控制
    /// </summary>
    public class HeroView:MainViewUnit
    {
        public Transform Sk1;
        public Transform Sk2;
        public Transform Sk3;
        private Hero _hero;
        
        public override void Init(LogicUnit logicUnit)
        {
            base.Init(logicUnit);
            _hero = logicUnit as Hero;
            
            SkillRange.gameObject.SetActive(false);

            if(Sk1!=null)Sk1?.gameObject.SetActive(false);
            if(Sk2!=null)Sk2?.gameObject.SetActive(false);
            if(Sk3!=null)Sk3?.gameObject.SetActive(false);
        }

        protected override Vector3 GetUnitViewDir()
        {
            //玩家朝向
            return _hero.InputDir.ConvertViewVector3();
        }

        public void DisableSkillGuide(int skillIndex)
        {
            switch (skillIndex)
            {
                case 1:
                    if(Sk1!=null)Sk1?.gameObject.SetActive(false);
                    break;
                case 2:
                    if(Sk2!=null)Sk2?.gameObject.SetActive(false);
                    break;
                case 3:
                    if(Sk3!=null)Sk3?.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
        public void SetSkillGuide(int skillIndex,bool state,ReleaseMode mode,Vector3 vec)
        {
            switch (skillIndex)
            {
               case 1:
                   Sk1.gameObject.SetActive(state);
                   if (state)
                   {
                       UpdateSkillGuide(Sk1,mode,vec);
                   }
                   break;
               case 2:
                   Sk2.gameObject.SetActive(state);
                   if (state)
                   {
                       UpdateSkillGuide(Sk2,mode,vec);
                   }
                   break;
               case 3:
                   Sk3.gameObject.SetActive(state);
                   if (state)
                   {
                       UpdateSkillGuide(Sk3,mode,vec);
                   }
                   break;
               default:
                   break;
            }
        }

        private void UpdateSkillGuide(Transform sk,ReleaseMode mode,Vector3 vec)
        {
            if (mode == ReleaseMode.Postion)
            {
                sk.localPosition = vec;
            }
            else
            {
                float angle=Vector2.SignedAngle(new Vector2(vec.x,vec.z),new Vector2(0,1));
                sk.localEulerAngles=new Vector3(0,angle,0);
            }
        }
    }
}