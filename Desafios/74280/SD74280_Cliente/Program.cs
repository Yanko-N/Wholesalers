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
            TcpClient ClientSocket = new TcpClient("example.com", 80); //Dizer que o cliente vai se ligar ao website example.com na porta 80

            //escrever no ecra que o cliente foi conectado ao servidor
            Console.WriteLine($"Connected to: {((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Address}:{((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Port}");

            var pedido = $"GET / HTTP/1.1\r\nHost: example.com \r\nConnection: Close\r\n\r\n";

            byte[] bytes = Encoding.UTF8.GetBytes(pedido);

            ClientSocket.GetStream().Write(bytes, 0, bytes.Length);
            byte[] Rbytes = new byte[1024];
            int n;
            var sb = new StringBuilder();
            do
            {
                n = ClientSocket.GetStream().Read(Rbytes, 0, Rbytes.Length);
                sb.Append(Encoding.ASCII.GetString(Rbytes, 0, n));
            } while (n > 0);


            ClientSocket.Client.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Console.WriteLine(sb.ToString());
            Console.ReadKey();


            /* Thread thread = new Thread(() => {
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
            ClientSocket.Close();*/

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
