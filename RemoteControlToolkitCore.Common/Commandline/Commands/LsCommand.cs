using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [PluginModule(Name = "ls", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    [CommandHelp("Shows all folders and files in the VFS.")]
    public class LsCommand : RCTApplication
    {
        private const int TAB = 5;
        public override string ProcessName => "LS";
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            MountFileSystem fileSystem = (MountFileSystem)context.ClientContext.GetExtension<IExtensionFileSystem>().FileSystem;
            string directoryPath = args.Arguments[1].ToString();
            if (fileSystem.DirectoryExists(directoryPath))
            {
                var directories = fileSystem.EnumerateDirectoryEntries(directoryPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var directory in directories)
                {
                    context.Out.Write(Output.Blue(directory.Name) + new string(' ', TAB));
                }
                var files = fileSystem.EnumerateFileEntries(directoryPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (file.Attributes == FileAttributes.System)
                    {
                        context.Out.Write(Output.Cyan(file.Name) + new string(' ', TAB));
                    }
                    else if (file.Attributes == FileAttributes.Compressed)
                    {
                        context.Out.WriteLine(Output.Red(file.Name) + new string(' ', TAB));
                    }
                    else
                    {
                        context.Out.Write(file.Name + new string(' ', TAB));
                    }
                }
                context.Out.WriteLine();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else throw new Exception("Directory does not exist.");
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}