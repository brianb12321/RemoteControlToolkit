using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using NDesk.Options;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

[assembly: PluginLibrary("NugetPackageManager", FriendlyName = "RCT Nuget Package Manager", LibraryType = NetworkSide.Server)]
namespace NugetPackageManager
{
    [PluginModule(Name = "pkg-install", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    [CommandHelp("Installs and manages packages.")]
    public class PkgInstallCommand : RCTApplication
    {
        public override string ProcessName { get; }

        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            context.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
            string mode = "showHelp";
            string package = string.Empty;
            string version = "1.0.0.0";
            string frameworkVersion = "net47";
            bool resolve = true;
            OptionSet options = new OptionSet()
                .Add("view", "View all packages from user-defined gallery locations.", v => mode = "view")
                .Add("dependency=", "Gets dependency data about the specified package. NOTE: A version must be supplied.", v =>
                {
                    mode = "dependency";
                    package = v;
                })
                .Add("dependencyGraph=", "Gets the entire dependency graph from the specified package id and version.",
                    v =>
                    {
                        mode = "dependencyGraph";
                        package = v;
                    })
                .Add("version|v=", "The version number to work with.", v => version = v)
                .Add("doNotResolve|r", "Do not use the NuGet resolver to remove duplicate dependencies", v => resolve = false)
                .Add("install=", "Install the specified package.", v =>
                {
                    mode = "install";
                    package = v;
                })
                .Add("showHelp|?", "Displays the help screen.", v => mode = "showHelp");

            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (mode == "showHelp")
            {
                options.WriteOptionDescriptions(context.Out);
            }
            else if (mode == "dependency")
            {
                PackageIdentity identity = new PackageIdentity(package, NuGetVersion.Parse(version));
                Logger logger = new Logger(context.Out);
                var settings = Settings.LoadDefaultSettings(null);
                var sourceRepositoryProvider = new SourceRepositoryProvider(new PackageSourceProvider(settings), Repository.Provider.GetCoreV3());
                var nuGetFramework = NuGetFramework.ParseFolder(frameworkVersion);
                using (var cacheContext = new SourceCacheContext() {DirectDownload = true})
                {
                    foreach (var sourceRepo in sourceRepositoryProvider.GetRepositories())
                    {
                        context.Out.WriteLine($"Resolving packages at source: {sourceRepo.PackageSource.Source}");
                        context.Out.WriteLine(getPackageDependencies(identity, nuGetFramework, cacheContext, logger, sourceRepo, token));
                    }
                }
            }
            else if (mode == "dependencyGraph")
            {
                PackageIdentity identity = new PackageIdentity(package, NuGetVersion.Parse(version));
                Logger logger = new Logger(context.Out);
                var settings = Settings.LoadDefaultSettings(null);
                var sourceRepositoryProvider = new SourceRepositoryProvider(new PackageSourceProvider(settings), Repository.Provider.GetCoreV3());
                var nuGetFramework = NuGetFramework.ParseFolder(frameworkVersion);
                using (var cacheContext = new SourceCacheContext() { DirectDownload = true })
                {
                    foreach (var sourceRepo in sourceRepositoryProvider.GetRepositories())
                    {
                        context.Out.WriteLine($"Resolving packages at source: {sourceRepo.PackageSource.Source}");
                        var available = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                        IEnumerable<SourcePackageDependencyInfo> packagesResolved = Enumerable.Empty<SourcePackageDependencyInfo>();
                        try
                        {
                            getPackageDependenciesRecursive(identity, nuGetFramework, cacheContext, logger, sourceRepo, available, token);
                            if (resolve)
                            {
                                var resolverContext = new PackageResolverContext(DependencyBehavior.Lowest,
                                    new[] { package },
                                    Enumerable.Empty<string>(),
                                    Enumerable.Empty<PackageReference>(),
                                    Enumerable.Empty<PackageIdentity>(),
                                    available,
                                    new[] { sourceRepo.PackageSource },
                                    logger);
                                var resolver = new PackageResolver();
                                packagesResolved = resolver.Resolve(resolverContext, token).Select(p => available.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning($"Package could not be found. Skipping source. Error: {e.Message}");
                            continue;
                        }

                        if (resolve)
                        {
                            context.Out.WriteLine("Packages resolved. These packages must be installed: ".BrightGreen());
                            for (int i = 0; i < packagesResolved.Count(); i++)
                            {
                                context.Out.WriteLine($"{i}: {packagesResolved.ElementAt(i)}");
                            }

                            break;
                        }
                        else
                        {
                            for (int i = 0; i < available.Count; i++)
                            {
                                context.Out.WriteLine($"{i}: {available.ElementAt(i)}");
                            }

                            break;
                        }
                    }
                }
            }
            else if (mode == "install")
            {
                PackageIdentity identity = new PackageIdentity(package, NuGetVersion.Parse(version));
                Logger logger = new Logger(context.Out);
                var settings = Settings.LoadDefaultSettings(null);
                var sourceRepositoryProvider = new SourceRepositoryProvider(new PackageSourceProvider(settings), Repository.Provider.GetCoreV3());
                var nuGetFramework = NuGetFramework.ParseFolder(frameworkVersion);
                using (var cacheContext = new SourceCacheContext() { DirectDownload = true })
                {
                    foreach (var sourceRepo in sourceRepositoryProvider.GetRepositories())
                    {
                        context.Out.WriteLine($"Resolving packages at source: {sourceRepo.PackageSource.Source}");
                        var available = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                        IEnumerable<SourcePackageDependencyInfo> packagesResolved = Enumerable.Empty<SourcePackageDependencyInfo>();
                        try
                        {
                            getPackageDependenciesRecursive(identity, nuGetFramework, cacheContext, logger, sourceRepo, available, token);
                            var resolverContext = new PackageResolverContext(DependencyBehavior.Lowest,
                                new[] { package },
                                Enumerable.Empty<string>(),
                                Enumerable.Empty<PackageReference>(),
                                Enumerable.Empty<PackageIdentity>(),
                                available,
                                new[] { sourceRepo.PackageSource },
                                logger);
                            var resolver = new PackageResolver();
                            packagesResolved = resolver.Resolve(resolverContext, token).Select(p => available.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning($"Package could not be found. Skipping source. Error: {e.Message}");
                            if (e.InnerException != null)
                            {
                                logger.LogWarning($"Inner Exception: {e.InnerException.Message}");
                            }
                            continue;
                        }
                        
                        context.Out.WriteLine("Packages resolved. These packages must be installed: ".BrightGreen());
                        for (int i = 0; i < packagesResolved.Count(); i++)
                        {
                            context.Out.WriteLine($"{i}: {packagesResolved.ElementAt(i)}");
                        }
                        context.Out.Write("Continue package installation [Yy/Nn]? ");
                        char answer = (char)context.In.Read();
                        if (answer == 'y' || answer == 'Y')
                        {
                            context.Out.WriteLine();
                            var packageExtractionContext = new PackageExtractionContext(
                                PackageSaveMode.Files,
                                XmlDocFileSaveMode.Skip, ClientPolicyContext.GetClientPolicy(settings, logger), logger);
                            var frameworkReducer = new FrameworkReducer();
                            foreach (var packageToInstall in packagesResolved)
                            {
                                var downloadResource = packageToInstall.Source.GetResource<DownloadResource>();
                                var downloadResult = downloadResource.GetDownloadResourceResultAsync(
                                    packageToInstall,
                                    new PackageDownloadContext(cacheContext),
                                    "packages",
                                    logger,
                                    token).GetAwaiter().GetResult();
                                var packagePathResolver = new PackagePathResolver(Path.GetFullPath($"extensions"));
                                PackageReaderBase packageReader;
                                var installedPath = packagePathResolver.GetInstalledPath(packageToInstall);
                                if (installedPath == null)
                                {
                                    packageReader = downloadResult.PackageReader;
                                }
                                else
                                {
                                    packageReader = new PackageFolderReader(installedPath);
                                }
                                PackageExtractor.ExtractPackageAsync(
                                    downloadResult.PackageSource,
                                    downloadResult.PackageStream,
                                    packagePathResolver,
                                    packageExtractionContext,
                                    token).GetAwaiter().GetResult();
                                context.Out.WriteLine($"Installation done: {packageToInstall.Id}!!".BrightGreen());
                                downloadResult.Dispose();
                                var libItems = packageReader.GetLibItems();
                                var nearest = frameworkReducer.GetNearest(nuGetFramework,
                                    libItems.Select(x => x.TargetFramework));
                                context.Out.WriteLine($"Found libraries in package {packageToInstall.Id} that match nearest version {frameworkVersion}");
                                context.Out.WriteLine(string.Join("\r\n", libItems.Where(x => x.TargetFramework.Equals(nearest)).SelectMany(x => x.Items)));
                                var frameworkItems = packageReader.GetFrameworkItems();
                                nearest = frameworkReducer.GetNearest(nuGetFramework,
                                    frameworkItems.Select(x => x.TargetFramework));
                                context.Out.WriteLine($"Found framework items in package {packageToInstall.Id} that match nearest version {frameworkVersion}");
                                context.Out.WriteLine(string.Join("\r\n", frameworkItems.Where(x => x.TargetFramework.Equals(nearest)).SelectMany(x => x.Items)));
                            }
                            context.Out.WriteLine($"The server application must be restarted for the changes to take effect.".BrightYellow());
                            break;
                        }
                        else
                        {
                            context.Out.WriteLine();
                            context.Out.WriteLine("Installation cancelled.");
                            break;
                        }
                    }
                }
            }

            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        private SourcePackageDependencyInfo getPackageDependencies(PackageIdentity identity, NuGetFramework framework,
            SourceCacheContext cache, ILogger logger, SourceRepository repo, CancellationToken token)
        {
            var dependencyInfoResource = repo.GetResource<DependencyInfoResource>();
            return dependencyInfoResource.ResolvePackage(identity, framework, cache, logger, token).GetAwaiter().GetResult();
        }
        private void getPackageDependenciesRecursive(PackageIdentity identity, NuGetFramework framework,
            SourceCacheContext cache, ILogger logger, SourceRepository repo,
            ISet<SourcePackageDependencyInfo> available, CancellationToken token)
        {
            if (available.Contains(identity)) return;
            var dependencyInfo = getPackageDependencies(identity, framework, cache, logger, repo, token);
            if (dependencyInfo == null) return;
            available.Add(dependencyInfo);
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                getPackageDependenciesRecursive(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), framework, cache, logger, repo, available, token);
            }
        }
        public override void InitializeServices(IServiceProvider kernel)
        {
        }

        private class Logger : ILogger
        {
            private TextWriter _textWriter;

            public Logger(TextWriter textWriter)
            {
                _textWriter = textWriter;
            }
            public void LogDebug(string data)
            {
                _textWriter.WriteLine($"DEBUG: {data}");
            }

            public void LogVerbose(string data)
            {
                _textWriter.WriteLine($"VERBOSE: {data}".BrightBlue());
            }

            public void LogInformation(string data)
            {
                _textWriter.WriteLine($"INFO: {data}");
            }

            public void LogMinimal(string data)
            {
                _textWriter.WriteLine(data);
            }

            public void LogWarning(string data)
            {
                _textWriter.WriteLine($"WARNING: {data}".BrightYellow());
            }

            public void LogError(string data)
            {
                _textWriter.WriteLine($"ERROR: {data}".Red());
            }

            public void LogInformationSummary(string data)
            {
                _textWriter.WriteLine($"SUMMARY: {data}".BrightGreen());
            }

            public void Log(LogLevel level, string data)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        LogDebug(data);
                        break;
                    case LogLevel.Error:
                        LogError(data);
                        break;
                    case LogLevel.Information:
                        LogInformation(data);
                        break;
                    case LogLevel.Minimal:
                        LogMinimal(data);
                        break;
                    case LogLevel.Verbose:
                        LogVerbose(data);
                        break;
                    case LogLevel.Warning:
                        LogWarning(data);
                        break;
                }
            }

            public Task LogAsync(LogLevel level, string data)
            {
                Log(level, data);
                return Task.CompletedTask;
            }

            public void Log(ILogMessage message)
            {
                Log(message.Level, message.Message);
            }

            public Task LogAsync(ILogMessage message)
            {
                Log(message);
                return Task.CompletedTask;
            }
        }
    }
}