using GameConfig.hok;
using HOKProtocol;
using PEMath;

namespace GameLogic
{
    /// <summary>
    /// 后羿被动攻速叠加buff(buffID=10200)
    /// </summary>
    public class HouyiPasvAttackSpeedBuff : Buff
    {
        private int _currOverCount;
        private int _maxOverCount;
        private int _resetTime;

        private PEInt _speedAddtion;
        private PEInt _speedOffset;
        
        
        public HouyiPasvAttackSpeedBuff(MainLogicUnit source, MainLogicUnit owner, Skill skill, int buffID, object[] args = null)
            : base(source, owner, skill, buffID, args)
        {
        }

        public override void LogicInit()
        {
            base.LogicInit();
            HouyiPasvAttackSpeedBuffCfg hpsb = Cfg
                as HouyiPasvAttackSpeedBuffCfg;
            _currOverCount = 0;
            _maxOverCount = hpsb.OverCount;
            _resetTime = hpsb.ResetTime;
            _speedAddtion=hpsb.SpeedAddtion;
            _speedOffset = PEInt.zero;

            Skill[] skillArr = Owner.GetAllSkill();
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
                if (_currOverCount >= _maxOverCount)
                {
                    return;
                }
                else
                {
                    ++_currOverCount;
                    PEInt add = Owner.AttackSpeedRateBase * (_speedAddtion / 100);
                    _speedOffset += add;
                    _isCounter = true;
                    Owner.ModifyAttackSpeed(add);
                }
            }
            else
            {
                if (skill.SkillId != 1021)
                {
                    ResetSpeed();
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
                    ResetSpeed();
                    _timeCount = 0;
                    _isCounter = false;
                }
            }
        }

        private void ResetSpeed()
        {
            Owner.ModifyAttackSpeed(-_speedOffset);
            _speedOffset=PEInt.zero;
            _currOverCount = 0;
        }
    }
}
