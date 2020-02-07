using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class FileSystemHelper : IFileSystem
    {
        public IFileSystem _fileSystem;
        public UPath _workingPath;
        public FileSystemHelper(IFileSystem fileSystem, UPath workingPath)
        {
            _fileSystem = fileSystem;
            _workingPath = workingPath;
        }
        public void CreateDirectory(UPath path)
        {
            _fileSystem.CreateDirectory(UPath.Combine(_workingPath, path));
        }

        public bool DirectoryExists(UPath path)
        {
            return _fileSystem.DirectoryExists(UPath.Combine(_workingPath, path));
        }

        public void MoveDirectory(UPath srcPath, UPath destPath)
        {
            _fileSystem.MoveDirectory(UPath.Combine(_workingPath, srcPath), destPath);
        }

        public void DeleteDirectory(UPath path, bool isRecursive)
        {
            _fileSystem.DeleteDirectory(UPath.Combine(_workingPath, path), isRecursive);
        }

        public void CopyFile(UPath srcPath, UPath destPath, bool overwrite)
        {
            _fileSystem.CopyFile(UPath.Combine(_workingPath, srcPath), destPath, overwrite);
        }

        public void ReplaceFile(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
        {
            _fileSystem.ReplaceFile(UPath.Combine(_workingPath, srcPath), destPath, destBackupPath, ignoreMetadataErrors);
        }

        public long GetFileLength(UPath path)
        {
            return _fileSystem.GetFileLength(UPath.Combine(_workingPath, path));
        }

        public bool FileExists(UPath path)
        {
            return _fileSystem.FileExists(UPath.Combine(_workingPath, path));
        }

        public void MoveFile(UPath srcPath, UPath destPath)
        {
            _fileSystem.MoveFile(UPath.Combine(_workingPath, srcPath), destPath);
        }

        public void DeleteFile(UPath path)
        {
            _fileSystem.DeleteFile(UPath.Combine(_workingPath, path));
        }

        public Stream OpenFile(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            return _fileSystem.OpenFile(UPath.Combine(_workingPath, path), mode, access, share);
        }

        public FileAttributes GetAttributes(UPath path)
        {
            return _fileSystem.GetAttributes(UPath.Combine(_workingPath, path));
        }

        public void SetAttributes(UPath path, FileAttributes attributes)
        {
            _fileSystem.SetAttributes(UPath.Combine(_workingPath, path), attributes);
        }

        public DateTime GetCreationTime(UPath path)
        {
            return _fileSystem.GetCreationTime(UPath.Combine(_workingPath, path));
        }

        public void SetCreationTime(UPath path, DateTime time)
        {
            _fileSystem.SetCreationTime(UPath.Combine(_workingPath, path), time);
        }

        public DateTime GetLastAccessTime(UPath path)
        {
            return _fileSystem.GetLastAccessTime(UPath.Combine(_workingPath, path));
        }

        public void SetLastAccessTime(UPath path, DateTime time)
        {
            _fileSystem.SetLastAccessTime(UPath.Combine(_workingPath, path), time);
        }

        public DateTime GetLastWriteTime(UPath path)
        {
            return _fileSystem.GetLastWriteTime(UPath.Combine(_workingPath, path));
        }

        public void SetLastWriteTime(UPath path, DateTime time)
        {
            _fileSystem.SetLastWriteTime(UPath.Combine(_workingPath, path), time);
        }

        public IEnumerable<UPath> EnumeratePaths(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            return _fileSystem.EnumeratePaths(UPath.Combine(_workingPath, path), searchPattern, searchOption, searchTarget);
        }

        public bool CanWatch(UPath path)
        {
            return _fileSystem.CanWatch(UPath.Combine(_workingPath, path));
        }

        public IFileSystemWatcher Watch(UPath path)
        {
            return _fileSystem.Watch(UPath.Combine(_workingPath, path));
        }

        public string ConvertPathToInternal(UPath path)
        {
            return _fileSystem.ConvertPathToInternal(UPath.Combine(_workingPath, path));
        }

        public UPath ConvertPathFromInternal(string innerPath)
        {
            return _fileSystem.ConvertPathFromInternal(innerPath);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }
    }
}