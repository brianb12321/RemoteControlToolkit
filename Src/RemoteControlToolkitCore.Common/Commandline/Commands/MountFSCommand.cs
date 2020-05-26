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
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "mountfs")]
    [CommandHelp("Manages the mount points of the file system.")]
    public class MountFsCommand : RCTApplication
    {
        private MountFileSystem FileSystem { get; set; }

        public override string ProcessName => "Mount Filesystem";

        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
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

            if (mode == "list")
            {
                var mounts = FileSystem.GetMounts();
                context.Out.WriteLine(mounts.ShowDictionary());
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            set.WriteOptionDescriptions(context.Out);
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            FileSystem = (MountFileSystem)kernel.GetRequiredService<FileSystemSubsystem>().GetFileSystem();
        }
    }
}