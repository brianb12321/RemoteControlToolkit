using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    public class UDevFileSystem : FileSystem
    {
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
            string file = path.GetNameWithoutExtension();
            if (file == "null") return true;
            else return false;
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
            if (path == "/null") return Stream.Null;
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
            if (path == "/")
            {
                return new List<UPath>()
                {
                    "/null"
                };
            }
            else throw new DirectoryNotFoundException();
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
        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return ("/dev", new UDevFileSystem());
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