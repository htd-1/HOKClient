using TEngine;
using UnityEngine.UIElements;

namespace GameLogic
{
    
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IStartEvent
    {
        void OnEnterLobby();
    }
}