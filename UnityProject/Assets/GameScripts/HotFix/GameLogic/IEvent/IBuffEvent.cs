using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface IBuffEvent
    {
        void CheckUIInput(bool isUIInput);
    }
}