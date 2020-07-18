using System.Diagnostics;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    [Plugin(PluginName = "External")]
    public class ExternalApplicationProcessFactory : PluginModule<ProcessFactorySubsystem>, IProcessFactory
    {
        public IProcessBuilder CreateProcessBuilder(RctProcess parentProcess, IProcessTable table)
        {
            IProcessBuilder builder = table.CreateProcessBuilder()
                .SetProcessName(args => Process.GetProcessesByName(args)[0].ProcessName)
                .SetParent(parentProcess)
                .SetAction((args, current, token) =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = args.Arguments[0];
                    process.StartInfo.Arguments = args.GetArguments();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.OutputDataReceived += (sender, args) => current.Out.WriteLine(sender);
                    process.ErrorDataReceived += (sender, args) => current.Error.WriteLine(sender);
                    token.Register(() => process.Kill());
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    return new CommandResponse(process.ExitCode);
                });

            return builder;
        }
    }
}