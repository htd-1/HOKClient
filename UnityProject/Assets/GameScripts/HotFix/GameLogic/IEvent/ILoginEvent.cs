using HOKProtocol;
using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupLogic)]
    public interface ILoginEvent
    {
        void OnLoginIntent(string account, string password);
        void OnGmIntent();
        void OnServerSelectIntent(bool isPublicServer);
        void OnRspLogin(RspLogin data);
        
    }
}
