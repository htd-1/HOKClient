using HOKProtocol;
using UnityEngine.UI;
using TEngine;
using AudioType = TEngine.AudioType;

namespace GameLogic
{
	[Window(UILayer.UI, location : "MatchUI")]
	public partial class MatchUI
	{
		private GameEventMgr _dataMgr;
		private ConfirmData[] _confirmArr;
		private int _remainingTime;

		#region 事件

		private partial void OnClick_confirmBtn()
		{
			m_btn_confirm.interactable = false;
			GameModule.Audio.Play(AudioType.UISound, GameServices.Config.GetAudio(AudioKey.MatchSureBtn));
			// ①命令：发 ILobbyCmd.SndConfirm（LobbySystem 监听后发包）
			GameEvent.Get<ILobbyCmd>().SndConfirm();
		}

		#endregion


		protected override void OnCreate()
		{
			base.OnCreate();
			// 推送式：订阅 ILobbyData.Changed 刷新确认数据/倒计时 + 拉首次快照（倒计时由 LobbySystem Timer 驱动）
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
			_confirmArr = state.MatchConfirmData;
			_remainingTime = state.MatchConfirmRemainingTime;
			RefreshUI();
		}

		private void RefreshUI()
		{
			if (_confirmArr == null) return;

			int count=_confirmArr.Length/2;
			for (int i = 0; i < 5; i++)
			{
				SetLeftIcon(i,count, _confirmArr);
			}

			for (int i = 0; i < 5; i++)
			{
				SetRightIcon(i+count, count, _confirmArr);
			}

			int confirmCount = 0;
			for (int i = 0; i < _confirmArr.Length; i++)
			{
				if(_confirmArr[i].confirmDone)++confirmCount;
			}
			m_tmp_confirm.text=confirmCount + "/" + _confirmArr.Length + "就绪";
			SetCountTime();
		}

		private void SetCountTime()
		{
			int time = _remainingTime;
			int min = time / 60;
			int sec = time % 60;
			m_tmp_time.text = $"{min:D2}:{sec:D2}";
		}

		private void SetLeftIcon(int index,int count,ConfirmData[] confirmArr)
		{
			var player = m_rect_leftplayers.GetChild(index);
			if (index< count)
			{
				player.gameObject.SetActive(true);

				string iconName = "icon_" + confirmArr[index].iconIndex;
				string frameName = "frame_" + (confirmArr[index].confirmDone ? "sure" : "normal");

				Image imgIcon = player.GetComponent<Image>();
				imgIcon.SetSprite(iconName);

				Image imgFrame = player.Find("img_state").GetComponent<Image>();
				imgFrame.SetSprite(frameName, setNativeSize: true);
			}
			else
			{
				player.gameObject.SetActive(false);
			}
		}
		private void SetRightIcon(int index, int count, ConfirmData[] confirmArr)
		{
			var player = m_rect_rightplayers.GetChild(index-count);
			if (index < count<<1)
			{
				player.gameObject.SetActive(true);

				string iconName = "icon_" + confirmArr[index].iconIndex;
				string frameName = "frame_" + (confirmArr[index].confirmDone ? "sure" : "normal");

				Image imgIcon = player.GetComponent<Image>();
				imgIcon.SetSprite(iconName);

				Image imgFrame = player.Find("img_state").GetComponent<Image>();
				imgFrame.SetSprite(frameName, setNativeSize: true);
			}
			else
			{
				player.gameObject.SetActive(false);
			}
		}
	}
}
