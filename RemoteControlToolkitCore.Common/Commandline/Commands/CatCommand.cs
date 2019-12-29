using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [PluginModule(Name = "cat", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    [CommandHelp("Reads the VFS and prints to StdOut.")]
    public class CatCommand : RCTApplication
    {
        public override string ProcessName => "Cat";
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            IFileSystem fileSystem = context.ClientContext.GetExtension<IExtensionFileSystem>().FileSystem;
            StreamReader sr = new StreamReader(fileSystem.OpenFile(args.Arguments[1].ToString(), FileMode.Open, FileAccess.Read, FileShare.Read));
            while (!sr.EndOfStream)
            {
                context.Out.WriteLine(sr.ReadLine());
            }
            sr.Close();
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}