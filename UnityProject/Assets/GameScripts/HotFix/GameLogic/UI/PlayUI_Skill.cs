using HOKProtocol;

namespace GameLogic
{
    public partial class PlayUI
    {
        private SkillItem _skaItem;
        private SkillItem _sk1Item;
        private SkillItem _sk2Item;
        private SkillItem _sk3Item;
        private void InitSkillInfo()
        {
            BattleHeroData self = _battleState.BattleHeroList[_battleState.BattleSelfIndex];
            UnitCfg heroCfg = GameServices.Config.GetUnit(self.heroID);
            int[] skillArr = heroCfg.SkillArr;
            
            _skaItem = CreateWidget<SkillItem>("m_rect_skill/m_img_ultSkill"); 
            _sk1Item = CreateWidget<SkillItem>("m_rect_skill/m_img_skill1");
            _sk2Item = CreateWidget<SkillItem>("m_rect_skill/m_img_skill2");
            _sk3Item = CreateWidget<SkillItem>("m_rect_skill/m_img_skill3");
            
            _skaItem.Init(GameServices.Config.GetSkill(skillArr[0]),0);
            _sk1Item.Init(GameServices.Config.GetSkill(skillArr[1]),1);
            _sk2Item.Init(GameServices.Config.GetSkill(skillArr[2]),2);
            _sk3Item.Init(GameServices.Config.GetSkill(skillArr[3]),3);
            SetForbidState(false);
            m_rect_skillInfo.gameObject.SetActive(false);
        }

        private void SetForbidState(bool state)
        {
            _sk1Item.SetForbidState(state);
            _sk2Item.SetForbidState(state);
            _sk3Item.SetForbidState(state);
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
    }
}