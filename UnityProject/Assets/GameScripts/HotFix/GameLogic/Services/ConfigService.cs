using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameConfig.hok;
using HOKProtocol;
using Luban;
using PEMath;
using PEPhysx;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 配置服务：通过 Luban 生成的 Tables 提供业务层配置查询。
    /// 对外仍返回业务 DTO（UnitCfg/MapCfg），内部从 Luban Tables 转换。
    /// </summary>
    public sealed class ConfigService : Singleton<ConfigService>
    {
        private Tables _tables;
        private readonly List<TextAsset> _loadedAssets = new();
        private Dictionary<string, string> _audioCache;

        /// <summary>
        /// 加载配置表。当前底层为同步加载（YooAsset 本地资源），
        /// 保留 async 签名以便未来切换为真正的异步加载。
        /// </summary>
        public async UniTask LoadAsync()
        {
            await UniTask.Yield();
            ConfigSystem.Instance.Load(LoadByteBuf);
            _tables = ConfigSystem.Instance.Tables;
            Log.Info("[ConfigService] Config tables loaded.");
        }

        /// <summary>
        /// 释放配置缓存并卸载所有已加载的 TextAsset（Singleton 释放时由基类调用）。
        /// </summary>
        protected override void OnRelease()
        {
            var resourceModule = GameModule.Resource;
            foreach (var asset in _loadedAssets)
            {
                if (asset != null) resourceModule.UnloadAsset(asset);
            }
            _loadedAssets.Clear();
            ConfigSystem.Instance.Release();
            _tables = null;
        }

        /// <summary>
        /// Luban bytes 加载回调：通过 GameModule.Resource 同步加载。
        /// </summary>
        private ByteBuf LoadByteBuf(string file)
        {
            var resourceModule = GameModule.Resource;
            TextAsset textAsset = resourceModule.LoadAsset<TextAsset>(file);

            if (textAsset == null)
            {
                Log.Error($"[ConfigService] Failed to load config bytes: {file}");
                throw new Exception($"Config bytes not found: {file}");
            }

            _loadedAssets.Add(textAsset);
            return new ByteBuf(textAsset.bytes);
        }

        /// <summary>
        /// 获取客户端定点更新帧时间间隔
        /// </summary>
        /// <returns></returns>
        public PEInt GetClientFrameTime()
        {
            return (PEInt)Configs.ClientLogicFrameDeltaSec;
        }
        
        
        #region Hero 查询

        /// <summary>
        /// 获取可选英雄列表（用于选英雄界面）。
        /// 优先使用服务器下发的 heroSelectData，否则从配置表生成。
        /// </summary>
        public List<HeroSelectData> GetHeroList(UserData userData)
        {
            if (userData?.heroSelectData != null && userData.heroSelectData.Count > 0)
            {
                return userData.heroSelectData;
            }

            // 从 Luban TbHero 配置生成
            var heroes = new List<HeroSelectData>();
            foreach (var heroCfg in _tables.TbHero.DataList)
            {
                if (heroCfg.Selectable)
                {
                    heroes.Add(new HeroSelectData { heroID = heroCfg.UnitId });
                }
            }

            return heroes;
        }

        /// <summary>
        /// 获取英雄对应的单位配置（业务 DTO）。
        /// </summary>
        public UnitCfg GetHero(int heroID)
        {
            return GetUnit(heroID);
        }

        public string GetHeroResName(int heroID)
        {
            var unit = GetUnitRecord(heroID);
            return unit?.ResName ?? "arthur";
        }

        public string GetHeroName(int heroID)
        {
            var unit = GetUnitRecord(heroID);
            return unit?.Name ?? "未知英雄";
        }

        public int[] GetHeroSkillIDs(int heroID)
        {
            var unit = GetUnitRecord(heroID);
            if (unit?.Skills == null) return null;
            var arr = new int[unit.Skills.Count];
            for (int i = 0; i < unit.Skills.Count; i++) arr[i] = unit.Skills[i];
            return arr;
        }

        #endregion

        #region Unit 查询

        /// <summary>
        /// 获取单位配置，转换为业务层 UnitCfg（包含 PEInt/PEVector3/ColliderConfig）。
        /// </summary>
        public UnitCfg GetUnit(int unitID)
        {
            var record = GetUnitRecord(unitID);
            if (record == null) return null;
            return ConvertUnit(record);
        }

        private Unit GetUnitRecord(int unitID)
        {
            return _tables?.TbUnit.GetOrDefault(unitID);
        }

        /// <summary>
        /// Luban Unit → 业务 UnitCfg 转换。
        /// float→PEInt, int colliderType→ColliderType enum。
        /// </summary>
        private static UnitCfg ConvertUnit(Unit src)
        {
            if (src == null) return null;

            var cfg = new UnitCfg
            {
                UnitID = src.Id,
                UnitName = src.Name,
                ResName = src.ResName,
                HitHeight = (PEInt)src.HitHeight,
                Hp = src.Hp,
                Def = src.Def,
                MoveSpeed = src.MoveSpeed,
                ColliderCfg = new ColliderConfig
                {
                    mType = (ColliderType)src.ColliderType,
                    mRadius = (PEInt)src.ColliderRadius,
                },
            };

            if (src.PassiveBuffs != null && src.PassiveBuffs.Count > 0)
            {
                cfg.PasvBuff = src.PassiveBuffs.ToArray();
            }

            if (src.Skills != null && src.Skills.Count > 0)
            {
                cfg.SkillArr = src.Skills.ToArray();
            }

            return cfg;
        }

        #endregion

        #region Map 查询

        /// <summary>
        /// 获取地图配置，转换为业务层 MapCfg。
        /// Vector3→PEVector3 转换。
        /// </summary>
        public MapCfg GetMap(int mapID)
        {
            var record = _tables?.TbMap.GetOrDefault(mapID);
            if (record == null) return null;

            return new MapCfg
            {
                MapID = record.Id,
                BlueBorn = ToPEVector3(record.BlueBorn),
                RedBorn = ToPEVector3(record.RedBorn),
                TowerIDArr = ToIntArray(record.TowerIds),
                TowerPosArr = ToPEVector3Array(record.TowerPositions),
                BornDelay = record.BornDelay,
                BornInterval = record.BornInterval,
                WaveInterval = record.WaveInterval,
                BlueSoldierIDArr = ToIntArray(record.BlueSoldierIds),
                BlueSoldierPosArr = ToPEVector3Array(record.BlueSoldierPositions),
                RedSoldierIDArr = ToIntArray(record.RedSoldierIds),
                RedSoldierPosArr = ToPEVector3Array(record.RedSoldierPositions),
            };
        }

        #endregion

        #region Skill / Buff / Bullet / TargetRule 查询

        public SkillCfg GetSkill(int skillID)
        {
            return _tables?.TbSkill.GetOrDefault(skillID);
        }

        public BuffCfg GetBuff(int buffID)
        {
            return _tables?.TbBuff.GetOrDefault(buffID);
        }

        public TargetRule GetTargetRule(int ruleID)
        {
            return _tables?.TbTargetRule.GetOrDefault(ruleID);
        }

        #endregion

        #region 类型转换工具

        private static PEVector3 ToPEVector3(Vector3 v)
        {
            return new PEVector3((PEInt)v.x, (PEInt)v.y, (PEInt)v.z);
        }

        private static PEVector3[] ToPEVector3Array(IReadOnlyList<Vector3> src)
        {
            if (src == null) return null;
            var arr = new PEVector3[src.Count];
            for (int i = 0; i < src.Count; i++)
            {
                arr[i] = ToPEVector3(src[i]);
            }
            return arr;
        }

        private static int[] ToIntArray(IReadOnlyList<int> src)
        {
            if (src == null) return null;
            var arr = new int[src.Count];
            for (int i = 0; i < src.Count; i++) arr[i] = src[i];
            return arr;
        }

        #endregion

        #region Client 配置查询（手感 + 音频）

        /// <summary>
        /// 客户端手感配置（单行 id=1）：摇杆触发距离 / 技能摇杆距离 / 技能取消距离。
        /// </summary>
        public ClientSetting ClientSetting => _tables?.TbClientSetting.GetOrDefault(1);

        /// <summary>
        /// 按业务键查询音频资源路径。key 对应 client_audio 表 key 列。
        /// </summary>
        public string GetAudio(string key)
        {
            EnsureAudioCache();
            return _audioCache != null && _audioCache.TryGetValue(key, out var path) ? path : null;
        }

        /// <summary>
        /// 按强类型 enum 查询音频资源路径（推荐）。enum 值名与 client_audio 表 key 列一一对应。
        /// </summary>
        public string GetAudio(AudioKey key) => GetAudio(key.ToString());

        /// <summary>
        /// 按分类（bgm / sfx_battle / sfx_ui）获取该组全部音频记录。
        /// </summary>
        public IReadOnlyList<ClientAudio> GetGroup(string group)
        {
            var result = new List<ClientAudio>();
            if (_tables?.TbClientAudio == null) return result;
            foreach (var rec in _tables.TbClientAudio.DataList)
            {
                if (rec.Group == group) result.Add(rec);
            }
            return result;
        }

        private void EnsureAudioCache()
        {
            if (_audioCache != null) return;
            _audioCache = new Dictionary<string, string>();
            if (_tables?.TbClientAudio == null) return;
            foreach (var rec in _tables.TbClientAudio.DataList)
            {
                if (!string.IsNullOrEmpty(rec.Key))
                    _audioCache[rec.Key] = rec.Path;
            }
        }

        #endregion
    }

    /// <summary>
    /// 客户端音频业务键（强类型）。值名与 client_audio 表 key 列一一对应，
    /// 经 ConfigService.GetAudio(AudioKey) → ToString() 查表。
    /// </summary>
    public enum AudioKey
    {
        // BGM
        MainBgm, BattleBgm, LoadBgm,
        // 战斗事件音效
        Welcombattle, Firstblood, SelfDeath, SelfTowerDestroy, DestroyEnemyTower, Victory, Defeat,
        // UI 音效
        LoginBtn, MatchBtn, MatchSureBtn, MatchReminder, SelectHeroBtn,
        ComClick1, ComClick2, ComCdOk,
    }
}
