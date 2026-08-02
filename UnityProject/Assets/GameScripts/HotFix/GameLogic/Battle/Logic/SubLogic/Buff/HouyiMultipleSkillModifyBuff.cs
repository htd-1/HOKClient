using GameConfig.hok;
using HOKProtocol;
using PEMath;
using TEngine;
using TEngine.Localization;

namespace GameLogic
{
    /// <summary>
    /// 后羿被动普攻修改buff(buffID=10201),逻辑待迁移。
    /// </summary>
    public class HouyiMultipleSkillModifyBuff : Buff
    {

        private int _originalID;
        private int _powerID;
        private int _superPowerID;

        private Skill _modifySkill;

        private int _currOverCount;
        private int _triggerOverCount;
        private int _resetTime;
        
        
        public HouyiMultipleSkillModifyBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }


        public override void LogicInit()
        {
            base.LogicInit();
            HouyiMultipleSkillModifyBuffCfg hpmb=Cfg as  HouyiMultipleSkillModifyBuffCfg;

            _triggerOverCount = hpmb.TriggerOverCount;
            _resetTime = hpmb.ResetTime;
            _originalID = hpmb.OriginalID;
            _powerID = hpmb.PowerID;
            _superPowerID = hpmb.SuperPowerID;
            Skill[] skillArr = Owner.GetAllSkill();
            _modifySkill = Owner.GetSkillByID(_originalID);
            for (int i = 0; i < skillArr.Length; i++)
            {
                skillArr[i].SpellSuccCallback += OnSpellSkillSucc;
            }
        }

        protected override void Start()
        {
            base.Start();
        }
        
        private void OnSpellSkillSucc(Skill skill)
        {
            if (skill.Cfg.IsNormalAttack)
            {
                _timeCount = 0;
                if (_currOverCount >= _triggerOverCount)
                {
                    if (Owner.IsPlayerSelf())
                    {
                        GameEvent.Get<IBattlePlayUI>().SetImgInfo(_resetTime);
                    }
                    return;
                }
                else
                {
                    ++_currOverCount;
                    if (_currOverCount == _triggerOverCount)
                    {
                        _isCounter = true;
                        if (_modifySkill.TempSkillID == 0)
                        {
                            _modifySkill.ReplaceSkillCfg(_powerID);
                        }
                        else
                        {
                            _modifySkill.ReplaceSkillCfg(_superPowerID);
                        }
                    } 
                }
            }
            else
            {
                if (skill.SkillId != 1021)
                {
                    ResetSkill();
                }
            }
        }
        private bool _isCounter;
        private int _timeCount;
        protected override void Tick()
        {
            base.Tick();
            if (_isCounter)
            {
                _timeCount += ServerConfig.ServerLogicFrameIntervelMs;
                if (_timeCount >= _resetTime)
                {
                    ResetSkill();
                    _timeCount = 0;
                    _isCounter = false;
                }
            }
        }

        private void ResetSkill()
        {
            _currOverCount = 0;
            if (_modifySkill.TempSkillID == _powerID)
            {
                _modifySkill.ReplaceSkillCfg(_originalID);
            }
            else if (_modifySkill.TempSkillID == _superPowerID)
            {
                _modifySkill.ReplaceSkillCfg(1024);
            }
            else
            {
                Log.Info("reset skill already");
            }
        }
    }
}
