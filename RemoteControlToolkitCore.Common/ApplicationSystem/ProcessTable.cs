using System.Collections.Concurrent;
using System.Linq;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class ProcessTable : IProcessTable
    {
        public RCTProcess.RCTPRocessFactory Factory { get; private set; }
        private ConcurrentDictionary<uint, RCTProcess> _activeProcesses;

        public uint LatestProcess
        {
            get
            {
                if (!_activeProcesses.Any()) return 0;
                return _activeProcesses.Values.Last().Pid;
            }
        }

        public ProcessTable()
        {
            Factory = new RCTProcess.RCTPRocessFactory(this);
            _activeProcesses = new ConcurrentDictionary<uint, RCTProcess>();
        }

        public void AddProcess(RCTProcess process)
        {
            _activeProcesses.TryAdd(process.Pid, process);
        }

        public void CancelProcess(uint pid)
        {
            if (_activeProcesses.TryRemove(pid, out RCTProcess process))
            {
                process.Close();
            }
            else throw new ProcessException($"Unable to lookup proccess with id {pid}");
        }

        public void AbortProcess(uint pid)
        {
            if (_activeProcesses.TryRemove(pid, out RCTProcess process))
            {
                process.Abort();
            }
            else throw new ProcessException($"Unable to lookup proccess with id {pid}");
        }

        public void RemoveProcess(uint pid)
        {
            if (_activeProcesses.TryRemove(pid, out RCTProcess process))
            {
            }
            else throw new ProcessException($"Unable to lookup proccess with id {pid}");
        }

        public bool ProcessExists(uint pid)
        {
            return _activeProcesses.ContainsKey(pid);
        }

        public void SendControlC(uint pid)
        {
            _activeProcesses[pid].InvokeControlC();
        }

        public void CloseAll()
        {
            foreach (uint pid in _activeProcesses.Keys)
            {
                try
                {
                    CancelProcess(pid);
                }
                catch
                {
                }
            }
        }
    }
}