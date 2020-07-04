using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using ThreadState = System.Threading.ThreadState;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public delegate CommandResponse ProcessDelegate(RctProcess current, CancellationToken token);
    public class RctProcess : IExtensibleObject<RctProcess>
    {
        public uint Pid { get; set; }
        public bool IsBackground { get; set; }
        public bool InRedirected { get; private set; }
        public bool OutRedirected { get; private set; }
        public bool ErrorRedirected { get; private set; }
        public IPrincipal Identity { get; }

        public IInstanceSession ClientContext { get; set; }
        public StreamWriter Out { get; private set; }
        private Stream _outStream;
        public StreamWriter Error { get; private set; }
        private Stream _errorStream;
        public TextReader In { get; private set; }
        private Stream _inStream;
        public bool DisposeIn { get; set; } = true;
        public bool DisposeOut { get; set; } = true;
        public bool DisposeError { get; set; } = true;

        public UPath WorkingDirectory
        {
            get => EnvironmentVariables["WORKINGDIR"].Value.ToString();
            set => EnvironmentVariables["WORKINGDIR"] = new EnvironmentVariable("WORKINGDIR", value);
        } 
        public IExtensionCollection<RctProcess> Extensions { get; }
        public bool Running => State == ThreadState.Running;
        public ThreadState State => _workingThread.ThreadState;
        public string Name { get; }
        public RctProcess Parent { get; }
        public RctProcess Child { get; set; }
        public event EventHandler<Exception> ThreadError;
        public event EventHandler<ControlCEventArgs> ControlC;
        public event EventHandler StandardOutDisposed;
        public event EventHandler StandardInDisposed;
        public event EventHandler StandardErrorDisposed;
        public event EventHandler ProcessFinished;
        private readonly Thread _workingThread;
        private readonly CancellationTokenSource _cts;
        private readonly ProcessDelegate _threadStart;
        public CommandResponse ExitCode { get; private set; }
        public EnvironmentVariableCollection EnvironmentVariables { get; }
        public bool Disposed { get; private set; }
        private readonly IProcessTable _table;
        private readonly IExtensionProvider<RctProcess>[] _extensionProviders;

        private RctProcess(IProcessTable table,
            IInstanceSession session,
            string name,
            RctProcess parent,
            ProcessDelegate threadStart,
            IPrincipal identity,
            IExtensionProvider<RctProcess>[] providers,
            ApartmentState apartmentState)
        {
            _table = table;
            Name = name;
            Parent = parent;
            _threadStart = threadStart;
            ClientContext = session;
            Pid = _table.LatestProcess + 1;
            Extensions = new ExtensionCollection<RctProcess>(this);
            _extensionProviders = providers;
            populateExtension();
            //Populate Extensions
            EnvironmentVariables = new EnvironmentVariableCollection();
            Identity = identity;
            if (Parent != null)
            {
                Parent.Child = this;
                Out = Parent.Out;
                In = Parent.In;
                Error = Parent.Error;
                _outStream = Parent._outStream;
                _errorStream = Parent._errorStream;
                _inStream = Parent._inStream;
                Identity = Parent.Identity;
                ClientContext = Parent.ClientContext;
                StandardOutDisposed += Parent.StandardOutDisposed;
                StandardInDisposed += Parent.StandardInDisposed;
                StandardErrorDisposed += Parent.StandardErrorDisposed;
                InRedirected = Parent.InRedirected;
                OutRedirected = Parent.OutRedirected;
                ErrorRedirected = Parent.ErrorRedirected;
                DisposeIn = Parent.DisposeIn;
                DisposeOut = Parent.DisposeOut;
                DisposeError = Parent.DisposeError;
                IsBackground = Parent.IsBackground;
                EnvironmentVariables = new EnvironmentVariableCollection();
                //Determine inherit-ability.
                foreach(EnvironmentVariable variable in Parent.EnvironmentVariables.Values)
                {
                    if(variable.Inheritable) EnvironmentVariables.Add(variable.Name, variable);
                }
            }

            _workingThread = new Thread(startThread);
            SetApartmentState(apartmentState);
            _cts = new CancellationTokenSource();
        }

        private void populateExtension()
        {
            foreach (IExtensionProvider<RctProcess> provider in _extensionProviders)
            {
                provider.GetExtension(this);
            }
        }

        public void SetApartmentState(ApartmentState state)
        {
            _workingThread.SetApartmentState(state);
        }
        public void Start()
        {
            if (IsBackground)
            {
                SetIn(Stream.Null);
                SetOut(Stream.Null);
                SetError(Stream.Null);
            }

            //Dispose process once finished regardless of errors.
            ProcessFinished += (sender, e) => Dispose();
            
            _table.AddProcess(this);
            _workingThread.Start(this);
        }
        public void InvokeControlC()
        {
            if(ControlC == null)
            {
                Close();
            }
            else
            {
                ControlCEventArgs args = new ControlCEventArgs();
                ControlC?.Invoke(this, args);
                if (args.CloseProcess) Close();
            }
        }

        private void startThread(object data)
        {
            try
            {
                Thread.CurrentPrincipal = Identity;
                ExitCode = _threadStart?.Invoke((RctProcess) data, _cts.Token);
            }
            catch (ThreadAbortException)
            {
                ExitCode = new CommandResponse(CommandResponse.CODE_THREAD_ABORT);
            }
            catch (Exception ex)
            {
                ThreadError?.Invoke(this, ex);
                ExitCode = new CommandResponse(CommandResponse.CODE_THREAD_ABORT);
            }
            finally
            {
                ProcessFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        public void WaitForExit()
        {
            _workingThread.Join();
        }

        public void Abort()
        {
            //Will break under .NET Core
            Child?.Abort();
            _workingThread?.Abort();
            Dispose();
        }

        public void Close()
        {
            Child?.Close();
            Dispose();
        }

        public Stream OpenOutputStream()
        {
            return _outStream;
        }

        public Stream OpenErrorStream()
        {
            return _errorStream;
        }

        public Stream OpenInputStream()
        {
            return _inStream;
        }

        StreamWriter configureStreamWriter(Stream input)
        {
            //We do not want the preamble to be written to the underlying stream.
            return new StreamWriter(input, new UTF8Encoding(false, false), 1, true)
                {AutoFlush = true};
        }

        public void SetOut(Stream outStream)
        {
            if (_outStream != null) OutRedirected = true;
            _outStream = outStream;
            Out = configureStreamWriter(outStream);
            Out.AutoFlush = true;
        }
        public void SetOut(StreamWriter outStream)
        {
            if (_outStream != null) OutRedirected = true;
            _outStream = outStream.BaseStream;
            Out = outStream;
        }

        public void SetError(Stream errorStream)
        {
            if (_errorStream != null) ErrorRedirected = true;
            _errorStream = errorStream;
            Error = configureStreamWriter(errorStream);
            Error.AutoFlush = true;
        }
        public void SetError(StreamWriter errorStream)
        {
            if (_errorStream != null) ErrorRedirected = true;
            _errorStream = errorStream.BaseStream;
            Error = errorStream;
        }

        public void SetIn(Stream inStream)
        {
            if (_inStream != null) InRedirected = true;
            _inStream = inStream;
            In = new StreamReader(_inStream);
        }
        public void SetIn(StreamReader inStream)
        {
            if (_inStream != null) InRedirected = true;
            _inStream = inStream.BaseStream;
            In = inStream;
        }
        public void SetIn(TextReader inReader, Stream inStream)
        {
            if (_inStream != null) InRedirected = true;
            //Currently reading from the terminal cannot be accomplished by a stream.
            _inStream = inStream;
            In = inReader;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (_workingThread.IsAlive)
                {
                    try
                    {
                        _cts?.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        
                    }
                }
                _cts?.Dispose();
                if (DisposeIn)
                {
                    In?.Close();
                    StandardInDisposed?.Invoke(this, EventArgs.Empty);
                }

                if (DisposeOut)
                {
                    Out?.Close();
                    StandardOutDisposed?.Invoke(this, EventArgs.Empty);
                }

                if (DisposeError)
                {
                    Error?.Close();
                    StandardErrorDisposed?.Invoke(this, EventArgs.Empty);
                }
                foreach (IExtensionProvider<RctProcess> provider in _extensionProviders)
                {
                    provider.RemoveExtension(this);
                }
                Child?.Dispose();

                //Check if process is already removed from table.

                if (_table.ProcessExists(Pid))
                {
                    _table.RemoveProcess(Pid);
                }

                Disposed = true;
            }
        }


        public class RctProcessBuilder : IProcessBuilder
        {
            private readonly IProcessTable _table;
            private readonly IServiceProvider _provider;
            private string _processName;
            private ProcessDelegate _action;
            private RctProcess _parent;
            private IPrincipal _principal;
            private IInstanceSession _session;
            private ApartmentState _apartmentState = ApartmentState.Unknown;
            private readonly List<IExtensionProvider<RctProcess>> _extensions;
            public RctProcessBuilder(IProcessTable table)
            {
                _table = table;
                _extensions = new List<IExtensionProvider<RctProcess>>();
            }

            public RctProcess Build()
            {
                return new RctProcess(_table,
                    _parent?.ClientContext ?? _session,
                    _processName,
                    _parent,
                    _action,
                    _parent?.Identity ?? _principal, 
                    _extensions.ToArray(), _apartmentState);
            }

            public IProcessBuilder SetProcessName(string name)
            {
                _processName = name;
                return this;
            }

            public IProcessBuilder SetAction(ProcessDelegate action)
            {
                _action = action;
                return this;
            }

            public IProcessBuilder SetParent(RctProcess process)
            {
                _parent = process;
                return this;
            }

            public IProcessBuilder SetSecurityPrincipal(IPrincipal principal)
            {
                _principal = principal;
                return this;
            }

            public IProcessBuilder SetInstanceSession(IInstanceSession session)
            {
                _session = session;
                return this;
            }

            public IProcessBuilder SetThreadApartmentMode(ApartmentState mode)
            {
                _apartmentState = mode;
                return this;
            }

            public IProcessBuilder AddProcessExtension(IExtensionProvider<RctProcess> extension)
            {
                _extensions.Add(extension);
                return this;
            }

            public IProcessBuilder AddProcessExtensions(IEnumerable<IExtensionProvider<RctProcess>> extensions)
            {
                _extensions.AddRange(extensions);
                return this;
            }
        }
    }
}