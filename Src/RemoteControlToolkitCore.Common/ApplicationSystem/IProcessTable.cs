namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public interface IProcessTable
    {
        RCTProcess.RCTPRocessFactory Factory { get; }
        uint LatestProcess { get; }
        void AddProcess(RCTProcess process);
        void CancelProcess(uint pid);
        void AbortProcess(uint pid);
        void RemoveProcess(uint pid);
        bool ProcessExists(uint pid);
        void SendControlC(uint pid);
        void CloseAll();
    }
}