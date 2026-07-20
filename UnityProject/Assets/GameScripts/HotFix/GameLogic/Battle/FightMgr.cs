using System.Collections.Generic;
using System.Diagnostics;

using HOKProtocol;
using PEMath;
using PEPhysx;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class FightMgr
    {
        private MapRoot _mapRoot;
        private EnvColliders _logicEnv;
        private Transform _transFollow;
        /// <summary>
        /// 英雄角色集合
        /// </summary>
        private List<Hero> _heroList;
        private int _selfIndex = -1;
        private Hero _selfHero;
        /// <summary>当前客户端操作英雄在 _heroList 中的下标(来自 NtfLoadRes.posIndex)。</summary>
        public int SelfIndex => _selfIndex;
        /// <summary>当前客户端操作英雄引用(Init 时按下标解析,供战斗逻辑层/表现层使用)。</summary>
        public Hero SelfHero => _selfHero;

        /// <summary>
        /// unit 是否本机友方（与本机英雄同队）。isFriend 判断集中在此唯一一处。
        /// HPUI 是 UI 层、拿不到 FightMgr，故由发送方（创建 View 处）算好后随 AddHPItemInfo 事件传入。
        /// </summary>
        public bool IsFriend(MainLogicUnit unit)
        {
            if (_selfHero == null || unit == null) return false;
            return unit.IsTeam(_selfHero.UnitData.Team);
        }
        public void Init(List<BattleHeroData> battleHeroList,MapCfg mapCfg,int selfIndex)
        {
            _heroList=new List<Hero>();
            //初始化

            // 注册 Buff 工厂,须在创建任何 MainLogicUnit(InitHero→LogicInit→技能/buff)之前
            BuffRegistry.Init();

            InitEnv();

            InitHero(battleHeroList,mapCfg);

            _selfIndex = selfIndex;
            if(_selfIndex >= 0 && _selfIndex < _heroList.Count)
            {
                _selfHero = _heroList[_selfIndex];
            }

            InitCamFollowTrans(selfIndex);

            // isFriend 必须在 OnSelfHeroLoaded 之前固化：OnSelfHeroLoaded 会触发 View 创建/Init，
            // MainViewUnit.Init 随即发 AddHPItemInfo(unit, hpRoot)，HPUI 读 unit.IsFriend——晚一步就读到默认 false（全红）。
            // Log.Info($"[FightMgr] selfIndex={_selfIndex} selfHero={_selfHero?.UnitName} team={_selfHero?.UnitData.Team}");
            if (_selfHero != null)
            {
                for (int i = 0; i < _heroList.Count; i++)
                {
                    _heroList[i].IsFriend = IsFriend(_heroList[i]);
                    // Log.Info($"[FightMgr] hero={_heroList[i].UnitName} team={_heroList[i].UnitData.Team} isFriend={_heroList[i].IsFriend}");
                }
            }
            else
            {
                Log.Warning("[FightMgr] _selfHero 为 null，所有 unit.isFriend 将为 false（血条全红）。检查 selfIndex/BattleSelfIndex 是否有效。");
            }

            // IsFriend 已固化，命令式触发各 hero 注册血条。
            // MainViewUnit.Init 不再自发：LogicInit 在 FightMgr.InitHero 里跑，早于此处设 IsFriend，自发会让 HPUI 读到 false。
            foreach (var hero in _heroList)
            {
                hero.MainViewUnit?.SendAddHPItem();
            }

            // 英雄全部加载完成,通知 UI 层
            GameEvent.Get<IBattleEvent>().OnSelfHeroLoaded(_selfHero);
        }

        private void InitHero(List<BattleHeroData> battleHeroList,MapCfg mapCfg)
        {
            int sep=battleHeroList.Count/2;

            Hero[] blueTeam = new Hero[sep];
            Hero[] redTeam = new Hero[sep];
            for (int i = 0; i < battleHeroList.Count; i++)
            {
                HeroData hd = new HeroData
                {
                    HeroID = battleHeroList[i].heroID,
                    PosIndex = i,
                    UserName = battleHeroList[i].userName,
                    UnitCfg = GameServices.Config.GetHero(battleHeroList[i].heroID),
                };
                Hero hero;
                if (i < sep)
                {
                    hd.Team = Team.Blue;
                    hd.BornPos = mapCfg.BlueBorn;
                    hero = new Hero(hd);
                    blueTeam[i] = hero;
                }
                else
                {
                    hd.Team = Team.Red;
                    hd.BornPos = mapCfg.RedBorn;
                    hero = new Hero(hd);
                    redTeam[i - sep] = hero;
                }
                hero.LogicInit();
                hero.SetEnvColliders(_logicEnv.GetEnvColliders());
                _heroList.Add(hero);
            }
            CalRule.BlueTeamHero=blueTeam;
            CalRule.RedTeamHero=redTeam;
            
        }
        public void UnInit()
        {
            _heroList.Clear();
        }

        public void Tick()
        {
            //Hero
            for (int i = 0; i < _heroList.Count; i++)
            {
                _heroList[i].LogicTick();
            }
        }

        public void InitCamFollowTrans(int posIndex)
        {
            _transFollow = _heroList[posIndex].MainViewUnit.transform;
        }
         
        private void InitEnv()
        {
            var transMapRoot =GameObject.FindGameObjectWithTag("MapRoot").transform;
            
            _mapRoot =transMapRoot.GetComponent<MapRoot>();
            List<ColliderConfig> envColliderCfgList=GenerateEnvColliCfgs(_mapRoot.transEnvCollider);

            _logicEnv = new EnvColliders()
            {
                envColliCfgLst = envColliderCfgList
            };
            _logicEnv.Init();
        }

        public void Update()
        {
            if (_transFollow != null)
            {
                _mapRoot.transCameraRoot.position=_transFollow.position;
            }
        }


        private List<ColliderConfig> GenerateEnvColliCfgs(Transform transEnvRoot)
        {
            List<ColliderConfig> env=new List<ColliderConfig>();
            BoxCollider[] boxArr = transEnvRoot.GetComponentsInChildren<BoxCollider>();

            for (int i = 0; i < boxArr.Length; i++)
            {
                Transform trans=boxArr[i].transform;

                var cfg = new ColliderConfig
                {
                    mPos = new PEVector3(trans.position)
                };
                cfg.mSize = new PEVector3(trans.localScale / 2);
                cfg.mType = ColliderType.Box;
                cfg.mAxis = new PEVector3[3];
                cfg.mAxis[0] = new PEVector3(trans.right);
                cfg.mAxis[1] = new PEVector3(trans.up);
                cfg.mAxis[2] = new PEVector3(trans.forward);
                env.Add(cfg);
            }
            CapsuleCollider[] cylindderArr = transEnvRoot.GetComponentsInChildren<CapsuleCollider>();
            for(int i = 0; i < cylindderArr.Length; i++) {
                Transform trans = cylindderArr[i].transform;
                var cfg = new ColliderConfig {
                    mPos = new PEVector3(trans.position)
                };
                cfg.mType = ColliderType.Cylinder;
                cfg.mRadius = (PEInt)(trans.localScale.x / 2);

                env.Add(cfg);
            }
            return env;
        }

        public void InputKey(List<OpKey> keyList)
        {
            for (int i = 0; i < keyList.Count; i++)
            {
                OpKey key=keyList[i];
                MainLogicUnit hero = _heroList[key.opIndex];
                hero.InputKey(key);
                
            }
        }

        public List<PEColliderBase> GetEnvColliders()
        {
            return _logicEnv.GetEnvColliders();
        }
       

    }  
    
}