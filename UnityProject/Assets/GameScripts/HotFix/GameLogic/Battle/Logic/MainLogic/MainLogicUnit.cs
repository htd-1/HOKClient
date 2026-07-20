using GameConfig.hok;
using HOKProtocol;
using PEMath;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public enum UnitState
    {
        Alive,
        Dead
    }



    public enum Team
    {
        None,
        Blue,
        Red,
        Neutal
    }
    /// <summary>
    /// 主要逻辑单位
    /// </summary>
    public partial class MainLogicUnit:LogicUnit
    {
        public LogicUnitData UnitData;
        public UnitState UnitState;
        public UnitType UnitType;
        public MainViewUnit MainViewUnit;
        
        
        public MainLogicUnit(LogicUnitData ud)
        {
           UnitData = ud;
           UnitName = ud.UnitCfg.UnitName;
        }
        
        public override void LogicInit()
        {
            //初始化属性
            InitProperties();
            //初始化技能
            InitSkill();
            //初始化移动
            InitMove();

            GameObject go = GameModule.Resource.LoadGameObject(
                UnitData.UnitCfg
                .ResName);
            
            MainViewUnit=go.GetComponent<MainViewUnit>();
            
            if(MainViewUnit==null)Log.Error("MainLogicUnit not found");
            
            MainViewUnit.Init(this);
            
            UnitState = UnitState.Alive;
            
        }

        public override void LogicTick()
        {
            TickSkill();
            TickMove();
        }

        public override void LogicUnInit()
        {
            UnInitSkill();
            UnInitMove();
        }

        public void InputKey(OpKey key)
        {
            switch (key.keyType)
            {
                case KeyType.Skill:
                    InputSkillKey(key.skillKey);
                break;
                case KeyType.Move:
                    PEInt x = PEInt.zero,z= PEInt.zero;
                    x.ScaledValue = key.moveKey.x;
                    z.ScaledValue= key.moveKey.z;
                    InputMoveKey(new PEVector3(x,0,z));
                    break;
                case KeyType.None:
                    Log.Warning("the opkey type doesn't exist");
                    break;
            }
        }

        public void PlayAudio(string audioName, bool loop = false, int delay = 0)
        {
            MainViewUnit.PlayAudio(audioName, loop, delay);
        }

        public void PlayAni(string aniName)
        {
            MainViewUnit.PlayAni(aniName);
        }


        public virtual bool IsPlayerSelf()
        {
            return false;
        }
    }
}