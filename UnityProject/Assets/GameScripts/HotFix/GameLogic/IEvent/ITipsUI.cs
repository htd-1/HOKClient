using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface ITipsUI
    {
        void AddTips(string tips);
        void AnimationFinished();
    }
}