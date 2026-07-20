using HOKProtocol;
using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface ILobbyUI
    {
        void ShowMatchInfo(bool isActive,int predictTime=0);
    }
}