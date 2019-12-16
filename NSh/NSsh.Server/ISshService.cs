using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NSsh.Server
{
    public interface ISshService
    {
        void DeregisterSession(ISshSession session);
    }
}
