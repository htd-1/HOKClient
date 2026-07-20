using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.Tips, location : "TipsUI")]
	public partial class TipsUI
	{
		private Queue<string> _tipsQue = new ();
		private bool _isTipsShow = false;
		private Animator _animator;
		protected override void OnCreate()
		{
			base.OnCreate();
			EventMgr.AddEvent<string>(ITipsUI_Event.AddTips,AddTips);
			EventMgr.AddEvent(ITipsUI_Event.AnimationFinished,AniPlayFinished);
			_animator=m_rect_tipsBg.gameObject.GetComponentInParent<Animator>();
			_isTipsShow=false;
			m_rect_tipsBg.gameObject.SetActive(false);
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			EventMgr.Clear();
		}

		protected override void OnUpdate()
		{
			// Log.Info(_tipsQue.Count);
			// if(_animator.Equals(null))Log.Warning($"{this.Type.ToString()} _animator is null");
			if (!_isTipsShow&&_tipsQue.Count>0)
			{
				string tips = _tipsQue.Dequeue();
				SetTips(tips);
			}

		}

		private void SetTips(string tips)
		{
			int len=tips.Length;
			m_rect_tipsBg.sizeDelta=new Vector2(35 * len + 100, 80);
			SetActive(true);
			m_tmp_tips.SetText(tips);
			// m_tmp_tips.ForceMeshUpdate();

			_animator.Play("TipsWindow", 0, 0);
		}

		private void AddTips(string tips)
		{
			_tipsQue.Enqueue(tips);
			// Log.Info("EnqueueTips");
		}

		private void AniPlayFinished()
		{
			// Log.Info("AniPlayFinished");
			SetActive(false);
		}

		private void SetActive(bool active)
		{
			m_rect_tipsBg.gameObject.SetActive(active);
			_isTipsShow = active;
		}
	}
}
