using RemoteControlToolkitCore.Common.Scripting;

namespace RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements
{
    public class ScriptCommandElement : BaseCommandElement
    {
        private IScriptingEngine _engine;
        private IScriptExecutionContext _context;

        public ScriptCommandElement(string value, IScriptingEngine engine, IScriptExecutionContext context)
        {
            _context = context;
            Value = value;
            _engine = engine;
        }
        public override string ToStringImpl()
        {
            return _engine.ExecuteString<dynamic>(Value.ToString(), _context).ToString();
        }
    }
}