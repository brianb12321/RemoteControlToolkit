using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "ps")]
    [CommandHelp("Manages the RCT process table.")]
    public class PsCommand : RCTApplication
    {
        private IHostApplication _application;
        public override string ProcessName => "Process status command";

        public override void InitializeServices(IServiceProvider provider)
        {
            _application = provider.GetService<IHostApplication>();
        }

        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            bool global = false;
            bool help = false;
            OptionSet options = new OptionSet()
                .Add("help|?", "Displays the help screen.", v => help = true)
                .Add("global|g", "Display the global process table.", v => global = true);

            options.Parse(args.Arguments);
            if (help)
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            IProcessTable table;
            if (global)
            {
                context.Out.WriteLine("Showing processes for global:\n\n");
                table = _application.GlobalSystemProcessTable;
            }
            else
            {
                context.Out.WriteLine("Showing processes for local connection:\n\n");
                table = context.ClientContext.ProcessTable;
            }
            Node rootNode = new Node(table.GetName(1), 1);
            populateChildren(rootNode, table);
            rootNode.PrintPretty("", true, context.Out);

            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        private void populateChildren(Node node, IProcessTable table)
        {
            //Base case
            if (!table.HasChildren(node.PID)) return;

            foreach (var children in table.GetChildren(node.PID))
            {
                Node childNode = new Node(table.GetName(children), children);
                node.Children.Add(childNode);
                populateChildren(childNode, table);
            }
        }
        private class Node
        {
            private string Name { get; set; }
            public uint PID { get; set; }
            public List<Node> Children { get; set; }

            public Node(string name, uint pid)
            {
                Name = name;
                PID = pid;
                Children = new List<Node>();
            }
            public void PrintPretty(string indent, bool last, TextWriter outWriter)
            {
                outWriter.Write(indent);
                if (last)
                {
                    outWriter.Write("└─");
                    indent += "  ";
                }
                else
                {
                    outWriter.Write("├─");
                    indent += "| ";
                }
                outWriter.WriteLine($"{PID}: {Name}");

                for (int i = 0; i < Children.Count; i++)
                    Children[i].PrintPretty(indent, i == Children.Count - 1, outWriter);

            }

        }
    }
}