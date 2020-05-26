using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    /// <summary>
    /// Provides access to all installed applications.
    /// </summary>
    public class BinFileSystem : FileSystem
    {
        private readonly ApplicationSubsystem _appSubsystem;

        public BinFileSystem(ApplicationSubsystem subsystem)
        {
            _appSubsystem = subsystem;
        }

        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            if (path == "/") return true;
            else return false;
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
            string programName = path.GetName();
            return _appSubsystem.ApplicationExists(programName);
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
            throw new NotImplementedException();
        }

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            return FileAttributes.Normal;
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
            if (path == "/")
            {
                foreach (PluginAttribute attribute in _appSubsystem.GetAllInstalledApplications())
                {
                    yield return $"/{attribute.PluginName}";
                }
            }
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

    [Plugin]
    public class BinFileSystemModule : PluginModule<FileSystemSubsystem>, IFileSystemPluginModule
    {
        public bool AutoMount => true;
        private ApplicationSubsystem _appSubsystem;

        public override void InitializeServices(IServiceProvider provider)
        {
            _appSubsystem = provider.GetService<ApplicationSubsystem>();
        }

        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return ("/bin", new BinFileSystem(_appSubsystem));
        }

        public void UnMount(MountFileSystem mfs)
        {
            mfs.Unmount("/bin");
        }

        public void UnMount(UPath mountPoint, MountFileSystem mfs)
        {
            mfs.Unmount(mountPoint);
        }
    }
}