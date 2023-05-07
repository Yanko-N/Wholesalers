using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aula_2___Sockets___Client {
    internal class Program {
        static void Main(string[] args) {
            TcpClient ClientSocket = new TcpClient("localhost", 1337);
            Console.WriteLine($"Connected to: {((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Address}:{((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Port}");

            Thread thread = new Thread(() => {
                Program.MainThread(ClientSocket);
            });
            thread.Start();

            var message = Console.ReadLine();
            while (!String.IsNullOrEmpty(message)) {
                var bytesmessage = Encoding.UTF8.GetBytes(message);
                ClientSocket.GetStream().Write(bytesmessage, 0, bytesmessage.Length);
                message = Console.ReadLine();
            }
            ClientSocket.Client.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();

        }

        public static void MainThread(TcpClient ClientSocket) {
            try {
                while (ClientSocket.Connected) {
                    byte[] buffer = new byte[1024];
                    int byte_count = ClientSocket.GetStream().Read(buffer, 0, buffer.Length);


                    string data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                    Console.WriteLine(data);
                }
            } catch (IOException e) {

            }
        }
    }
}
