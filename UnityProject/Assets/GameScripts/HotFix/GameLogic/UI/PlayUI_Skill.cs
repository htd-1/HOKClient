using HOKProtocol;
using TEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class PlayUI
    {
        private SkillItem _skaItem;
        private SkillItem _sk1Item;
        private SkillItem _sk2Item;
        private SkillItem _sk3Item;
        private bool _isForbidReleaseSkill;

        private Image _imgInfoCD;
        private void InitSkillInfo()
        {
            BattleHeroData self = _battleState.BattleHeroList[_battleState.BattleSelfIndex];
            UnitCfg heroCfg = ConfigService.Instance.GetUnit(self.heroID);
            int[] skillArr = heroCfg.SkillArr;
            
            _skaItem = CreateWidget<SkillItem>("m_rect_skill/m_img_ultSkill"); 
            _sk1Item = CreateWidget<SkillItem>("m_rect_skill/m_img_skill1");
            _sk2Item = CreateWidget<SkillItem>("m_rect_skill/m_img_skill2");
            _sk3Item = CreateWidget<SkillItem>("m_rect_skill/m_img_skill3");
            
            _skaItem.Init(ConfigService.Instance.GetSkill(skillArr[0]),0);
            _sk1Item.Init(ConfigService.Instance.GetSkill(skillArr[1]),1);
            _sk2Item.Init(ConfigService.Instance.GetSkill(skillArr[2]),2);
            _sk3Item.Init(ConfigService.Instance.GetSkill(skillArr[3]),3);
            SetForbidState(false);
            m_rect_skillInfo.gameObject.SetActive(false);
            _imgInfoCD = m_rect_skillInfo.Find("cdimg").GetComponent<Image>();
            if(_imgInfoCD==null)Log.Error("Can not find cdimg");
        }

        private void SetForbidState(bool state)
        {
            _sk1Item.SetForbidState(state);
            _sk2Item.SetForbidState(state);
            _sk3Item.SetForbidState(state);
        }

        private void SetForbidState()
        {
            SetForbidState(true);
            _isForbidReleaseSkill = true;
        }

        private void SetImgInfo(int cdTime)
        {
            m_rect_skillInfo.gameObject.SetActive(true);
            
            _showImgInfo=true;
            _showTimeCounter = 0;
            _showTime = cdTime * 1.0f / 1000;
        }
        private void UpdateSkill()
        {
            if (_isForbidReleaseSkill)
            {
                if (!BattleSystem.Instance.IsForbidSelfPlayerReleaseSkill())
                {
                    SetForbidState(false);
                    _isForbidReleaseSkill = false;
                }
            }
        }
        /// <summary>
        /// 逻辑层施法成功后经 GameEvent 触发：找到 SkillID 匹配的技能槽进入 CD。
        /// </summary>
        private void OnSkillEnterCD(int skillID, int cdTime)
        {
            if (_skaItem != null && _skaItem.CheckSkillID(skillID)) { _skaItem.EnterCDState(cdTime); return; }
            if (_sk1Item != null && _sk1Item.CheckSkillID(skillID)) { _sk1Item.EnterCDState(cdTime); return; }
            if (_sk2Item != null && _sk2Item.CheckSkillID(skillID)) { _sk2Item.EnterCDState(cdTime); return; }
            if (_sk3Item != null && _sk3Item.CheckSkillID(skillID)) { _sk3Item.EnterCDState(cdTime); return; }
        }

        private bool _showImgInfo;
        private float _showTimeCounter;
        private float _showTime;
        
        private void UpdateImgInfo(float delta)
        {
            if (_showImgInfo)
            {
                _showTimeCounter+=delta;
                if (_showTimeCounter >= _showTime)
                {
                    _showTimeCounter = 0;
                    _imgInfoCD.gameObject.SetActive(false);
                    _showImgInfo = false;
                }
                else
                {
                    _imgInfoCD.fillAmount = 1-(_showTimeCounter/_showTime);
                }
            }
        }
    }
}