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
            TcpClient ClientSocket = new TcpClient("example.com", 80);
            Console.WriteLine($"Connected to: {((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Address}:{((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Port}");


            var request = $"GET / HTTP/1.1\r\nHost: example.com \r\nConnection: Close\r\n\r\n";

            byte[] bytes = Encoding.UTF8.GetBytes(request);

            ClientSocket.GetStream().Write(bytes, 0, bytes.Length);
            byte[] bRec = new byte[1024];
            int n;
            var sb = new StringBuilder();
            do {
                n = ClientSocket.GetStream().Read(bRec, 0, bRec.Length);
                sb.Append(Encoding.ASCII.GetString(bRec, 0, n));
            } while (n > 0);


            ClientSocket.Client.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Console.WriteLine(sb.ToString());
            Console.ReadKey();
        }
    }
}