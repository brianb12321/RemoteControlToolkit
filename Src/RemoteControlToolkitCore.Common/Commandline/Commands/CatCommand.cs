﻿using System;
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
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "cat")]
    [CommandHelp("Reads the VFS or StdIn and prints to StdOut.")]
    public class CatCommand : RCTApplication
    {
        public override string ProcessName => "Cat";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            if (args.Arguments.Length > 1)
            {
                IFileSystem fileSystem = context.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                StreamReader sr = new StreamReader(fileSystem.OpenFile(args.Arguments[1], FileMode.Open, FileAccess.Read, FileShare.Read));
                while (!sr.EndOfStream && !token.IsCancellationRequested)
                {
                    context.Out.Write((char)sr.Read());
                }
                sr.Close();
            }
            else
            {
                context.Out.WriteLine(context.In.ReadToEnd());
            }
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}