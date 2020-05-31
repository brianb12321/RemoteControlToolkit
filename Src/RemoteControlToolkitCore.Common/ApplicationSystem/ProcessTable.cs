using System;
using System.Collections.Concurrent;
using System.Linq;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class ProcessTable : IProcessTable
    {
        private readonly ConcurrentDictionary<uint, RctProcess> _activeProcesses;

        public IProcessBuilder CreateProcessBuilder()
        {
            return new RctProcess.RctProcessBuilder(this);
        }

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