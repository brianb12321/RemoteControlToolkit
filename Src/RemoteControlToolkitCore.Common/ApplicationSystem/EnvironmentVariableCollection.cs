using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class EnvironmentVariableCollection : Dictionary<string, EnvironmentVariable>
    {
        public EnvironmentVariableCollection() : base()
        {

        }
        public EnvironmentVariableCollection(EnvironmentVariableCollection parentCollection) : base(parentCollection)
        {

        }

        void addInternal(string name, object value, bool inheritable = true)
        {
            if (ContainsKey(name))
            {
                Remove(name);
            }
            Add(name, new EnvironmentVariable(name, value, inheritable));
        }

        public void AddVariable(string name, object value)
        {
            addInternal(name, value);
        }

        public void AddVariableLocal(string name, object value)
        {
            addInternal(name, value, false);
        }
    }
}