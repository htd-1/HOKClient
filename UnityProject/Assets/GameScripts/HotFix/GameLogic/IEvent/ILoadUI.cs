using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface ILoadUI
    {
        void RefreshUI();
    }
}
