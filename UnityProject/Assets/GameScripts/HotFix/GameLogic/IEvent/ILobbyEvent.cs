using HOKProtocol;
using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupLogic)]
    public interface ILobbyEvent
    {
        void OnReqMatch(PVPEnum pvpMode);
        void OnRspMatch(RspMatch data);
        void OnNtfConfirm(NtfConfirm data);
        void OnNtfSelect();
        void OnNtfLoadRes(NtfLoadRes data);
        void OnNtfLoadPrg(NtfLoadPrg data);
        void OnSndConfirm();
        void OnSndSelect(int heroID);
    }
}
