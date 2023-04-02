using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aula_2___Sockets___Client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            TcpClient ClientSocket = ConnectServer();

            if (ClientSocket.Connected)
            {
                Console.WriteLine($"Connected to: {((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Address}:{((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Port}");


                Thread thread = new Thread(() =>
                {
                    Program.MainThread(ClientSocket);
                });
                thread.Start();
                SendFile(ClientSocket, "./teste.csv");
                var message = Console.ReadLine();
                while (!String.IsNullOrEmpty(message))
                {
                    var bytesmessage = Encoding.UTF8.GetBytes(message);
                    ClientSocket.GetStream().Write(bytesmessage, 0, bytesmessage.Length);
                    message = Console.ReadLine();
                }
                ClientSocket.Client.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
            ClientSocket.Close();

        }

        public static TcpClient ConnectServer()
        {
            TcpClient ClientSocket = new TcpClient();

            string[] ipBuffer;
            while (true)
            {
                Console.WriteLine("Write the IP you want to connect to:");
                ipBuffer = Console.ReadLine().Split(':');
                if (ipBuffer.Length == 2)
                {
                    if (!String.IsNullOrEmpty(ipBuffer[0]) && !String.IsNullOrEmpty(ipBuffer[1]))
                        break;
                }
            }

            try
            {
                ClientSocket.Connect(ipBuffer[0], int.Parse(ipBuffer[1]));
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n");
            }

            return ClientSocket;
        }

        public static void MainThread(TcpClient ClientSocket)
        {
            try
            {
                while (ClientSocket.Connected)
                {
                    byte[] buffer = new byte[1024];
                    int byte_count = ClientSocket.GetStream().Read(buffer, 0, buffer.Length);


                    string data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                    Console.WriteLine(data);
                }
            }
            catch (IOException e)
            {
            }
        }

        public static void SendFile(TcpClient ClientSocket, string path)
        {
            Console.WriteLine($"Sending File: {path}");
            var buff = File.ReadAllBytes(path).Concat(Encoding.UTF8.GetBytes("\0\0\0")).ToArray();
            ClientSocket.GetStream().Write(buff, 0, buff.Length);
        }

    }
}
