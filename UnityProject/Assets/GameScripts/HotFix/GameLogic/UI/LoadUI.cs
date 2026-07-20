using System.Collections.Generic;
using HOKProtocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, location : "LoadUI")]
	public partial class LoadUI
	{
		private GameEventMgr _dataMgr;
		private LobbyState _state;

		protected override void OnCreate()
		{
			base.OnCreate();
			// 推送式：订阅 ILobbyData.Changed 刷新加载进度 + 拉首次快照（替代直读 RuntimeData）
			_dataMgr = new GameEventMgr();
			_dataMgr.AddEvent<LobbyState>(ILobbyData_Event.Changed, OnLobbyDataChanged);
			GameEvent.Get<ILobbyCmd>().RequestSnapshot();
		}

		protected override void OnDestroy()
		{
			_dataMgr?.Clear();
			_dataMgr = null;
			base.OnDestroy();
		}

		private void OnLobbyDataChanged(LobbyState state)
		{
			_state = state;
			RefreshUI();
		}

		private void RefreshUI()
		{
			List<BattleHeroData> heroes = _state?.BattleHeroList;
			int count = heroes?.Count ?? 0;
			int sep = count / 2;
			RefreshTeam((Transform)m_rect_blueTeam, 0, sep);
			RefreshTeam((Transform)m_rect_redTeam, sep, count);
		}

		private void RefreshTeam(Transform teamRoot, int startIndex, int endIndex)
		{
			int teamCount = endIndex - startIndex;
			for (int i = 0; i < teamRoot.childCount; i++)
			{
				if (i < teamCount)
				{
					RefreshPlayer(teamRoot.GetChild(i), startIndex + i);
				}
				else
				{
					teamRoot.GetChild(i).gameObject.SetActive(false);
				}
			}
		}

		private void RefreshPlayer(Transform playerRoot, int index)
		{
			List<BattleHeroData> heroes = _state?.BattleHeroList;
			bool hasHero = heroes != null && index >= 0 && index < heroes.Count;
			playerRoot.gameObject.SetActive(hasHero);
			if (!hasHero) return;

			BattleHeroData hero = heroes[index];
			string resName = GameServices.Config.GetHeroResName(hero.heroID);

			playerRoot.Find("heroimg").GetComponent<Image>().SetSprite(resName + "_load", setNativeSize: false);
			playerRoot.Find("heroName").GetComponent<TextMeshProUGUI>().text = GameServices.Config.GetHeroName(hero.heroID);
			playerRoot.Find("nameBg/playerName").GetComponent<TextMeshProUGUI>().text = hero.userName;
			playerRoot.Find("progress").GetComponent<TextMeshProUGUI>().text = _state.GetLoadingProgress(index) + "%";
		}
	}
}
