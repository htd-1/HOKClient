using System.Collections.Generic;
using HOKProtocol;
using UnityEngine;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, location : "SelectUI")]
	public partial class SelectUI
	{
		private bool _isSelect;
		private int _selectedHeroId = -1;
		private GameEventMgr _lobbyDataMgr;
		private GameEventMgr _playerDataMgr;
		private UserData _userData;
		private int _selectRemainingTime;
		private List<HeroSelectData> _heroSelectList;
		private readonly List<ItemHero> _heroItems = new();

		#region 事件

		private partial void OnClick_confirmBtn()
		{
			ConfirmSelect();
		}

		#endregion

		protected override void OnCreate()
		{
			base.OnCreate();
			m_btn_confirm.interactable = true;
			_isSelect = false;
			_selectedHeroId = -1;

			// 推送式：订阅大厅流程数据（倒计时/超时）+ 玩家数据（英雄列表输入），拉首次快照
			_lobbyDataMgr = new GameEventMgr();
			_lobbyDataMgr.AddEvent<LobbyState>(ILobbyData_Event.Changed, OnLobbyDataChanged);
			_playerDataMgr = new GameEventMgr();
			_playerDataMgr.AddEvent<UserData>(IPlayerData_Event.Changed, OnPlayerDataChanged);

			GameEvent.Get<IPlayerCmd>().RequestSnapshot();
			GameEvent.Get<ILobbyCmd>().RequestSnapshot();
		}

		protected override void OnDestroy()
		{
			ClearHeroItems();
			_lobbyDataMgr?.Clear();
			_lobbyDataMgr = null;
			_playerDataMgr?.Clear();
			_playerDataMgr = null;
			base.OnDestroy();
		}

		private void OnPlayerDataChanged(UserData data)
		{
			_userData = data;
			BuildHeroList();
		}

		private void OnLobbyDataChanged(LobbyState state)
		{
			_selectRemainingTime = state.SelectRemainingTime;
			RefreshCountDown();
			// 超时自动选（_selectedHeroId 为 UI 当前选中，默认首个）；倒计时由 LobbySystem Timer 驱动递减
			if (!_isSelect && _selectRemainingTime <= 0 && _selectedHeroId >= 0)
			{
				ConfirmSelect();
			}
		}

		private void BuildHeroList()
		{
			if (_userData == null) return;
			if (_heroSelectList != null) return;

			_heroSelectList = GameServices.Config.GetHeroList(_userData);
			CreateHeroItems();

			if (_heroItems.Count > 0)
			{
				OnHeroSelected(_heroSelectList[0].heroID, GameServices.Config.GetHeroResName(_heroSelectList[0].heroID));
			}
		}

		private void CreateHeroItems()
		{
			GameObject template = m_rect_content.GetChild(0).gameObject;
			template.SetActive(false);

			for (int i = 0; i < _heroSelectList.Count; i++)
			{
				GameObject go = Object.Instantiate(template, m_rect_content);
				go.SetActive(true);
				var item = CreateWidget<ItemHero>(go);
				item.Setup(_heroSelectList[i], OnHeroSelected);
				_heroItems.Add(item);
			}
		}

		private void ClearHeroItems()
		{
			for (int i = _heroItems.Count - 1; i >= 0; i--)
			{
				_heroItems[i].Destroy();
			}
			_heroItems.Clear();

			for (int i = m_rect_content.childCount - 1; i >= 1; i--)
			{
				Object.Destroy(m_rect_content.GetChild(i).gameObject);
			}
		}

		private void OnHeroSelected(int heroId, string resName)
		{
			if (_isSelect) return;

			_selectedHeroId = heroId;

			for (int i = 0; i < _heroItems.Count; i++)
			{
				_heroItems[i].SetSelected(_heroItems[i].HeroId == heroId);
			}

			m_img_hero.SetSprite(resName + "_show", setNativeSize: false);

			m_img_skill0.SetSprite(resName + "_sk0", setNativeSize: true);
			m_img_skill1.SetSprite(resName + "_sk1", setNativeSize: true);
			m_img_skill2.SetSprite(resName + "_sk2", setNativeSize: true);
			m_img_skill3.SetSprite(resName + "_sk3", setNativeSize: true);
		}

		private void ConfirmSelect()
		{
			if (_isSelect) return;
			if (_selectedHeroId < 0) return;

			_isSelect = true;
			m_btn_confirm.interactable = false;

			// ①命令：发 ILobbyCmd.SndSelect（LobbySystem 监听后发包 + 停倒计时）
			GameEvent.Get<ILobbyCmd>().SndSelect(_selectedHeroId);
		}

		private void RefreshCountDown()
		{
			int time = _selectRemainingTime;
			int min = time / 60;
			int sec = time % 60;
			m_tmp_confirm.text = $"{min:D2}:{sec:D2}";
		}
	}
}
