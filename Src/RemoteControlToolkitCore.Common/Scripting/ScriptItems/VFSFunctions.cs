using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [Plugin]
    public class VFSFunctions : PluginModule<ScriptingSubsystem>, IScriptExtensionModule
    {
        private IScriptingEngine _currentEngine;

        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            _currentEngine = engine;
            var context = engine.CreateModule("vfs");
            context.AddVariable("open_vfs", new Func<string, FileMode, FileAccess, Stream>(openVFS));
        }

        private Stream openVFS(string file, FileMode mode, FileAccess access)
        {
            var fileSystem = _currentEngine.ParentProcess.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
            return fileSystem.OpenFile(file, mode, access);
        }
    }
}