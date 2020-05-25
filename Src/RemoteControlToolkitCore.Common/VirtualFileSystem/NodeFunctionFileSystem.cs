using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public delegate (TNodeType[] node, string[] stringNode) FolderSeedDelegate<TNodeType>();
    public abstract class NodeFunctionFileSystem<TNodeType> : FileSystem
    {
        public Func<string, TNodeType> FileSelector { get; protected set; }
        public FolderSeedDelegate<TNodeType> FolderSeed { get; protected set; }
        public Func<TNodeType[], UPath, bool> FolderSelectionPredicate { get; protected set; }
        public Dictionary<string, Action<TNodeType, StreamWriter>> Nodes { get; protected set; }

        public NodeFunctionFileSystem()
        {
            Nodes = new Dictionary<string, Action<TNodeType, StreamWriter>>();
        }
        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            if (path == "/") return true;
            TNodeType[] nodes = FolderSeed().node;
            if (FolderSelectionPredicate(nodes, path)) return true;
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
                return Nodes.ContainsKey(fileName);
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
                string parent = path.GetDirectory().GetName();
                string file = path.GetNameWithoutExtension();
                TNodeType node = FileSelector(parent);
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.AutoFlush = true;
                Nodes[file](node, sw);
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
                foreach (string node in FolderSeed().stringNode)
                {
                    paths.Add($"/{node}");
                }

                return paths;
            }
            else
            {
                foreach (string file in Nodes.Keys)
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
}