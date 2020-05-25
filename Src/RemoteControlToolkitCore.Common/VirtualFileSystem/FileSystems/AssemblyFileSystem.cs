using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    public class AssemblyFileSystem : NodeFunctionFileSystem<Assembly>
    {
        public AssemblyFileSystem()
        {
            FileSelector = assemblyName =>
            {
                return AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == assemblyName);
            };
            FolderSeed = () =>
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                return (assemblies, assemblies.Select(a => a.GetName().Name).ToArray());
            };
            FolderSelectionPredicate = (assemblies, path) =>
            {
                return assemblies.Any(a => a.GetName().Name == path.GetName());
            };
            Nodes.Add("fullName", (assembly, sw) => sw.WriteLine(assembly.GetName().FullName));
            Nodes.Add("version", (assembly, sw) => sw.WriteLine(assembly.GetName().Version));
            Nodes.Add("publicKeyToken", (assembly, sw) =>
            {
                sw.Write("[");
                foreach (byte b in assembly.GetName().GetPublicKeyToken())
                {
                    sw.Write(b);
                }
                sw.WriteLine("]");
            });
            Nodes.Add("publicKey", (assembly, sw) =>
            {
                sw.Write("[");
                foreach (byte b in assembly.GetName().GetPublicKey())
                {
                    sw.Write(b);
                }

                sw.WriteLine("]");
            });
            Nodes.Add("cultureName", (assembly, sw) => sw.WriteLine(assembly.GetName().CultureName));
            Nodes.Add("types", (assembly, sw) =>
            {
                foreach (Type t in assembly.GetTypes())
                {
                    sw.WriteLine(t.FullName);
                }
            });
            Nodes.Add("exportedTypes", (assembly, sw) =>
            {
                foreach (Type t in assembly.GetExportedTypes())
                {
                    sw.WriteLineAsync(t.FullName);
                }
            });
            Nodes.Add("attributes", (assembly, sw) =>
            {
                foreach (var t in assembly.GetCustomAttributes())
                {
                    sw.WriteLine(t);
                }
            });
            Nodes.Add("modules", (assembly, sw) =>
            {
                foreach (Module module in assembly.Modules)
                {
                    sw.WriteLine(module.Name);
                }
            });
            Nodes.Add("flags", (assembly, sw) => sw.WriteLine(assembly.GetName().Flags));
            Nodes.Add("location", (assembly, sw) => sw.WriteLine(assembly.Location));
        }
    }

    [Plugin]
    public class AssemblyFileSystemModule : PluginModule<FileSystemSubsystem>, IFileSystemPluginModule
    {
        public bool AutoMount => true;

        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return ("/assemblies", new AssemblyFileSystem());
        }

        public void UnMount(MountFileSystem mfs)
        {
            mfs.Unmount("/assemblies");
        }

        public void UnMount(UPath mountPoint, MountFileSystem mfs)
        {
            mfs.Unmount(mountPoint);
        }
    }
}