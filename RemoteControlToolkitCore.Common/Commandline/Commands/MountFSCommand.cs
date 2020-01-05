using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [PluginModule(Name = "mountfs", ExecutingSide = NetworkSide.Proxy | NetworkSide.Server)]
    [CommandHelp("Manages the mount points of the file system.")]
    public class MountFSCommand : RCTApplication
    {
        private MountFileSystem _fileSystem { get; set; }

        public override string ProcessName => "Mount Filesystem";

        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            string mode = "list";
            OptionSet set = new OptionSet()
                .Add("help|?", "Displays the help screen.", v => mode = "help")
                .Add("list", "Displays all the mount points in the VFS.", v => mode = "list");
            set.Parse(args.Arguments.Select(a => a.ToString()));
            if(mode == "help")
            {
                set.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "list")
            {
                var mounts = _fileSystem.GetMounts();
                context.Out.WriteLine(mounts.ShowDictionary());
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else
            {
                set.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _fileSystem = (MountFileSystem)kernel.GetRequiredService<IFileSystemSubsystem>().GetFileSystem();
        }
    }
}