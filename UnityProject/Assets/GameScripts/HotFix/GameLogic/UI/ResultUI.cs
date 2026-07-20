using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, location : "ResultUI")]
	public partial class ResultUI
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			// 结算数据写入由 BattleSystem.OnRspBattleEnd 负责（双写 RuntimeData.FinishBattle），UI 不再直写数据层。
			// ResultUI 由 ProcedureBattle.OnRspBattleEnd → ExitToResult 触发 ShowUI，到达时结算状态已就绪。
			m_img_result.SetSprite("win", setNativeSize: true);
		}

		private partial void OnClick_continueBtn()
		{
			GameModule.UI.CloseUI<PlayUI>();
			GameModule.UI.CloseUI<ResultUI>();
		}
	}
}
