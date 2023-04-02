using Aula_2___Sockets;
using Aula_2___Sockets.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Aula_2___Sockets___Server
{


    class Program
    {
        private static Mutex mutex = new Mutex();
        private static ConcurrentDictionary<TcpClient, Guid> clients = new ConcurrentDictionary<TcpClient, Guid>();
        private static List<string> files = new List<string>();

        public enum StatusCode
        {
            OK = 100,
            COMPLETED = 101,
            IN_PROGRESS = 200,
            OPEN = 201,
            ERROR = 300,
            BYE = 400
        }


        static void Main(string[] args)
        {
            //A classe TCPListener implementa os métodos da classe Socket utilizando o protócolo TCP, permitindo uma maior abstração das etapas tipicamente associadas ao Socket.
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 1337);
            Console.WriteLine($"Listening on: {((IPEndPoint)ServerSocket.LocalEndpoint).Address}:{((IPEndPoint)ServerSocket.LocalEndpoint).Port}");

            //A chamada ao método "Start" inicia o Socket para ficar à escuta de novas conexões por parte dos clientes
            ServerSocket.Start();
            Thread thread = new Thread(() =>
            {
                Program.MainThread(ServerSocket);
            });
            thread.Start();
        }

        public static void MainThread(TcpListener ServerSocket)
        {
            while (true)
            {
                //Ciclo infinito para ficar à espera que um cliente Socket/TCP até quando pretender conectar-se

                TcpClient client = ServerSocket.AcceptTcpClient();
                mutex.WaitOne();
                clients.TryAdd(client, Guid.NewGuid());
                mutex.ReleaseMutex();
                Thread thread = new Thread(() =>
                {
                    Program.MainThread(ServerSocket);
                });
                thread.Start();
                //Só avança para esta parte do código, depois de um cliente ter se conectado ao servidor
                Console.WriteLine($"{clients[client]} has connected! - {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                handle_client(client);
                Console.WriteLine($"{clients[client]} has disconnected!");
                mutex.WaitOne();
                clients.TryRemove(client, out _);
                mutex.ReleaseMutex();
            }
        }

        public static void handle_client(TcpClient client)
        {
            // Neste método, é iniciada a gestão da comunicação do servidor com o cliente

            while (true)
            {
                ParseFile(client);
                //ciclo infinito de receção de mensagens por parte do cliente, até o cliente ter enviado uma mensagem vazia (só clicou no 'Enter')
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                //Envio da mensagem de confirmação do servidor de volta para o cliente
                string data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                broadcast(client, data);
                Console.WriteLine(data);
            }
            // código para desligar a conexão com o cliente
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }


        public static void broadcast(TcpClient client, string data)
        {
            mutex.WaitOne();
            foreach (var c in clients)
            {
                NetworkStream stream = c.Key.GetStream();
                byte[] buffer;
                if (c.Key == client)
                    buffer = Encoding.UTF8.GetBytes(data + " - Server Acknowledgement" + Environment.NewLine);
                else
                    buffer = Encoding.UTF8.GetBytes(clients[client] + ": " + data);
                stream.Write(buffer, 0, buffer.Length);
            }
            mutex.ReleaseMutex();
        }

        public static void ParseFile(TcpClient client)
        {
            byte[] bRec = new byte[1024];
            int n;
            var sb = new StringBuilder();

            do
            {
                n = client.GetStream().Read(bRec, 0, bRec.Length);
                sb.Append(Encoding.UTF8.GetString(bRec, 0, n));
            } while (!Encoding.UTF8.GetString(bRec, 0, n).Contains("\0\0\0"));
            sb.Replace("\0\0\0", "");
            Console.WriteLine(sb.ToString());

            bRec = Encoding.UTF8.GetBytes("File ACK");
            client.GetStream().Write(bRec, 0, bRec.Length);

            File.WriteAllText("./teste.csv", sb.ToString());
            var hash = ChecksumUtil.GetChecksum(HashingAlgoTypes.SHA256, "./teste.csv");

            if (!files.Contains(hash))
            {
                files.Add(hash);
                var lista = CsvParser.CsvToList("./teste.csv", ';');
            }
            else
            {
                Console.WriteLine("Erro: Ficheiro já processado!");
                bRec = Encoding.UTF8.GetBytes("Erro: Ficheiro já processado!");
                client.GetStream().Write(bRec, 0, bRec.Length);
            }

        }

        public static List<dataModel> GetDataModelsMunicipio()
        {
            using (var context = new dataContext())
            {
                List<dataModel> data = context.datas.OrderBy(x => x.Municipio).OrderBy(x=>x.Rua).ToList();
                return data;
            };
        }

        public static List<dataModel> GetDataModelMunicipio(string municipio)
        {
            using (var context = new dataContext())
            {
                List<dataModel> data = context.datas.Where(x => x.Municipio.Contains(municipio)).OrderBy(x=>x.Rua).ToList();
                return data;
            }
        }
    }
}
