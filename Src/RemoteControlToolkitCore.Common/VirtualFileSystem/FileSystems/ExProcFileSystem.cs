using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    public class ExProcFileSystem : FileSystem
    {
        private readonly Dictionary<string, Action<Process, StreamWriter>> _functions;

        public ExProcFileSystem()
        {
            _functions = new Dictionary<string, Action<Process, StreamWriter>>
            {
                {"priority", (p, w) => w.WriteLine(p.BasePriority.ToString())},
                {"exited", (p, w) => w.WriteLine(p.HasExited)},
                {"name", (p, w) => w.WriteLine(p.ProcessName)},
                {"machineName", (p, w) => w.WriteLine(p.MachineName)},
                {
                    "startTime",
                    (p, w) => w.WriteLine($"{p.StartTime.ToShortDateString()} - {p.StartTime.ToShortTimeString()}")
                },
                {"handle", (p, w) => w.WriteLine(p.Handle.ToString("X8"))},
                {"mainWindowHandle", (p, w) => w.WriteLine(p.MainWindowHandle.ToString("X8"))},
                {"handleCount", (p, w) => w.WriteLine(p.HandleCount)},
                {
                    "modules", (p, w) =>
                    {
                        foreach (ProcessModule module in p.Modules)
                        {
                            w.WriteLine($"{module.FileName}: 0x{module.BaseAddress.ToString("X8")}");
                        }
                    }
                },
                {
                    "threads", (p, w) =>
                    {
                        foreach (ProcessThread thread in p.Threads)
                        {
                            w.WriteLine($"{thread.Id}: {thread.ThreadState}");
                        }
                    }
                },
                {"windowTitle", (p, w) => w.WriteLine(p.MainWindowTitle)}
            };
        }
        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            if (path == "/") return true;
            if (int.TryParse(path.GetNameWithoutExtension(), out int id))
            {
                Process[] exProcs = Process.GetProcesses();
                if (exProcs.Any(p => p.Id == id)) return true;
                else return false;
            }
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
            string process = path.GetDirectory().FullName;
            if (DirectoryExistsImpl(process))
            {
                string fileName = path.GetNameWithoutExtension();
                return _functions.ContainsKey(fileName);
            }
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
            if (FileExistsImpl(path))
            {
                int id = int.Parse(path.GetDirectory().GetName());
                string file = path.GetNameWithoutExtension();
                Process p = Process.GetProcessById(id);
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms) {AutoFlush = true};
                _functions[file](p, sw);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            else throw new FileNotFoundException();
        }

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            return FileAttributes.System;
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
            List<UPath> paths = new List<UPath>();
            if (path == "/")
            {
                foreach (Process p in Process.GetProcesses())
                {
                    paths.Add($"/{p.Id}");
                }

                return paths;
            }
            else
            {
                foreach (string file in _functions.Keys)
                {
                    paths.Add(UPath.Combine(path, file));
                }

                return paths;
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
    public class ExProcFileSystemModule : PluginModule<FileSystemSubsystem>, IFileSystemPluginModule
    {
        public bool AutoMount => true;
        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return ("/exproc", new ExProcFileSystem());
        }

        public void UnMount(MountFileSystem mfs)
        {
            mfs.Unmount("/exproc");
        }

        public void UnMount(UPath mountPoint, MountFileSystem mfs)
        {
            mfs.Unmount(mountPoint);
        }
    }
}