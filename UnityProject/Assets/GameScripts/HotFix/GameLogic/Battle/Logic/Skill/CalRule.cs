using System.Collections.Generic;
using GameConfig.hok;
using PEMath;
using TEngine;

namespace GameLogic
{
/// <summary>
/// 计算规则
/// </summary>
    public static class CalRule
    {
        private static Hero[] _blueTeamHero;
        private static Hero[] _redTeamHero;
        private static Hero[] _blueTeamTower;
        private static Hero[] _redTeamTower;
        private static List<Soldier> _blueTeamSoldier=new List<Soldier>();
        private static List<Soldier> _redTeamSoldier=new List<Soldier>();

        public static Hero[] BlueTeamHero
        {
            get { return _blueTeamHero; }
            set { _blueTeamHero = value; }
        }

        public static Hero[] RedTeamHero
        {
            get { return _redTeamHero; }
            set { _redTeamHero = value; }
        }

        public static Hero[] BlueTeamTower
        {
            get { return _blueTeamTower; }
            set { _blueTeamTower = value; }
        }

        public static Hero[] RedTeamTower
        {
            get { return _redTeamTower; }
            set { _redTeamTower = value; }
        }

        public static List<Soldier> BlueTeamSoldier
        {
            get { return _blueTeamSoldier; }
            set { _blueTeamSoldier = value; }
        }

        public static List<Soldier> RedTeamSoldier
        {
            get { return _redTeamSoldier; }
            set { _redTeamSoldier = value; }
        }

        public static MainLogicUnit FindSingleTargetByRule(MainLogicUnit self,TargetCfg cfg,PEVector3 pos)
        {
            List<MainLogicUnit> searchTeam = GetTargetTeam(self,cfg);

            switch (cfg.SelectRule)
            {
                case SelectRule.MinHPValue:
                    //todo
                    break;
                case SelectRule.MinHPPercent:
                    //todo
                    break;
                case SelectRule.TargetClosestSingle:
                    return FindMinDisTargetInTeam(self, searchTeam,(PEInt)cfg.SelectRange);
                case SelectRule.PositionClosestSingle:
                    //todo
                    break;
                default:
                    Log.Warning("SelectRule Not Implemented.");
                    break;
            }
            return null;
        }

        private static MainLogicUnit FindMinDisTargetInTeam(MainLogicUnit self, List<MainLogicUnit> targetTeam, PEInt range)
        {
            if (targetTeam == null)
            {
                return null;
            }

            MainLogicUnit target = null;
            int count=targetTeam.Count;
            PEVector3 selfPos = self.LogicPos;
            PEInt len = 0;
            for (int i = 0; i < count; i++)
            {
                PEInt sumRaius=targetTeam[i].UnitData.UnitCfg.ColliderCfg.mRadius+self.UnitData.UnitCfg.ColliderCfg.mRadius;
                PEInt tempLen=(targetTeam[i].LogicPos-selfPos).magnitude-sumRaius;
                if ((len == 0&&target==null)||tempLen<len)
                {
                    len=tempLen;
                    target = targetTeam[i];
                }
            }

            if (len < range) return target;
            return null;
        }
        private static List<MainLogicUnit> GetTargetTeam(MainLogicUnit self, TargetCfg cfg)
        {
            List<MainLogicUnit> targetList = new List<MainLogicUnit>();
            // 1. 确定 self 队伍合法性
            bool selfIsBlue = self.IsTeam(Team.Blue);
            if (!selfIsBlue && !self.IsTeam(Team.Red))
            {
                Log.Warning("Self Hero is Unknow.");
                return targetList;
            }
            // 2. Friend 取己方，Enemy 取对方；Dynamic/未知早退
            bool targetIsBlue;
            if (cfg.TargetTeam == TargetTeam.Friend)
            {
                targetIsBlue = selfIsBlue;
            }
            else if (cfg.TargetTeam == TargetTeam.Enemy)
            {
                targetIsBlue = !selfIsBlue;
            }
            else
            {
                Log.Warning("targetTeam is Unknow.");
                return targetList;
            }
            // 3. 按配置的单位类型，把对应队伍的列表加进来
            if (ContainTargetType(cfg, UnitType.Hero))
            {
                targetList.AddRange(targetIsBlue ? BlueTeamHero : RedTeamHero);
            }
            if (ContainTargetType(cfg, UnitType.Tower))
            {
                // targetList.AddRange(targetIsBlue ? BlueTeamTower : RedTeamTower);
            }
            if (ContainTargetType(cfg, UnitType.Soldier))
            {
                // targetList.AddRange(targetIsBlue ? BlueTeamSoldier : RedTeamSoldier);
            }
            // 4. 倒序删除死亡单位（起点必须 Count - 1，用 Count 会越界）
            for (int i = targetList.Count - 1; i >= 0; i--)
            {
                if (targetList[i].UnitState == UnitState.Dead)
                {
                    targetList.RemoveAt(i);
                }
            }
            return targetList;
        }

        private static bool ContainTargetType(TargetCfg cfg, UnitType target)
        {
            for (int i = 0; i < cfg.TargetTypeArr.Count; i++)
            {
                if (cfg.TargetTypeArr[i] == target)
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}