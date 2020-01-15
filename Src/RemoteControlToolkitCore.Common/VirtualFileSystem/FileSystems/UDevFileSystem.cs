using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Plugin;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    public class UDevFileSystem : FileSystem
    {
        private readonly IDeviceBus _bus;

        public UDevFileSystem(IDeviceBus bus)
        {
            _bus = bus;
        }
        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            if (path == "/") return true;
            else return _bus.CategoryExist(path.GetNameWithoutExtension());
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            throw new NotImplementedException();
        }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        protected override long GetFileLengthImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool FileExistsImpl(UPath path)
        {
            string category = path.GetDirectory().GetNameWithoutExtension();
            string device = path.GetNameWithoutExtension();
            if (string.IsNullOrEmpty(category))
            {
                return _bus.GetDeviceSelector("root").DeviceConnected(device);
            }
            else return _bus.GetDeviceSelector(category).DeviceConnected(device);
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteFileImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            string context = path.GetDirectory().GetNameWithoutExtension();
            string name = path.GetNameWithoutExtension();
            if (FileExists(path))
            {
                if (string.IsNullOrEmpty(context))
                {
                    return _bus.GetDeviceSelector("root").GetDevice(name).OpenDevice();
                }
                else
                {
                    return _bus.GetDeviceSelector(context).GetDevice(name).OpenDevice();
                }
            }
            else throw new FileNotFoundException();
        }

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            return FileAttributes.Device;
        }

        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetCreationTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            List<UPath> items = new List<UPath>();
            if (path == "/")
            {
                if(searchTarget == SearchTarget.Both || searchTarget == SearchTarget.Directory)
                    items.AddRange(_bus.GetAllModules().Select(v => new UPath($"/{v.Category}")));
                if (searchTarget == SearchTarget.Both || searchTarget == SearchTarget.File)
                {
                    items.AddRange(_bus.GetDeviceSelector("root").GetDevicesInfo().Select(v => new UPath($"/{v.FileName}")));
                }
            }
            else
            {
                return _bus.GetDeviceSelector(path.GetNameWithoutExtension()).GetDevicesInfo()
                    .Select(v => UPath.Combine(path, v.FileName));
            }

            return items;
        }

        protected override IFileSystemWatcher WatchImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override string ConvertPathToInternalImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override UPath ConvertPathFromInternalImpl(string innerPath)
        {
            throw new NotImplementedException();
        }
    }

    [PluginModule]
    public class UDevFileSystemModule : IFileSystemPluginModule
    {
        public bool AutoMount => true;
        private IDeviceBus _bus;
        public void InitializeServices(IServiceProvider kernel)
        {
            _bus = kernel.GetRequiredService<IDeviceBus>();
        }

        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return ("/dev", new UDevFileSystem(_bus));
        }

        public void UnMount(MountFileSystem mfs)
        {
            mfs.Unmount("/dev");
        }

        public void UnMount(UPath mountPoint, MountFileSystem mfs)
        {
            mfs.Unmount(mountPoint);
        }
    }
}