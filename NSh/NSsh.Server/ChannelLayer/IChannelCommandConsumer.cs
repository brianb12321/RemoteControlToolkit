using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSsh.Server.ChannelLayer
{
    public interface IChannelCommandConsumer : IChannelConsumer
    {
        string Command { get; set; }
    }
}
