
using HOKProtocol;
using TEngine;
using AudioType = TEngine.AudioType;

namespace GameLogic
{
	[Window(UILayer.UI, location : "LobbyUI")]
	public partial class LobbyUI
	{
		private int _currentTime = 0;
		private int _timerID = -1;
		private GameEventMgr _dataMgr;
		#region 事件

		private partial void OnClick_match1v1Btn()
		{
			GameModule.Audio.Play(AudioType.UISound, GameServices.Config.GetAudio(AudioKey.MatchBtn));
			GameEvent.Get<ILobbyCmd>().ReqMatch(PVPEnum._1V1);
		}

		private partial void OnClick_rankBtn()
		{
			GameModule.Audio.Play(AudioType.UISound, GameServices.Config.GetAudio(AudioKey.MatchBtn));
			GameEvent.Get<ILobbyCmd>().ReqMatch(PVPEnum._2V2);
		}

		#endregion

		protected override void OnCreate()
		{
			base.OnCreate();
			m_rect_matchInfoRoot.gameObject.SetActive(false);
			// 推送式：私有 mgr 订阅 ILobbyUI.ShowMatchInfo（匹配浮层）+ IPlayerData.Changed（玩家信息）+ 拉首次快照
			_dataMgr = new GameEventMgr();
			_dataMgr.AddEvent<bool, int>(ILobbyUI_Event.ShowMatchInfo, ShowMatchInfo);
			_dataMgr.AddEvent<UserData>(IPlayerData_Event.Changed, OnPlayerDataChanged);
			GameEvent.Get<IPlayerCmd>().RequestSnapshot();
		}

		protected override void OnDestroy()
		{
			_dataMgr?.Clear();
			_dataMgr = null;
			base.OnDestroy();
		}

		private void TimeFlash(object[] args)
		{
			_currentTime++;
			SetCountTime();
		}

		private void OnPlayerDataChanged(UserData data)
		{
			if (data == null) return;

			m_tmp_info.text = data.name;
			m_tmp_level.text = data.lv.ToString();
			m_tmp_exp.text = data.exp.ToString();
			m_tmp_coin.text = data.coin.ToString();
			m_tmp_diamond.text = data.diamond.ToString();
			m_tmp_ticket.text = data.ticket.ToString();
		}

		private void InitTimer()
		{
			if(_timerID != -1)return;
			_timerID=GameModule.Timer.AddTimer(TimeFlash, time: 1f, isLoop: true);
		}

		private void DestroyTimer()
		{
            if(_timerID==-1)return;
			GameModule.Timer.RemoveTimer(_timerID);
			_timerID = -1;
		}
		private void ShowMatchInfo(bool isActive,int predictTime=0)
		{
			if(isActive)
			{
				InitTimer();
				m_rect_matchInfoRoot.gameObject.SetActive(true);
				
				m_tmp_predictTime.text="预计匹配时间："+FormatTime(predictTime);
			}
			else
			{
				DestroyTimer();
				m_rect_matchInfoRoot.gameObject.SetActive(false);
				
				_currentTime = 0;
			}
		}

		private void SetCountTime()
		{
			m_tmp_countTime.text=FormatTime(_currentTime);
		}
		private string FormatTime(int time)
		{
			int min = time / 60;
			int sec = time % 60;
			string minStr = min < 10 ? "0" + min + ":" : min + ":";
			string secStr = sec < 10 ? "0" + sec : sec.ToString();
			return minStr + secStr;
		}


	}
}
