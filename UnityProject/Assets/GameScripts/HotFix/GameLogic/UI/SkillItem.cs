using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using dnlib.DotNet.Pdb;
using GameConfig.hok;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	public partial class SkillItem
	{
		
		private int _skillIndex;
		private SkillCfg _skillCfg;
		private float _pointDis;
		private Vector2 _startPos;
		private CancellationTokenSource _effectCts;
        
		private HeroView _viewHero;
		protected override void OnCreate()
		{
			base.OnCreate();
			_imgForbid?.gameObject.SetActive(false);

		}

		protected override void RegisterEvent()
		{
			AddUIEvent<MainLogicUnit>(IBattleEvent_Event.OnSelfHeroLoaded, OnSelfHeroLoaded);
		}

		private void OnSelfHeroLoaded(MainLogicUnit selfHero)
		{
			_viewHero = selfHero?.MainViewUnit as HeroView;
		}

		public void Init(SkillCfg cfg,int skillindex)
		{
			_skillIndex = skillindex;
			_skillCfg = cfg;
			
			
			_pointDis = Screen.height * 1.0f / ClientConfig.ScreenStandardHeight *
			            ConfigService.Instance.ClientSetting.SkillOPDis;

			
			if (!_skillCfg.IsNormalAttack)
			{
				_skillIcon.SetSprite(_skillCfg.IconName);
				_imgCD.gameObject.SetActive(false);
				_txtCD.gameObject.SetActive(false);
				
				_skillIcon.gameObject.AddClickDown((evt, args) =>
				{
					_startPos = evt.position;
					_imgCycle.gameObject.SetActive(true);
					_imgPoint.gameObject.SetActive(true);
					// Log.Info($"[当前技能技能] releaseMode={_skillCfg.ReleaseMode} targetCfg={(_skillCfg.TargetCfg != null)}");
					ShowSkillAtkRange(true);

					if (_skillCfg.ReleaseMode == ReleaseMode.Postion)
					{
						_viewHero.SetSkillGuide(_skillIndex,true,ReleaseMode.Postion,Vector3.zero);
					}
					else if (_skillCfg.ReleaseMode == ReleaseMode.Direction)
					{
						_viewHero.SetSkillGuide(_skillIndex,true,ReleaseMode.Direction,Vector3.zero);
					}
				});
				_skillIcon.gameObject.AddDrag((evt, args) =>
				{
					Vector2 dir=evt.position-_startPos;
					float len=dir.magnitude;
					if (len > _pointDis)
					{
						Vector2 clampDir = Vector2.ClampMagnitude(dir, _pointDis);
						_imgPoint.transform.position = _startPos + clampDir;
					}
					else
					{
						_imgPoint.transform.position = evt.position;
					}
					if (_skillCfg.ReleaseMode == ReleaseMode.Postion)
					{
						if (dir == Vector2.zero) return;
						dir = BattleSystem.Instance.SkillDisMultipler * dir;
						Vector2 clampDir = Vector2.ClampMagnitude(dir, _skillCfg.TargetCfg.SelectRange);
						Vector3 clampDirVector3 =new Vector3(clampDir.x, 0, clampDir.y);
						clampDirVector3 = Quaternion.Euler(0, 45, 0) * clampDirVector3;
						_viewHero.SetSkillGuide(skillindex,true,ReleaseMode.Postion,clampDirVector3);

					}
					else if (_skillCfg.ReleaseMode == ReleaseMode.Direction)
					{
						Vector3 dirVector3 =new Vector3(dir.x, 0, dir.y);
						
						dirVector3 = Quaternion.Euler(0, 45, 0) * dirVector3;
						
						_viewHero.SetSkillGuide(skillindex,true,ReleaseMode.Direction,dirVector3.normalized);
					}
					else
					{
						Log.Warning("this type not define in code");
					}

					if (len >= ConfigService.Instance.ClientSetting.SkillCancelDis)
					{
						GameEvent.Get<IBattlePlayUI>().OnSkillCancel(true);
					}
					else
					{
						GameEvent.Get<IBattlePlayUI>().OnSkillCancel(false);
					}
					
				});
				_skillIcon.gameObject.AddClickUp((evt, args) =>
				{
					Vector2 dir =evt.position-_startPos;
					_imgPoint.transform.position = transform.position;
					_imgCycle.gameObject.SetActive(false);
					_imgPoint.gameObject.SetActive(false);
					
					GameEvent.Get<IBattlePlayUI>().OnSkillCancel(false);
					
					ShowSkillAtkRange(false);

					if (dir.magnitude >= ConfigService.Instance.ClientSetting.SkillCancelDis)
					{
						_viewHero.DisableSkillGuide(skillindex);
						return;
					}

					if (_skillCfg.ReleaseMode == ReleaseMode.Click)
					{
						ClickSkillItem();
					}
					else if (_skillCfg.ReleaseMode == ReleaseMode.Postion)
					{
						dir = BattleSystem.Instance.SkillDisMultipler * dir;
						Vector2 clampDir = Vector2.ClampMagnitude(dir, _skillCfg.TargetCfg.SelectRange);
						Vector3 clampDirVector3 =new Vector3(clampDir.x, 0, clampDir.y);
						clampDirVector3 = Quaternion.Euler(0, 45, 0) * clampDirVector3;
						
						_viewHero.DisableSkillGuide(skillindex);

						ClickSkillItem(clampDirVector3);

						ShowEffect().Forget();
					}

					else if (_skillCfg.ReleaseMode == ReleaseMode.Direction)
					{
						
						if (dir == Vector2.zero) return;
						Vector3 dirVector3 =new Vector3(dir.x, 0, dir.y);
						
						dirVector3 = Quaternion.Euler(0, 45, 0) * dirVector3;
						
						_viewHero.DisableSkillGuide(skillindex);
						ClickSkillItem(dirVector3.normalized);
					}
					else
					{
						Log.Warning("this type not define in code");
					}
				});
			}
			else
			{
				//普通攻击
				_skillIcon.gameObject.AddClickDown((evt,args) =>
				{
					ShowSkillAtkRange(true);
					ClickSkillItem();
				});
				
				_skillIcon.gameObject.AddClickUp((evt, args) =>
				{
					ShowSkillAtkRange(false);
					ShowEffect().Forget();
					//disable range todo
				});
			}
			
		}

		private async UniTask ShowEffect()
		{
			_effectCts?.Cancel();   // 打断上一次
			_effectCts = CancellationTokenSource.CreateLinkedTokenSource(
				gameObject.GetCancellationTokenOnDestroy());
			var token = _effectCts.Token;

			_effectRoot.gameObject.SetActive(false);  
			_effectRoot.gameObject.SetActive(true);   
			try
			{
				await UniTask.Delay(500, cancellationToken: token);
			}
			catch (OperationCanceledException) { return; }
			_effectRoot.gameObject.SetActive(false);
		}

		private void ShowSkillAtkRange(bool state)
		{
			//没配置就不显示攻击圆圈
			if (_skillCfg?.TargetCfg != null)
			{
				_viewHero?.SetAtkSkillRange(state,_skillCfg.TargetCfg.SelectRange);
			}
		}

		// === CD 倒计时 ===
		private int _cdTimerId = -1;     // -1 表示当前无 CD
		private int _cdTotalSec;         // 总秒数（算 fillAmount 用）
		private int _cdLeftSec;          // 剩余秒数

		public bool CheckSkillID(int skillID)
		{
			return _skillCfg != null && _skillCfg.SkillID == skillID;
		}

		public void EnterCDState(int cdTime)
		{
			// cdTime 单位毫秒。重入保护：若已在 CD 中，先清掉旧 timer
			ClearCD();

			int sec = cdTime / 1000;     // 整秒；ms 尾数忽略（TEngine Timer 无首次延迟参数）
			if (sec <= 0) return;

			_cdTotalSec = sec;
			_cdLeftSec  = sec;

			// 初始显示（第 0 秒）
			_imgCD.gameObject.SetActive(true);
			_txtCD.gameObject.SetActive(true);
			_txtCD.SetText(sec.ToString());
			_imgCD.fillAmount = 1f;
			_skillIcon.raycastTarget = false;   // CD 期间拦截点击

			// 循环定时器：每 1 秒 tick。注意传方法组 CDTick，不是 CDTick()
			_cdTimerId = GameModule.Timer.AddTimer(CDTick, 1f, isLoop: true);
		}

		private void CDTick(object[] args)
		{
			_cdLeftSec--;
			if (_cdLeftSec > 0)
			{
				_txtCD.SetText(_cdLeftSec.ToString());
				_imgCD.fillAmount = (float)_cdLeftSec / _cdTotalSec;   // 秒级跳变：满黑→0
			}
			else
			{
				// CD 结束
				ClearCD();
				ShowEffect().Forget();   // 就绪高亮（复用已有方法）
				// GameModule.Audio.Play(AudioType.UISound, "com_cd_ok"); // 就绪音效，按需接
			}
		}

		public void ClearCD()
		{
			if (_cdTimerId > 0)
			{
				GameModule.Timer.RemoveTimer(_cdTimerId);   // 必须移除，否则继续 tick
				_cdTimerId = -1;
			}
			_imgCD?.gameObject.SetActive(false);
			_txtCD?.gameObject.SetActive(false);
			if (_skillIcon != null) _skillIcon.raycastTarget = true;
		}

		protected override void OnDestroy()
		{
			ClearCD();          // ★ widget 回收时清 timer，否则销毁后 tick 触发会空引用
			base.OnDestroy();
		}
		public void ClickSkillItem()
		{
			BattleSystem.Instance.SendSkillKey(_skillCfg.SkillID,Vector3.zero);
		}

		public void ClickSkillItem(Vector3 vec)
		{
			BattleSystem.Instance.SendSkillKey(_skillCfg.SkillID,vec);
		}
		public void SetForbidState(bool state)
		{
			_imgForbid?.gameObject.SetActive(state);
			
		}
	}
}
