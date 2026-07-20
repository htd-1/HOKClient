using System.Collections.Generic;
using HOKProtocol;

namespace GameLogic
{
    /// <summary>
    /// L5 域状态：大厅流程（匹配/确认/选英雄）+ 加载进度 + 战斗启动数据。由 <see cref="LobbySystem"/> 私有持有。
    /// <para>纯数据 POCO，不耦合事件总线；变更由 LobbySystem 写入后发 <c>ILobbyData.Changed</c>。</para>
    /// <para>本类为大厅域唯一状态源（由 LobbySystem 私有持有，经 ILobbyData.Changed 推送）。</para>
    /// </summary>
    public sealed class LobbyState
    {
        // === 匹配流程（Lobby/Match，进 Load 即作废）===
        public int LobbyPredictMatchTime { get; private set; }
        public bool IsMatching { get; private set; }
        public uint MatchRoomID { get; private set; }
        public ConfirmData[] MatchConfirmData { get; private set; }
        public int MatchConfirmRemainingTime { get; private set; }

        // === 选英雄（Select）===
        public int SelectRemainingTime { get; private set; }
        public int SelectedHeroID { get; private set; } = -1;

        // === 加载进度（Load）===
        public NtfLoadRes LoadingData { get; private set; }
        public int LoadingTotalProgress { get; private set; }
        public List<int> LoadingProgress { get; private set; }

        // === 战斗启动数据（跨 Load→Battle→战斗结束）===
        public int BattleMapID { get; private set; }
        public List<BattleHeroData> BattleHeroList { get; private set; }
        public int BattleSelfIndex { get; private set; } = -1;

        // --- 匹配 ---
        public void SetMatchPredictTime(int predictTime) => LobbyPredictMatchTime = predictTime;

        public void SetMatching(bool matching, int predictTime = 0)
        {
            IsMatching = matching;
            if (matching) LobbyPredictMatchTime = predictTime;
        }

        public void StartMatchConfirm(NtfConfirm confirm, int countdown)
        {
            MatchRoomID = confirm.roomID;
            MatchConfirmData = confirm.confirmArr;
            MatchConfirmRemainingTime = countdown;
        }

        public void UpdateMatchConfirm(NtfConfirm confirm)
        {
            MatchRoomID = confirm.roomID;
            MatchConfirmData = confirm.confirmArr;
        }

        public void TickMatchConfirm()
        {
            if (MatchConfirmRemainingTime > 0) MatchConfirmRemainingTime--;
        }

        public void ClearMatchConfirm()
        {
            MatchRoomID = 0;
            MatchConfirmData = null;
            MatchConfirmRemainingTime = 0;
        }

        // --- 选英雄 ---
        public void StartSelect(int countdown)
        {
            SelectRemainingTime = countdown;
            SelectedHeroID = -1;
        }

        public void SetSelectedHero(int heroID) => SelectedHeroID = heroID;

        public void TickSelect()
        {
            if (SelectRemainingTime > 0) SelectRemainingTime--;
        }

        public void ClearSelect()
        {
            SelectRemainingTime = 0;
            SelectedHeroID = -1;
        }

        // --- 加载 ---
        public void StartLoading(NtfLoadRes data)
        {
            LoadingData = data;
            BattleMapID = data?.mapID ?? 0;
            BattleHeroList = data?.heroList;
            BattleSelfIndex = data?.posIndex ?? -1;

            int count = BattleHeroList?.Count ?? 0;
            LoadingProgress = new List<int>(count);
            for (int i = 0; i < count; i++) LoadingProgress.Add(0);
            LoadingTotalProgress = 0;
        }

        public void UpdateLoadingProgress(NtfLoadPrg data)
        {
            if (data?.percentLst == null || data.percentLst.Count == 0) return;

            int count = data.percentLst.Count;
            if (LoadingProgress == null || LoadingProgress.Count != count)
            {
                LoadingProgress = new List<int>(count);
            }
            else
            {
                LoadingProgress.Clear();
            }

            int total = 0;
            for (int i = 0; i < count; i++)
            {
                int progress = data.percentLst[i];
                LoadingProgress.Add(progress);
                total += progress;
            }
            LoadingTotalProgress = total / count;
        }

        public int GetLoadingProgress(int index)
        {
            if (LoadingProgress == null || index < 0 || index >= LoadingProgress.Count) return 0;
            return LoadingProgress[index];
        }

        public void ClearLoading()
        {
            LoadingData = null;
            LoadingProgress = null;
            LoadingTotalProgress = 0;
        }
    }
}
