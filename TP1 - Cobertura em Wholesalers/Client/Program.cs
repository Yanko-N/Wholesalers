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
        public enum StatusCode {
            OK = 100,
            ERROR = 300,
            BYE = 400
        }
        static void Main(string[] args) {

            TcpClient ClientSocket = ConnectServer();

            if (ClientSocket.Connected) {
                Console.WriteLine($"Connected to: {((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Address}:{((IPEndPoint)ClientSocket.Client.RemoteEndPoint).Port}");

                byte[] buffer = new byte[1024];
                string data;
                do {
                    int byte_count = ClientSocket.GetStream().Read(buffer, 0, buffer.Length);
                    data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                } while (!data.Contains("\0\0\0"));

                if (!data.Contains($"{(int)StatusCode.OK} - {StatusCode.OK}")) {
                    Console.WriteLine("Error: Closing connection...");
                    ClientSocket.Client.Shutdown(SocketShutdown.Both);
                    ClientSocket.Close();
                    Console.ReadKey();
                    return;
                }


                SendFile(ClientSocket, "./teste.csv");

                do {
                    int byte_count = ClientSocket.GetStream().Read(buffer, 0, buffer.Length);
                    data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                } while (!data.Contains("\0\0\0"));

                Console.WriteLine(data);
                Console.ReadKey();

            }
            ClientSocket.Close();

        }

        public static TcpClient ConnectServer() {
            TcpClient ClientSocket = new TcpClient();

            string[] ipBuffer;
            while (true) {
                Console.WriteLine("Write the IP you want to connect to:");
                ipBuffer = Console.ReadLine().Split(':');
                if (ipBuffer.Length == 2) {
                    if (!String.IsNullOrEmpty(ipBuffer[0]) && !String.IsNullOrEmpty(ipBuffer[1]))
                        break;
                }
            }

            try {
                ClientSocket.Connect(ipBuffer[0], int.Parse(ipBuffer[1]));
            } catch (Exception e) {
                Console.WriteLine($"{e.Message}\n");
            }

            return ClientSocket;
        }


        public static void SendFile(TcpClient ClientSocket, string path) {
            Console.WriteLine($"Sending File: {path}");
            var buff = File.ReadAllBytes(path).Concat(Encoding.UTF8.GetBytes("\0\0\0")).ToArray();
            ClientSocket.GetStream().Write(buff, 0, buff.Length);
        }

    }
}
