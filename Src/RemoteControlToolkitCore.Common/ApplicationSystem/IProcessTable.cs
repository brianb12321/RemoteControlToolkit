using System;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public interface IProcessTable
    {
        /// <summary>
        /// Creates a new process builder.
        /// </summary>
        /// <returns>The instantiated process builder.</returns>
        IProcessBuilder CreateProcessBuilder();
        uint LatestProcess { get; }
        void AddProcess(RctProcess process);
        void CancelProcess(uint pid);
        void AbortProcess(uint pid);
        void RemoveProcess(uint pid);
        bool ProcessExists(uint pid);
        void SendControlC(uint pid);
        void CloseAll();
    }
}