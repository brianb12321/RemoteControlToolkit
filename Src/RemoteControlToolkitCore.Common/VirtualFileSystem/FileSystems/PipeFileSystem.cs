using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Modules;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    /// <summary>
    /// Allows access all opened operating system pipes.
    /// </summary>
    public class PipeFileSystem : FileSystem
    {
        private IPipeService _pipeService;

        private enum PipeKind
        {
            AnonymousServer,
            AnonymousClient,
            NamedServer,
            NamedClient
        }

        private Dictionary<PipeKind, Func<UPath, IEnumerable<UPath>>> _pipeFunctions;

        public PipeFileSystem(IPipeService service)
        {
            _pipeService = service;
            _pipeFunctions = new Dictionary<PipeKind, Func<UPath, IEnumerable<UPath>>>();
            _pipeFunctions.Add(PipeKind.AnonymousServer, (path) =>
            {
                List<UPath> pipes = new List<UPath>();
                for (int i = 0; i < _pipeService.GetServerAnonymousPipes().Length; i++)
                {
                    pipes.Add(UPath.Combine(path, $"{i}"));
                }

                return pipes;
            });
            _pipeFunctions.Add(PipeKind.AnonymousClient, (path) =>
            {
                List<UPath> pipes = new List<UPath>();
                for (int i = 0; i < _pipeService.GetClientAnonymousPipes().Length; i++)
                {
                    pipes.Add(UPath.Combine(path, $"{i}"));
                }

                return pipes;
            });
            _pipeFunctions.Add(PipeKind.NamedServer, (path) =>
            {
                List<UPath> pipes = new List<UPath>();
                for (int i = 0; i < _pipeService.GetServerNamedPipes().Length; i++)
                {
                    pipes.Add(UPath.Combine(path, $"{i}"));
                }

                return pipes;
            });
            _pipeFunctions.Add(PipeKind.NamedClient, (path) =>
            {
                List<UPath> pipes = new List<UPath>();
                for (int i = 0; i < _pipeService.GetClientNamedPipes().Length; i++)
                {
                    pipes.Add(UPath.Combine(path, $"{i}"));
                }

                return pipes;
            });
        }

        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            if (path == UPath.Root) return true;
            else if (path == "/as" || path == "/ac" || path == "/ns" || path == "/nc") return true;
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

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath,
            bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        protected override long GetFileLengthImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool FileExistsImpl(UPath path)
        {
            switch (path.GetDirectory().FullName)
            {
                case "/as":
                    return _pipeService.AnonymousServerPipeExists(int.Parse(path.GetNameWithoutExtension()));
                case "/ac":
                    return _pipeService.AnonymousClientPipeExists(int.Parse(path.GetNameWithoutExtension()));
                case "/ns":
                    return _pipeService.NamedServerPipeExists(int.Parse(path.GetNameWithoutExtension()));
                case "/nc":
                    return _pipeService.NamedClientPipeExists(int.Parse(path.GetNameWithoutExtension()));
                default:
                    return false;
            }
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteFileImpl(UPath path)
        {

        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            if (FileExists(path))
            {
                var directory = path.GetDirectory();
                int pipeNumber = int.Parse(path.GetNameWithoutExtension());
                if (directory == "/as") return new UnClosableStream(_pipeService.GetAnonymousPipeServer(pipeNumber));
                else if (directory == "/ac") return new UnClosableStream(_pipeService.GetAnonymousPipeClient(pipeNumber));
                else if (directory == "/ns") return new UnClosableStream(_pipeService.GetNamedPipeServer(pipeNumber));
                else if (directory == "/nc") return new UnClosableStream(_pipeService.GetNamedPipeClient(pipeNumber));
                else throw new DirectoryNotFoundException();
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

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern,
            SearchOption searchOption, SearchTarget searchTarget)
        {
            if (path == "/")
            {
                return new List<UPath>()
                {
                    new UPath("/as"),
                    new UPath("/ac"),
                    new UPath("/ns"),
                    new UPath("/nc")
                };
            }
            else if(path == "/as") return _pipeFunctions[PipeKind.AnonymousServer](path);
            else if(path == "/ac") return _pipeFunctions[PipeKind.AnonymousClient](path);
            else if(path == "/ns") return _pipeFunctions[PipeKind.NamedServer](path);
            else if(path == "/nc") return _pipeFunctions[PipeKind.NamedClient](path);
            else return Enumerable.Empty<UPath>();
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

    public class UnClosableStream : Stream
    {
        private Stream _baseStream;

        public UnClosableStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }
        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override void Close()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }
    }

    [Plugin]
    public class PipeFileSystemModule : PluginModule<FileSystemSubsystem>, IFileSystemPluginModule
    {
        private IPipeService _service;
        public override void InitializeServices(IServiceProvider kernel)
        {
            _service = kernel.GetService<IPipeService>();
        }

        public bool AutoMount => true;
        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return (new UPath("/ospipes"), new PipeFileSystem(_service));
        }

        public void UnMount(MountFileSystem mfs)
        {
            mfs.Unmount("/ospipes");
        }

        public void UnMount(UPath mountPoint, MountFileSystem mfs)
        {
            mfs.Unmount(mountPoint);
        }
    }
}