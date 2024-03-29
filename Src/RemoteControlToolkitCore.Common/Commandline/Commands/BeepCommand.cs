﻿using System;
using System.Linq;
using System.Threading;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "beep")]
    [CommandHelp("Generates a sine wave.")]
    public class BeepCommand : RCTApplication
    {
        public override string ProcessName => "Beep";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            int frequency = 1000;
            int duration = 1000;
            bool showHelp = false;
            OptionSet set = new OptionSet()
                .Add("f|frequency=", "The frequency at which to play the sound. (Hz)", (v) =>
                {
                    if (int.TryParse(v, out int result))
                    {
                        if (result > 32767 || result < 37)
                        {
                            throw new Exception("Frequency must be in the range of 37 and 32767 Hz.");
                        }
                        frequency = result;
                    }
                    else
                    {
                        throw new Exception("Frequency must be a number.");
                    }
                })
                .Add("d|duration=", "Duration in milliseconds.", v =>
                {
                    if (int.TryParse(v, out int result))
                    {
                        duration = result;
                    }
                    else
                    {
                        throw new Exception("Duration must be a number.");
                    }
                })
                .Add("help|?", "Displays the help screen.", v => showHelp = true);
            set.Parse(args.Arguments);
            if (showHelp)
            {
                set.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            Console.Beep(frequency, duration);
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}