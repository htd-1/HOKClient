using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, location : "StartUI")]
	public partial class StartUI
	{
		#region 事件

		private partial void OnClickStartBtn()
		{
			AudioSvc.Instance.PlayUIAudio(ConfigService.Instance.GetAudio(AudioKey.ComClick1));
			GameEvent.Get<IStartEvent>().OnEnterLobby();
		}

		#endregion
	}
}
