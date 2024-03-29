﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Crayon.Output;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "ls")]
    [CommandHelp("Shows all folders and files in the VFS.")]
    public class LsCommand : RCTApplication
    {
        private const int TAB = 5;
        public override string ProcessName => "LS";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            IFileSystem fileSystem =
                context.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
            string directoryPath =
                (args.Arguments.Length > 1) ? args.Arguments[1] : context.WorkingDirectory.ToString();
            if (fileSystem.DirectoryExists(directoryPath))
            {
                var directories = fileSystem.EnumerateDirectoryEntries(directoryPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var directory in directories)
                {
                    context.Out.Write(Blue(directory.Name) + new string(' ', TAB));
                }
                var files = fileSystem.EnumerateFileEntries(directoryPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (file.Attributes == FileAttributes.System)
                    {
                        context.Out.Write(Blue(file.Name) + new string(' ', TAB));
                    }
                    else if (file.Attributes == FileAttributes.Compressed)
                    {
                        context.Out.Write(Red(file.Name) + new string(' ', TAB));
                    }
                    else if (file.Attributes == FileAttributes.Device)
                    {
                        context.Out.Write(Yellow(file.Name) + new string(' ', TAB));
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