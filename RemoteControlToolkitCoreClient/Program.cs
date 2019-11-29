using RemoteControlToolkitCore.Common.Networking;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace RemoteControlToolkitCoreClient
{
    class Program
    {
        private const int RESPONSE_LENGTH = 4096;
        static void Main(string[] args)
        {
            new Program().Run();
        }

        public void Run()
        {
            Console.Write("Enter IP: ");
            string ip = Console.ReadLine();
            Console.Write("Enter Port: ");
            int port = int.Parse(Console.ReadLine());
            TcpClient client = new TcpClient();
            client.Connect(ip, port);
            StreamReader sr = new StreamReader(client.GetStream());
            StreamWriter sw = new StreamWriter(client.GetStream());
            Thread readThread = new Thread(() =>
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            });
            readThread.Start();
            while (true)
            {
                string input = Console.ReadLine();
                sw.WriteLine(input);
                sw.Flush();   
            }
        }
    }
}
