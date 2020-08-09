using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Configuration
{
    public class ApplicationOptions
    {
        public BootLoaderOptions BootLoader { get; set; }

        public ApplicationOptions()
        {
            BootLoader = new BootLoaderOptions();
        }

        public class BootLoaderOptions
        {
            public List<StartupOptions> StartupPrograms { get; set; }

            public BootLoaderOptions()
            {
                StartupPrograms = new List<StartupOptions>();
            }
        }

        public class StartupOptions
        {
            public string Name { get; set; }
            public string[] Arguments { get; set; }
        }
    }
}