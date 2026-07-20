using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TEngine;

namespace GameLogic
{
    public enum JumpType
    {
        None,
        SkillDamage,
        BuffDamage,
        Cure,
        SlowSpeed,
    }

    public enum JumpAni
    {
        None,
        LeftCurve,
        RightCurve,
        CenterUp
    }

    public class JumpUpdateInfo
    {
        public int JumpVal;
        public Vector2 Pos;
        public JumpType JumpType;
        public JumpAni JumpAni;
    }

    public class JumpNum : MonoBehaviour
    {
        public RectTransform Rect;
        public Animator Ani;
        public TextMeshProUGUI Text;

        public int MaxFont;
        public int MinFont;
        public int MaxFontValue;
        public Color SkillDamageColor;
        public Color BuffDamageColor;
        public Color CureDamageColor;
        public Color SlowSpeedColor;

        JumpNumPool ownerPool;

        public void Init(JumpNumPool ownerPool)
        {
            this.ownerPool = ownerPool;
        }

        public void Show(JumpUpdateInfo ji)
        {
            int fontSize = (int)Mathf.Clamp(ji.JumpVal * 1.0f / MaxFontValue, MinFont, MaxFont);
            Text.fontSize = fontSize;
            Rect.anchoredPosition = ji.Pos;
            Log.Info("我运行到这里了");
            switch (ji.JumpType)
            {
                case JumpType.SkillDamage:
                    Log.Info("我运行到这里了1");
                    Text.text = ji.JumpVal.ToString();
                    Text.color = SkillDamageColor;
                    break;
                case JumpType.BuffDamage:
                    Text.text = ji.JumpVal.ToString();
                    Text.color = BuffDamageColor;
                    break;
                case JumpType.Cure:
                    Text.text = "+" + ji.JumpVal;
                    Text.color = CureDamageColor;
                    break;
                case JumpType.SlowSpeed:
                    Text.text = "减速";
                    Text.color = SlowSpeedColor;
                    break;
            }

            switch (ji.JumpAni)
            {
                case JumpAni.LeftCurve:
                    Ani.Play("JumpLeft", 0);
                    break;
                case JumpAni.RightCurve:
                    Ani.Play("JumpRight", 0);
                    break;
                case JumpAni.CenterUp:
                    Ani.Play("JumpCenter", 0);
                    break;
            }

            Recycle().Forget();
        }

        private async UniTaskVoid Recycle()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f));
            Ani.Play("Empty");
            ownerPool.PushOne(this);
        }
    }

    public class JumpNumPool
    {
        private Transform _poolRoot;
        private Queue<JumpNum> _jumpNumQue;

        public JumpNumPool(int count, Transform poolRoot)
        {
            _poolRoot = poolRoot;
            _jumpNumQue = new Queue<JumpNum>();

            for (int i = 0; i < count; i++)
            {
                PushOne(CreateOne());
            }
        }

        private int index = 0;
        private int Index=>index++;

        private JumpNum CreateOne()
        {
            GameObject go = GameModule.Resource.LoadGameObject("JumpNum", _poolRoot);
            go.name = "JumpNum_" + Index;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            JumpNum jn = go.GetComponent<JumpNum>();
            jn.Init(this);
            return jn;
        }

        public JumpNum PopOne()
        {
            if (_jumpNumQue.Count > 0)
            {
                return _jumpNumQue.Dequeue();
            }
            Log.Warning("飘字超额，动态调整上限");
            PushOne(CreateOne());
            return PopOne();
        }

        public void PushOne(JumpNum jn)
        {
            _jumpNumQue.Enqueue(jn);
        }
    }
}
