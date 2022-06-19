using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public class PluginSubsystemCollection : IList<PluginSubsystem>
    {
        private List<PluginSubsystem> _internalList;

        public PluginSubsystemCollection()
        {
            _internalList = new List<PluginSubsystem>();
        }
        public IEnumerator<PluginSubsystem> GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(PluginSubsystem pluginSubsystem)
        {
            _internalList.Add(pluginSubsystem);
        }

        public void Clear()
        {
            _internalList.Clear();
        }

        public bool Contains(PluginSubsystem item)
        {
            return _internalList.Contains(item);
        }

        public void CopyTo(PluginSubsystem[] array, int arrayIndex)
        {
            _internalList.CopyTo(array, arrayIndex);
        }

        public bool Remove(PluginSubsystem item)
        {
            return _internalList.Remove(item);
        }

        public int Count => _internalList.Count;
        public bool IsReadOnly => false;
        public int IndexOf(PluginSubsystem item)
        {
            return _internalList.IndexOf(item);
        }

        public void Insert(int index, PluginSubsystem item)
        {
            _internalList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _internalList.RemoveAt(index);
        }

        public PluginSubsystem this[int index]
        {
            get => _internalList[index];
            set => _internalList[index] = value;
        }
    }
}