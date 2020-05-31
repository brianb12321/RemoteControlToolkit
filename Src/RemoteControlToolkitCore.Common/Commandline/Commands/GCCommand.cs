using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "gc")]
    [CommandHelp("Controls the system garbage collector, a service that automatically reclaims unused memory.")]
    public class GCCommand : RCTApplication
    {
        public override string ProcessName => "Garbage Collection Control";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            string mode = string.Empty;
            int generation = GC.MaxGeneration;
            bool block = false;
            GCCollectionMode collectionMode = GCCollectionMode.Default;
            OptionSet options = new OptionSet();
            options.Add("addMemoryPressure=",
                "Informs the runtime of a large allocation of unmanaged memory {BYTES} that should be taken into account when scheduling garbage collection.",
                v => GC.AddMemoryPressure(long.Parse(v)));
            options.Add("removeMemoryPressure=",
                "Informs the runtime that unmanaged memory {BYTES} has been released and no longer needs to be taken into account when scheduling garbage collection.",
                v => GC.RemoveMemoryPressure(int.Parse(v)));
            options.Add("collectionCount=",
                "Returns the number of times garbage collection has occurred for the specified {GENERATION} of objects.",
                v => context.Out.WriteLine(GC.CollectionCount(int.Parse(v))));
            options.Add("collect:", "Forces an immediate garbage collection of all generations, {GENERATION} generations, or a specific {MODE}, whether to {BLOCK}.", (k, v) =>
            {
                mode = "collect";
                if (k == "generation") generation = int.Parse(v);
                if (k == "mode") collectionMode = (GCCollectionMode) Enum.Parse(typeof(GCCollectionMode), v, true);
                if (k == "block") block = true;
            });
            options.Add("help|?", "Displays the help screen.", v => options.WriteOptionDescriptions(context.Out));
            options.Add("maxGenerations", "Gets the maximum number of generations that the system currently supports.",
                v => context.Out.WriteLine(GC.MaxGeneration));
            options.Add("totalMemory:",
                "Retrieves the number of bytes currently thought to be allocated. A parameter indicates whether this method can {WAIT} a short interval before returning, to allow the system to collect garbage and finalize objects.",
                new Action<bool>((v) => context.Out.WriteLine(GC.GetTotalMemory(v))));
            try
            {
                options.Parse(args.Arguments);
                if (mode == "collect")
                {
                    GC.Collect(generation, collectionMode, block);
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            catch (Exception e)
            {
                context.Out.WriteLine($"An error occurred while performing a GC action: {e.Message}".Red());
                return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
        }
    }
}