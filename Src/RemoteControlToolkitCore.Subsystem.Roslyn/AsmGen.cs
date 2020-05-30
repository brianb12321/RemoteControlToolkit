using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Crayon;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

[assembly: PluginLibrary("RemoteControlToolkitCore.Subsystem.Roslyn", ".NET Roslyn Assembly Generator.")]
namespace RemoteControlToolkitCore.Subsystem.Roslyn
{
    [Plugin(PluginName = "asmGen")]
    [CommandHelp("Generates dynamic assembly.")]
    public class AsmGen : RCTApplication
    {
        public override string ProcessName => "AsmGen";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            IFileSystem fileSystem = context.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
            string assemblyName = string.Empty;
            string inputMode = "text";
            string outputString = string.Empty;
            string outputMode = "file";
            OutputKind outputType = OutputKind.DynamicallyLinkedLibrary;
            string mode = "help";
            OptionSet options = new OptionSet();
            options.Add("help|?", "Displays the help screen.", v => mode = "help");
            options.Add("assemble|a", "Create an assembly from the input string provided.", v => mode = "assemble");
            options.Add("assemblyName|n=", "The assembly name to generate.", v => assemblyName = v);
            options.Add("file|f", "The input is a file.", v => inputMode = "file");
            options.Add("outputType|u=", "The assembly type to create (Default: DynamicallyLinkedLibrary)", v =>
            {
                if (!Enum.TryParse(v, out outputType))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"The value \"{v}\" for outputType is not valid. The following are valid options: ");
                    foreach (string validOption in Enum.GetNames(typeof(OutputKind)))
                    {
                        sb.Append($"{validOption}, ");
                    }
                    //Remove extra , and space.
                    sb.Length -= 2;
                    context.Out.WriteLine(sb.ToString());
                }
            });
            options.Add("stdIn|i", "The input is from standard in.", v => inputMode = "stdIn");
            options.Add("text|t", "The input is text. (Default)", v => inputMode = "text");
            options.Add("output|o=", "The output location of the compiled assembly.", v => outputString = v);
            options.Add("stdOut", "Redirect the compiled assembly to standard out.", v => outputMode = "stdOut");

            var inputStrings = options.Parse(args.Arguments).Skip(1).ToArray();
            if (mode == "help")
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }

            if (mode == "assemble")
            {
                SyntaxTree tree;
                switch (inputMode)
                {
                    case "text":
                        tree = CSharpSyntaxTree.ParseText(inputStrings[0]);
                        break;
                    case "file":
                        tree = CSharpSyntaxTree.ParseText(fileSystem.ReadAllText(inputStrings[0]));
                        break;
                    case "stdIn":
                        tree = CSharpSyntaxTree.ParseText(context.In.ReadToEnd());
                        break;
                    default:
                        tree = null;
                        break;
                }
                CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(outputType);
                MetadataReference[] references = {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                };
                CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[]{ tree }, references, compilationOptions);
                context.Out.WriteLine("Compiling assembly...".BrightCyan());
                //Setup output.
                //The output stream.
                Stream peStream;
                switch (outputMode)
                {
                    case "stdOut":
                        //Not great but necessary. We shouldn't have to use the network stream.
                        peStream = context.ClientContext.OpenNetworkStream();
                        break;
                    default:
                        peStream = fileSystem.OpenFile(outputString, FileMode.Create, FileAccess.Write);
                        break;
                }

                EmitResult result = compilation.Emit(peStream, 
                    null, 
                    null, 
                    null, 
                    null, 
                    null, 
                    token);
                //Log compilation
                context.Out.WriteLine(!result.Success
                    ? "Compilation failed.".Red()
                    : "Compilation succeeded.".BrightGreen());
                IEnumerable<Diagnostic> diagnostics = result.Diagnostics;
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if(diagnostic.Severity == DiagnosticSeverity.Warning) context.Out.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}".BrightYellow());
                    else if(diagnostic.Severity == DiagnosticSeverity.Error) context.Out.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}".Red());
                    else if(diagnostic.Severity == DiagnosticSeverity.Info) context.Out.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                peStream.Close();
                context.Out.WriteLine($"Compilation finished.".BrightCyan());
                if(result.Success) return new CommandResponse(CommandResponse.CODE_SUCCESS);
                else return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
            else return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }
    }
}