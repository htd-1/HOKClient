using GameConfig.hok;
namespace GameLogic
{
    /// <summary>
    /// 英雄单位
    /// </summary>
    public class Hero:MainLogicUnit
    {
        public int HeroID;
        public int PosIndex;
        public string UserName;
        public Hero(HeroData ud) : base(ud)
        {
           HeroID = ud.HeroID;
           PosIndex = ud.PosIndex;
           UserName = ud.UserName;
           
           UnitType=UnitType.Hero;
           
           UnitName=ud.UnitCfg.UnitName+"_"+UserName;
        }

        public override void LogicInit()
        {
            base.LogicInit();
        }

        public override void LogicTick()
        {
            base.LogicTick();
        }

        public override void LogicUnInit()
        {
            base.LogicUnInit();
        }

        #region API Functions

        public override bool IsPlayerSelf()
        {
            return PosIndex == BattleSystem.Instance.BattleSelfIndex;
        }

        #endregion
    }
}