using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Networking
{
    /// <summary>
    /// Represents all the opened operating system pipes.
    /// </summary>
    public interface IPipeService
    {
        (int position, AnonymousPipeServerStream stream) OpenAnonymousPipe(PipeDirection direction);
        (int position, AnonymousPipeClientStream stream) ConnectToPipe(string handle, PipeDirection direction);
        AnonymousPipeServerStream[] GetServerAnonymousPipes();
        AnonymousPipeClientStream[] GetClientAnonymousPipes();
        bool AnonymousServerPipeExists(int id);
        bool AnonymousClientPipeExists(int id);
        void CloseAnonymousServerPipe(int id);
        void DisconnectAnonymousClientPipe(int id);
        AnonymousPipeServerStream GetAnonymousPipeServer(int id);
        AnonymousPipeClientStream GetAnonymousPipeClient(int id);
        (int position, NamedPipeServerStream stream) OpenNamedPipe(PipeDirection direction, string name);
        (int position, NamedPipeClientStream stream) ConnectToNamedPipe(string name, string pipeName, PipeDirection direction, int timeout);
        NamedPipeServerStream[] GetServerNamedPipes();
        NamedPipeClientStream[] GetClientNamedPipes();
        bool NamedServerPipeExists(int id);
        bool NamedClientPipeExists(int id);
        void CloseNamedServerPipe(int id);
        void DisconnectNamedClientPipe(int id);
        NamedPipeServerStream GetNamedPipeServer(int id);
        NamedPipeClientStream GetNamedPipeClient(int id);
    }
}