using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using AudioType = TEngine.AudioType;

namespace GameLogic
{
	[Window(UILayer.UI, location : "LoginUI")]
	public partial class LoginUI
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			m_tmpInput_acct.text= Random.Range(100, 999).ToString();
			m_tmpInput_pass.text= Random.Range(100, 999).ToString();
			GameModule.UI.ShowUIAsync<TipsUI>();
		}



		#region 事件

		private partial void OnClick_enterBtn()
		{
			GameModule.Audio.Play(AudioType.UISound, GameServices.Config.GetAudio(AudioKey.LoginBtn));
			if (m_tmp_acct.text.Length >= 3 && m_tmp_pass.text.Length >= 3)
			{
				// ①命令：发 IPlayerCmd.Login（LoginSystem 监听后 NetSvc.Send 发包）
				GameEvent.Get<IPlayerCmd>().Login(m_tmp_acct.text, m_tmp_pass.text);
				GameEvent.Get<ITipsUI>().AddTips("正在登录...");
			}
			else
			{
				GameEvent.Get<ITipsUI>().AddTips("账号密码格式错误");
			}
		}

		private partial void OnClick_gmBtn()
		{
			GameEvent.Get<IPlayerCmd>().GmIntent();
		}

		private partial void OnToggleSrvChange(bool isOn)
		{
			GameEvent.Get<IPlayerCmd>().ServerSelect(isOn);
		}

		#endregion
	}
}
