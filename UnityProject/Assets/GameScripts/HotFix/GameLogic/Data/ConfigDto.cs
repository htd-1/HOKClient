
using PEMath;
using PEPhysx;

namespace GameLogic
{
    /// <summary>ConfigService 对外业务 DTO:单位配置(Luban Unit record 定点数转换结果)。</summary>
    public class UnitCfg
    {
        public int UnitID;
        public string UnitName;
        public string ResName;
        public PEInt HitHeight;
        public int Hp;
        public int Def;
        public int MoveSpeed;
        public ColliderConfig ColliderCfg;
        public int[] PasvBuff;
        public int[] SkillArr;
    }

    /// <summary>ConfigService 对外业务 DTO:地图配置(Luban Map record 定点数转换结果)。</summary>
    public class MapCfg
    {
        public int MapID;
        public PEVector3 BlueBorn;
        public PEVector3 RedBorn;
        public int[] TowerIDArr;
        public PEVector3[] TowerPosArr;
        public int BornDelay;
        public int BornInterval;
        public int WaveInterval;
        public int[] BlueSoldierIDArr;
        public PEVector3[] BlueSoldierPosArr;
        public int[] RedSoldierIDArr;
        public PEVector3[] RedSoldierPosArr;
    }

    public class LogicUnitData
    {
        public Team Team;
        public PEVector3 BornPos;
        public UnitCfg UnitCfg;
    }
    
    public class HeroData :LogicUnitData
    {
        public int HeroID;
        public int PosIndex;
        public string UserName;
        
    }
}
