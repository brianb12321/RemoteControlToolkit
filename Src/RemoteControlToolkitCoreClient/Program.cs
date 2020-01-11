using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using VtNetCore.VirtualTerminal;

namespace RemoteControlToolkitCoreClient
{
    class Program
    {
        static void Main(string[] args)
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
                while (true)
                {
                    Console.Write((char)sr.Read());
                }
            });
            readThread.Start();
            while (true)
            {
                sw.Write((char)Console.Read());
            }
        }
    }
}