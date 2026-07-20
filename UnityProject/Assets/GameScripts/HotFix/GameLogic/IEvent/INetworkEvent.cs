using HOKProtocol;
using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupLogic)]
    public interface INetworkEvent
    {
        void OnNetworkStatusChanged(NetworkStatus status);
        void OnNetworkError(ErrorCode error);
        void OnNetworkWarning(string message);
    }
}
