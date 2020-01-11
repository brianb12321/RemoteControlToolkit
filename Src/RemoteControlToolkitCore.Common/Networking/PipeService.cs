using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Networking
{
    public class PipeService : IPipeService
    {
        private List<AnonymousPipeServerStream> _anonymousServerPipes;
        private List<AnonymousPipeClientStream> _anonymousClientPipes;
        private List<NamedPipeServerStream> _namedServerPipes;
        private List<NamedPipeClientStream> _namedClientPipes;

        public PipeService()
        {
            _anonymousServerPipes = new List<AnonymousPipeServerStream>();
            _anonymousClientPipes = new List<AnonymousPipeClientStream>();
            _namedServerPipes = new List<NamedPipeServerStream>();
            _namedClientPipes = new List<NamedPipeClientStream>();
        }
        public (int position, AnonymousPipeServerStream stream) OpenAnonymousPipe(PipeDirection direction)
        {
            AnonymousPipeServerStream stream = new AnonymousPipeServerStream(direction);
            _anonymousServerPipes.Add(stream);
            return (_anonymousServerPipes.IndexOf(stream), stream);
        }

        public (int position, AnonymousPipeClientStream stream) ConnectToPipe(string handle, PipeDirection direction)
        {
            AnonymousPipeClientStream client = new AnonymousPipeClientStream(direction, handle);
            _anonymousClientPipes.Add(client);
            return (_anonymousClientPipes.IndexOf(client), client);
        }

        public AnonymousPipeServerStream[] GetServerAnonymousPipes()
        {
            return _anonymousServerPipes.ToArray();
        }

        public AnonymousPipeClientStream[] GetClientAnonymousPipes()
        {
            return _anonymousClientPipes.ToArray();
        }

        public bool AnonymousServerPipeExists(int id)
        {
            try
            {
                var pipe = _anonymousServerPipes[id];
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AnonymousClientPipeExists(int id)
        {
            try
            {
                var pipe = _anonymousClientPipes[id];
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void CloseAnonymousServerPipe(int id)
        {
            var pipe = _anonymousServerPipes[id];
            pipe.DisposeLocalCopyOfClientHandle();
            pipe.Close();
            _anonymousServerPipes.Remove(pipe);
        }

        public void DisconnectAnonymousClientPipe(int id)
        {
            _anonymousClientPipes[id].Close();
        }

        public AnonymousPipeServerStream GetAnonymousPipeServer(int id)
        {
            return _anonymousServerPipes[id];
        }

        public AnonymousPipeClientStream GetAnonymousPipeClient(int id)
        {
            return _anonymousClientPipes[id];
        }

        public (int position, NamedPipeServerStream stream) OpenNamedPipe(PipeDirection direction, string name)
        {
            NamedPipeServerStream pipe = new NamedPipeServerStream(name, direction);
            _namedServerPipes.Add(pipe);
            return (_namedServerPipes.IndexOf(pipe), pipe);
        }

        public (int position, NamedPipeClientStream stream) ConnectToNamedPipe(string name, string pipeName, PipeDirection direction, int timeout)
        {
            NamedPipeClientStream pipe = new NamedPipeClientStream(name, pipeName, direction);
            pipe.Connect(timeout);
            _namedClientPipes.Add(pipe);
            return (_namedClientPipes.IndexOf(pipe), pipe);
        }

        public NamedPipeServerStream[] GetServerNamedPipes()
        {
            return _namedServerPipes.ToArray();
        }

        public NamedPipeClientStream[] GetClientNamedPipes()
        {
            return _namedClientPipes.ToArray();
        }

        public bool NamedServerPipeExists(int id)
        {
            try
            {
                var pipe = _namedServerPipes[id];
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NamedClientPipeExists(int id)
        {
            try
            {
                var pipe = _namedClientPipes[id];
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void CloseNamedServerPipe(int id)
        {
            var pipe = _namedServerPipes[id];
            pipe.Close();
            _namedServerPipes.Remove(pipe);
        }

        public void DisconnectNamedClientPipe(int id)
        {
            var pipe = _namedClientPipes[id];
            pipe.Close();
            _namedClientPipes.Remove(pipe);
        }

        public NamedPipeServerStream GetNamedPipeServer(int id)
        {
            return _namedServerPipes[id];
        }

        public NamedPipeClientStream GetNamedPipeClient(int id)
        {
            return _namedClientPipes[id];
        }
    }
}