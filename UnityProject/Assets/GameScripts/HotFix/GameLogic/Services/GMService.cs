using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HOKProtocol;
using PEMath;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// GM 命令服务。离线战斗、调试功能入口。
    /// </summary>
    public  class GMService:Singleton<GMService>,IFixedUpdate
    {
        private bool _isActive=false;
        public bool IsActive=>_isActive;
        private uint _frameID = 0;
        private List<OpKey>opKeyList=new ();
        public void StartSimulate()
        {
            _isActive = true;
            SimulateFlow().Forget();
        }

        private async UniTask SimulateFlow()
        {
            await SimulateLoadRes();
            
            await UniTask.Delay(1000);
            
            await SimulateBattleStart();
        }

        private  UniTask SimulateLoadRes()
        {
            HOKMsg msg = new HOKMsg
            {
                cmd = CMD.NtfLoadRes,
                ntfLoadRes = new NtfLoadRes
                {
                    mapID = 102,
                    heroList = new List<BattleHeroData>
                    {
                        new BattleHeroData { heroID = 101, userName = "hdt1" },
                        new BattleHeroData { heroID = 102, userName = "hdt2" },
                        new BattleHeroData { heroID = 101, userName = "hdt3" },
                        new BattleHeroData { heroID = 101, userName = "hdt4" },
                        new BattleHeroData { heroID = 101, userName = "hdt5" },
                        new BattleHeroData { heroID = 102, userName = "hdt6" },
                    },
                    posIndex=1
                }
            };
            
            // 离线模式补发确认（与在线流程对齐；模拟服务器不响应，仅走客户端命令链路）
            LobbySystem.Instance.SndConfirm();

            // 注入收包队列，由 NetSvc.Pump→HandoutMsg 分发（与在线收包同路径）
            NetSvc.Instance.AddMsgQue(msg);
            
            return UniTask.CompletedTask;
        }

        private UniTask SimulateBattleStart()
        {
            HOKMsg msg = new HOKMsg
            {
               cmd=CMD.RspBattleStart
            };

            NetSvc.Instance.AddMsgQue(msg);
            
            return  UniTask.CompletedTask;
        }

        public void SimulateServerRcvMsg(HOKMsg msg)
        {
            switch (msg.cmd)
            {
                case CMD.SndOpKey:
                    UpdateOpkey(msg.sndOpKey.opKey);
                    break;
                default:
                    break;
            }
        }

        public void OnFixedUpdate()
        {
            if (!_isActive) return;
            ++_frameID;
            HOKMsg msg = new HOKMsg
            {
                  cmd = CMD.NtfOpKey,
                  ntfOpKey = new NtfOpKey
                  {
                      frameID =  _frameID,
                      keyList = new List<OpKey>()
                  }
            };
            int count=opKeyList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    msg.ntfOpKey.keyList.Add(opKeyList[i]);
                }
            }
            opKeyList.Clear();
            NetSvc.Instance.AddMsgQue(msg);
        }
        private void UpdateOpkey(OpKey key)
        {
            opKeyList.Add(key);
        }

        
    }
}
