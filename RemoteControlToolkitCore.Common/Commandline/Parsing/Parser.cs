using System;
using System.Collections.Generic;
using System.Linq;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements;
using RemoteControlToolkitCore.Common.Scripting;

namespace RemoteControlToolkitCore.Common.Commandline.Parsing
{
    public class Parser : IParser
    {
        private readonly IScriptingEngine _scriptingEngine;
        private readonly IScriptExecutionContext _context;
        private readonly IReadOnlyDictionary<string, string> _envVars;

        public Parser(IScriptingEngine engine, IScriptExecutionContext context, IReadOnlyDictionary<string, string> envVars)
        {
            _context = context;
            _scriptingEngine = engine;
            _envVars = envVars;
        }

        public RedirectionMode OutputRedirected { get; private set; } = RedirectionMode.None;
        public bool OutputAppendMode { get; private set; }
        public RedirectionMode ErrorRedirected { get; private set; } = RedirectionMode.None;
        public bool ErrorAppendMode { get; private set; }
        public RedirectionMode InputRedirected { get; private set; } = RedirectionMode.None;
        public string Input { get; private set; }
        public string Output { get; private set; }
        public string Error { get; private set; }

        public IReadOnlyList<IReadOnlyList<ICommandElement>> Parse(IReadOnlyList<CommandToken> tokens)
        {
            List<List<ICommandElement>> _parsedElements = new List<List<ICommandElement>>();
            List<ICommandElement> _elements = new List<ICommandElement>(tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].Type)
                {
                    case TokenType.Word:
                        if (i == 0)
                        {
                            _elements.Add(new CommandNameCommandElement(tokens[i].Value));
                        }
                        else
                        {
                            _elements.Add(new StringCommandElement(tokens[i].Value));
                        }
                        break;
                    case TokenType.Quote:
                        _elements.Add(new StringCommandElement(tokens[i].Value));
                        break;
                    case TokenType.Script:
                        _elements.Add(new ScriptCommandElement(tokens[i].ToString(), _scriptingEngine, _context));
                        break;
                    case TokenType.EnvironmentVariable:
                        string value = tokens[i].Value.Substring(1);
                        if (!_envVars.ContainsKey(value))
                        {
                            throw new ParserException($"Environment variable \"{value}\" does not exist.");
                        }
                        _elements.Add(new StringCommandElement(_envVars[value]));
                        break;
                    case TokenType.OutRedirect:
                        if (tokens[i].Value.StartsWith("$"))
                        {
                            OutputRedirected = RedirectionMode.VFS;
                            Output = tokens[i].Value.Substring(1);
                            i++;
                        }
                        else
                        {
                            OutputRedirected = RedirectionMode.File;
                            Output = tokens[i].Value;
                            i++;
                        }
                        break;
                    case TokenType.AppendOutRedirect:
                        if (tokens[i + 1].Value.StartsWith("$"))
                        {
                            OutputRedirected = RedirectionMode.VFS;
                            Output = tokens[i].Value.Substring(1);
                            OutputAppendMode = true;
                            i++;
                        }
                        else
                        {
                            OutputRedirected = RedirectionMode.File;
                            Output = tokens[i].Value;
                            OutputAppendMode = true;
                            i++;
                        }
                        break;
                    case TokenType.InRedirect:
                        if (tokens[i + 1].Value.StartsWith("$"))
                        {
                            InputRedirected = RedirectionMode.VFS;
                            Input = tokens[i].Value.Substring(1);
                            i++;
                        }
                        else
                        {
                            InputRedirected = RedirectionMode.File;
                            Input = tokens[i].Value;
                            i++;
                        }
                        break;
                    case TokenType.Pipe:
                        _parsedElements.Add(new List<ICommandElement>(_elements));
                        _parsedElements.Add(new List<ICommandElement>() { new PipeCommandElement() });
                        _elements.Clear();
                        break;
                }
            }
            //Copy left-over command elements to the list.
            _parsedElements.Add(new List<ICommandElement>(_elements));
            //Create Linked List
            foreach (var elements in _parsedElements)
            {
                ICommandElement current = elements[0];
                if (elements.Count > 1)
                {
                    elements[0].Next = elements[1];
                    for (int i = 1; i < elements.Count - 1; i++)
                    {
                        elements[i].Previous = current;
                        elements[i].Next = elements[i + 1];
                        current = elements[i];
                        elements[elements.Count - 1].Previous = elements[elements.Count - 2];
                    }
                }
            }
            return _parsedElements;
        }
    }
}