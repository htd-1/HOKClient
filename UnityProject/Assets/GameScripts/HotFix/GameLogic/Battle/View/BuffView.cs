using GameConfig.hok;
using TEngine;

namespace GameLogic
{
    public class BuffView:ViewUnit
    {
        private Buff _buff;
        public override void Init(LogicUnit logicUnit)
        {
            base.Init(logicUnit);
            _buff=logicUnit as Buff;

            if (_buff.Cfg.StaticPosType != StaticPosType.None)
            {
                // Log.Info("init in buffview transform changed");
                // 固定位置buff
                transform.position = _buff.LogicPos.ConvertViewVector3();
                transform.rotation = CalcRotation(_buff.LogicDir.ConvertViewVector3());
                
            }
        }


        //空函数覆盖位置与方向刷新
        protected override void Update()
        {
            
        }

        public void DestroyBuff()
        {
            Destroy(gameObject,0.1f);
        }
    }
}