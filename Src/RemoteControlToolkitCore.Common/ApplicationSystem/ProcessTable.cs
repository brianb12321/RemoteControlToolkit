using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class ProcessTable : IProcessTable
    {
        public RctProcess.RctProcessFactory Factory { get; private set; }
        private readonly ConcurrentDictionary<uint, RctProcess> _activeProcesses;

        public uint LatestProcess
        {
            get
            {
                if (!_activeProcesses.Any()) return 0;
                return _activeProcesses.Values.Last().Pid;
            }
        }

        public ProcessTable(IServiceProvider provider)
        {
            Factory = new RctProcess.RctProcessFactory(this, provider);
            _activeProcesses = new ConcurrentDictionary<uint, RctProcess>();
        }

        public void AddProcess(RctProcess process)
        {
            _activeProcesses.TryAdd(process.Pid, process);
        }

        public void CancelProcess(uint pid)
        {
            if (_activeProcesses.TryRemove(pid, out RctProcess process))
            {
                process.Close();
            }
            else throw new ProcessException($"Unable to lookup proccess with id {pid}");
        }

        public void AbortProcess(uint pid)
        {
            if (_activeProcesses.TryRemove(pid, out RctProcess process))
            {
                process.Abort();
            }
            else throw new ProcessException($"Unable to lookup process with id {pid}");
        }

        public void RemoveProcess(uint pid)
        {
            if (_activeProcesses.TryRemove(pid, out _))
            {
            }
            else throw new ProcessException($"Unable to lookup process with id {pid}");
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
                    // ignored
                }
            }
        }
    }
}