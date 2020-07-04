using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public struct EnvironmentVariable
    {
        public object Value { get; set; }
        public string Name { get; }
        public bool Inheritable { get; set; }

        public EnvironmentVariable(string name, object value)
        {
            Name = name;
            Value = value;
            Inheritable = true;
        }

        public EnvironmentVariable(string name, object value, bool inheritable)
        {
            Name = name;
            Value = value;
            Inheritable = inheritable;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(EnvironmentVariable left, object right)
        {
            return left.Value.Equals(right);
        }

        public static bool operator != (EnvironmentVariable left, object right)
        {
            return !left.Value.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj != null && ((obj is EnvironmentVariable variable && Value == variable.Value) ||
                   Value.Equals(obj));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}