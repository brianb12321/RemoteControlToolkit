using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "wget")]
    [CommandHelp("Downloads resources from the network.")]
    public class WGetCommand :  RCTApplication
    {
        private IFileSystem _fileSystem;
        public override string ProcessName => "WGet";
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            bool showHelp = false;
            string location = string.Empty;
            string localPath = string.Empty;
            OptionSet options = new OptionSet()
                .Add("webLocation|w=", "The location to download from.", v => location = v)
                .Add("path|p=", "The local VFS path to store the downloaded resource.", v => localPath = v)
                .Add("showHelp|?", "Displays the help screen.", v => showHelp = true);
            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (showHelp)
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else
            {
                _fileSystem = context.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                context.Out.WriteLine("Beginning request.");
                downloadFileAsync(location, new Progress<double>((p) =>
                {
                    context.Out.Write($"\rWriting {p}%");
                }), token, localPath, context.Out).GetAwaiter().GetResult();
                context.Out.WriteLine($"\r\n{"Finished writing!!!".BrightGreen()}");
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
        }

        private async Task downloadFileAsync(string url, IProgress<double> progress, CancellationToken token,
            string localPath, TextWriter outWriter)
        {
            HttpClient client = new HttpClient();
            outWriter.WriteLine($"Retrieving HTTP response from url: \"{url}\"");
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"The request returned with HTTP status code {response.StatusCode}");
            }
            outWriter.WriteLine("Got response!");
            outWriter.WriteLine($"Response with HTTP code {response.StatusCode}.");
            outWriter.WriteLine("Reading response body...");
            using (var stream = await response.Content.ReadAsStreamAsync())
                using(var fileStream = _fileSystem.OpenFile(localPath, FileMode.Create, FileAccess.Write))
            {
                outWriter.WriteLine("Opened local VFS file for writing.");
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;
                do
                {
                    token.ThrowIfCancellationRequested();
                    var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);
                        await fileStream.WriteAsync(data, 0, data.Length, token);
                        await fileStream.FlushAsync(token);
                        totalRead += read;
                        progress.Report((totalRead * 1d) / (totalRead * 1d) * 100);
                    }
                } while (isMoreToRead);
                fileStream.Close();
            }
        }
        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}