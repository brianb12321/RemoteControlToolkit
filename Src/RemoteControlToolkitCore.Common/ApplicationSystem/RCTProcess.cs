using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Crayon;
using IronPython.Modules;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking;
using Zio;
using ThreadState = System.Threading.ThreadState;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public delegate CommandResponse ProcessDelegate(RCTProcess current, CancellationToken token);
    public class RCTProcess : IExtensibleObject<RCTProcess>
    {
        public uint Pid { get; set; }
        public bool IsBackground { get; set; }
        public bool DisposeIn { get; set; } = true;
        public bool DisposeOut { get; set; } = true;
        public bool DisposeError { get; set; } = true;
        public bool InRedirected { get; private set; }
        public bool OutRedirected { get; private set; }
        public bool ErrorRedirected { get; private set; }
        public IPrincipal Identity { get; }

        public IInstanceSession ClientContext { get; set; }
        public TextWriter Out { get; private set; }
        public TextWriter Error { get; private set; }
        public TextReader In { get; private set; }
        public UPath WorkingDirectory { get; set; }
        public IExtensionCollection<RCTProcess> Extensions { get; }
        public bool Running => State == ThreadState.Running;
        public ThreadState State => _workingThread.ThreadState;
        public string Name { get; }
        public RCTProcess Parent { get; }
        public RCTProcess Child { get; set; }
        public event EventHandler<Exception> ThreadError;
        public event EventHandler<ControlCEventArgs> ControlC;
        public event EventHandler StandardOutDisposed;
        public event EventHandler StandardInDisposed;
        public event EventHandler StandardErrorDisposed;
        private Thread _workingThread;
        private CancellationTokenSource cts;
        private ProcessDelegate _threadStart;
        public CommandResponse ExitCode { get; private set; }
        public Dictionary<string, string> EnvironmentVariables { get; }
        public bool Disposed { get; private set; }
        private IProcessTable _table;

        private RCTProcess(IProcessTable table, IInstanceSession session, string name, RCTProcess parent, ProcessDelegate threadStart, IPrincipal identity)
        {
            WorkingDirectory = "/";
            _table = table;
            Name = name;
            Parent = parent;
            _threadStart = threadStart;
            ClientContext = session;
            Pid = _table.LatestProcess + 1;
            Extensions = new ExtensionCollection<RCTProcess>(this);
            EnvironmentVariables = new Dictionary<string, string>();
            Identity = identity;
            if (Parent != null)
            {
                Parent.Child = this;
                Out = Parent.Out;
                In = Parent.In;
                Error = Parent.Error;
                Identity = Parent.Identity;
                ClientContext = Parent.ClientContext;
                DisposeIn = Parent.DisposeIn;
                DisposeError = Parent.DisposeError;
                DisposeOut = Parent.DisposeOut;
                StandardOutDisposed += Parent.StandardOutDisposed;
                StandardInDisposed += Parent.StandardInDisposed;
                StandardErrorDisposed += Parent.StandardErrorDisposed;
                InRedirected = Parent.InRedirected;
                OutRedirected = Parent.OutRedirected;
                ErrorRedirected = Parent.ErrorRedirected;
                IsBackground = Parent.IsBackground;
                WorkingDirectory = Parent.WorkingDirectory;
                IExtension<RCTProcess>[] buffer = new IExtension<RCTProcess>[Parent.Extensions.Count];
                Parent.Extensions.CopyTo(buffer, 0);
                Extensions = new ExtensionCollection<RCTProcess>(this);
                EnvironmentVariables = new Dictionary<string, string>(Parent.EnvironmentVariables);
            }

            _workingThread = new Thread(startThread);
            _workingThread.SetApartmentState(ApartmentState.STA);
            cts = new CancellationTokenSource();
        }

        public void Start()
        {
            if (IsBackground)
            {
                SetIn(TextReader.Null);
                SetOut(TextWriter.Null);
                SetError(TextWriter.Null);
            }
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
                ExitCode = _threadStart?.Invoke((RCTProcess)data, cts.Token);
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
        }

        public void WaitForExit()
        {
            _workingThread.Join();
            Dispose();
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

        public void SetOut(TextWriter outWriter)
        {
            if (Out != null) OutRedirected = true;
            Out = outWriter;
        }

        public void SetError(TextWriter errorWriter)
        {
            if (Error != null) ErrorRedirected = true;
            Error = errorWriter;
        }

        public void SetIn(TextReader inReader)
        {
            if (In != null) InRedirected = true;
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
                        cts?.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        
                    }
                }
                cts?.Dispose();
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
                Child?.Dispose();
                _table.RemoveProcess(Pid);
                Disposed = true;
            }
        }


        public class RCTPRocessFactory
        {
            private IProcessTable _table;
            public RCTPRocessFactory(IProcessTable table)
            {
                _table = table;
            }
            public RCTProcess Create(IInstanceSession session, string name, ProcessDelegate processDelegate, RCTProcess parent, IPrincipal identity)
            {
                RCTProcess process = new RCTProcess(_table, session, name, parent, processDelegate, parent?.Identity ?? identity);
                return process;
            }
            public RCTProcess CreateOnApplication(IInstanceSession session, IApplication application, RCTProcess parent, CommandRequest request, IPrincipal identity)
            {
                RCTProcess process = new RCTProcess(_table, session, application.ProcessName, parent, (proc, token) => application.Execute(request, proc, token), parent?.Identity ?? identity);
                return process;
            }

            public RCTProcess CreateOnExternalProcess(IInstanceSession session, CommandRequest args, RCTProcess parent, IPrincipal identity)
            {
                RCTProcess process = new RCTProcess(_table, session, $"External process: {args.Arguments[0]}", parent, (proc, token) =>
                {
                    try
                    {
                        string data = proc.In.ReadToEnd();
                        Process extProcess = new Process();
                        extProcess.StartInfo.UseShellExecute = false;
                        extProcess.StartInfo.FileName = args.Arguments[0].ToString().Substring(2);
                        extProcess.StartInfo.Arguments = args.GetArguments();
                        extProcess.StartInfo.RedirectStandardError = true;
                        extProcess.StartInfo.RedirectStandardOutput = true;
                        extProcess.StartInfo.RedirectStandardInput = true;
                        extProcess.OutputDataReceived += (sender, e) =>
                        {
                            proc.Out.WriteLine(e.Data);
                        };
                        extProcess.ErrorDataReceived += (sender, e) =>
                        {
                            proc.Out.WriteLine(e.Data);
                        };
                        token.Register(() =>
                        {
                            extProcess.Kill();
                        });
                        extProcess.Start();
                        extProcess.BeginOutputReadLine();
                        extProcess.BeginErrorReadLine();
                        extProcess.StandardInput.WriteLine(data);
                        extProcess.WaitForExit();
                        return new CommandResponse(extProcess.ExitCode);
                    }
                    catch (Exception e)
                    {
                        proc.Error.WriteLine(Output.Red($"Error while executing external program: {e.Message}"));
                        return new CommandResponse(CommandResponse.CODE_FAILURE);
                    }
                }, parent?.Identity ?? identity);
                return process;
            }
        }
    }
}