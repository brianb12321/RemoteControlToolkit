using System.Diagnostics;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    [Plugin(PluginName = "External")]
    public class ExternalApplicationProcessFactory : PluginModule<ProcessFactorySubsystem>, IProcessFactory
    {
        public IProcessBuilder CreateProcessBuilder(CommandRequest request, RctProcess parentProcess, IProcessTable table)
        {
            Process process = new Process();
            IProcessBuilder builder = table.CreateProcessBuilder()
                .SetProcessName(request.Arguments[0])
                .SetParent(parentProcess)
                .SetAction((current, token) =>
                {
                    process.StartInfo.FileName = request.Arguments[0];
                    process.StartInfo.Arguments = request.GetArguments();
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