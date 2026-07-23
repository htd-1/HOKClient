
using System.Text;
using HOKProtocol;
using PEMath;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 临时方案 后续改成 如果是键盘输入就直接搞一个InputSystem_Action的封装直接在一个单例里面读写然后通知
    /// 如果涉及UI就走UI通知一个单例然后发送输入 (这些内容AI无需在意)
    /// <para>战斗上下文（FightMgr/selfIndex/roomID）由 <see cref="ProcedureBattle"/>.OnEnter 经 <see cref="Active"/> 一次性注入，
    /// 不再读全局 RuntimeData。</para>
    /// </summary>
    public class BattleInputSvc:Singleton<BattleInputSvc>
    {
        private FightMgr _fightMgr;
        private int _selfIndex;
        private uint _roomID;
        private uint _keyID=0;
        private bool _active;

        public float SkillDisMultipler=0.03f;
        public bool IsActive=>_active;
        public uint KeyID => ++_keyID;
        public new void Active(FightMgr fightMgr, int selfIndex, uint roomID)
        {
           _fightMgr = fightMgr;
           _selfIndex = selfIndex;
           _roomID = roomID;
           _active = true;
        }

        public override void Release()
        {
            base.Release();
            _active = false;
            _fightMgr = null;
        }
        /// <summary>
        /// 发送移动帧操作到服务器
        /// </summary>
        /// <param name="logicDir"></param>
        /// <returns></returns>
        public bool SendMoveKey(PEVector3 logicDir)
        {
            if (!_active)
            {
                return false;
            }

            if (!CanMove())
            {
                return false;
            }
            var msg = new HOKMsg {
                cmd = CMD.SndOpKey,
                sndOpKey = new SndOpKey {
                    opKey = new OpKey {
                        opIndex = _selfIndex,
                        keyType = KeyType.Move,
                        moveKey = new MoveKey()
                    }
                }
            };
            msg.sndOpKey.opKey.moveKey.x = logicDir.x.ScaledValue;
            msg.sndOpKey.opKey.moveKey.z = logicDir.z.ScaledValue;
            msg.sndOpKey.opKey.moveKey.keyID = KeyID;
            NetSvc.Instance.Send(msg);
            return true;
        }

        public void SendSkillKey(int skillID)
        {
            SendSkillKey(skillID,Vector3.zero);
        }
        public void SendSkillKey(int skillID,Vector3 vec)
        {
            if(!CanReleaseSkill(skillID))
            {
                Log.Info("skill can not release ");
                return;
            }
            HOKMsg msg = new HOKMsg
            {
                cmd=CMD.SndOpKey,
                sndOpKey = new SndOpKey
                {
                    roomID = _roomID,
                    opKey = new OpKey
                    {
                       opIndex    = _selfIndex,
                       keyType = KeyType.Skill,
                       skillKey = new SkillKey
                       {
                           skillID = (uint)skillID,
                           x_value = ((PEInt)vec.x).ScaledValue,
                           z_value = ((PEInt)vec.z).ScaledValue,
                       }
                    }
                }
            };
            NetSvc.Instance.Send(msg);
        }

        private bool CanReleaseSkill(int  skillID)
        {
            return _fightMgr.CanReleaseSkill(_selfIndex,skillID);
        }
        private bool CanMove()
        {
            return _fightMgr.CanMove(_selfIndex);
        }

        public bool IsForbidSelfPlayerReleaseSkill()
        {
            return _fightMgr.IsForbidReleaseSkill(_selfIndex);
        }
    }
}
