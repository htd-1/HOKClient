using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using AudioType = TEngine.AudioType;

namespace GameLogic
{
	[Window(UILayer.UI, location : "StartUI")]
	public partial class StartUI
	{
		#region 事件

		private partial void OnClickStartBtn()
		{
			GameModule.Audio.Play(AudioType.UISound, GameServices.Config.GetAudio(AudioKey.ComClick1));
			GameEvent.Get<IStartEvent>().OnEnterLobby();
		}

		#endregion
	}
}
