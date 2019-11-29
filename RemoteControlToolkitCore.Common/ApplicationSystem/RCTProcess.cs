using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using Crayon;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking;
using ThreadState = System.Threading.ThreadState;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public delegate CommandResponse ProcessDelegate(RCTProcess current, CancellationToken token);
    public class RCTProcess
    {
        public uint Pid { get; set; }
        public bool DisposeIO { get; set; } = true;
        public IInstanceSession ClientContext { get; set; }
        public TextWriter Out { get; private set; }
        public TextWriter Error { get; private set; }
        public TextReader In { get; private set; }
        public bool Running => State == ThreadState.Running;
        public ThreadState State => _workingThread.ThreadState;
        public string Name { get; }
        public RCTProcess Parent { get; }
        public RCTProcess Child { get; set; }
        public event EventHandler<Exception> ThreadError;
        public event EventHandler<ControlCEventArgs> ControlC;
        private Thread _workingThread;
        private CancellationTokenSource cts;
        private ProcessDelegate _threadStart;
        public CommandResponse ExitCode { get; private set; }
        public bool Disposed { get; private set; }
        private IProcessTable _table;

        private RCTProcess(IProcessTable table, IInstanceSession session, string name, RCTProcess parent, ProcessDelegate threadStart)
        {
            _table = table;
            Name = name;
            Parent = parent;
            _threadStart = threadStart;
            ClientContext = session;
            Pid = _table.LatestProcess + 1;
            if (Parent != null)
            {
                Parent.Child = this;
                Out = Parent.Out;
                In = Parent.In;
                Error = Parent.Error;
                ClientContext = Parent.ClientContext;
            }

            _workingThread = new Thread(startThread);
            _workingThread.SetApartmentState(ApartmentState.STA);
            cts = new CancellationTokenSource();
        }

        public void Start()
        {
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
            catch (ThreadAbortException e)
            {
                ExitCode = new CommandResponse(CommandResponse.CODE_THREAD_ABORT);
            }
            catch (Exception ex)
            {
                ThreadError?.Invoke(this, ex);
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
            cts.Cancel();
            Dispose();
        }

        public void SetOut(TextWriter outWriter)
        {
            Out = outWriter;
        }

        public void SetError(TextWriter errorWriter)
        {
            Error = errorWriter;
        }

        public void SetIn(TextReader inReader)
        {
            In = inReader;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (_workingThread.IsAlive)
                {
                    cts?.Cancel();
                }
                cts?.Dispose();
                if (DisposeIO)
                {
                    In?.Close();
                    Out?.Close();
                    Error?.Close();
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
            public RCTProcess Create(IInstanceSession session, string name, ProcessDelegate processDelegate, RCTProcess parent)
            {
                RCTProcess process = new RCTProcess(_table, session, name, parent, processDelegate);
                return process;
            }
            public RCTProcess CreateOnApplication(IInstanceSession session, IApplication application, RCTProcess parent, CommandRequest request)
            {
                RCTProcess process = new RCTProcess(_table, session, application.ProcessName, parent, (proc, token) => application.Execute(request, proc, token));
                return process;
            }

            public RCTProcess CreateOnExternalProcess(IInstanceSession session, CommandRequest args, RCTProcess parent)
            {
                RCTProcess process = new RCTProcess(_table, session, $"External process: {args.Arguments[0]}", parent, (proc, token) =>
                {
                    try
                    {
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
                            proc.Error.WriteLine(e.Data);
                        };
                        token.Register(() =>
                        {
                            extProcess.Kill();
                        });
                        extProcess.Start();
                        extProcess.BeginOutputReadLine();
                        extProcess.BeginErrorReadLine();
                        while (!extProcess.HasExited)
                        {
                            string text = proc.In.ReadLine();
                            extProcess.StandardInput.WriteLine(text);
                        }

                        return new CommandResponse(extProcess.ExitCode);
                    }
                    catch (Exception e)
                    {
                        proc.Error.WriteLine(Output.Red($"Error while executing external program: {e.Message}"));
                        return new CommandResponse(CommandResponse.CODE_FAILURE);
                    }
                });
                return process;
            }
        }
    }
}