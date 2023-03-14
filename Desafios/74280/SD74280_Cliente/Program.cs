using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SD74280_Cliente
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpClient ClientSocket = new TcpClient("localhost", 1337);

            //escrever no ecra que o cliente foi conectado ao servidor

             Thread thread = new Thread(() => {
                Program.MainThread(ClientSocket);
            });

            thread.Start();

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

        public static void MainThread(TcpClient ClientSocket)
        {

            try
            {
                while (ClientSocket.Connected)
                {
                    byte[] buffer = new byte[1024];
                    int byte_count = ClientSocket.GetStream().Read(buffer, 0, buffer.Length);


                    string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                    Console.WriteLine(data);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
