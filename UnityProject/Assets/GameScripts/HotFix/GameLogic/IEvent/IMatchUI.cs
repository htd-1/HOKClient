using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface IMatchUI
    {
        void RefreshUI();
    }
}
