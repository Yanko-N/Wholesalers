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
using Aula_2___Sockets;
using Aula_2___Sockets.Models;

namespace Aula_2___Sockets___Server
{


    class Program {
        public static dataContext dataContext = new dataContext();
        public enum StatusCode {
            OK = 100,
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
                Thread thread = new Thread(() => {
                    Program.MainThread(ServerSocket);
                });
                thread.Start();
                //Só avança para esta parte do código, depois de um cliente ter se conectado ao servidor
                handle_client(client);
            }
        }

        public static void handle_client(TcpClient client)
        {
            // Neste método, é iniciada a gestão da comunicação do servidor com o cliente
                ParseFile(client);


            while (true) {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                //ciclo infinito de receção de mensagens por parte do cliente, até o cliente ter enviado uma mensagem vazia (só clicou no 'Enter')

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                Console.WriteLine(data);
            }
            // código para desligar a conexão com o cliente
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }


        public static void ParseFile(TcpClient client)
        {
            byte[] bRec = new byte[1024];
            int n;
            var sb = new StringBuilder();
            string filename = Guid.NewGuid().ToString();

            do
            {
                n = client.GetStream().Read(bRec, 0, bRec.Length);
                sb.Append(Encoding.UTF8.GetString(bRec, 0, n));
            } while (!Encoding.UTF8.GetString(bRec, 0, n).Contains("\0\0\0"));
            sb.Replace("\0\0\0", "");
            Console.WriteLine(sb.ToString());

            bRec = Encoding.UTF8.GetBytes("File ACK");
            client.GetStream().Write(bRec, 0, bRec.Length);

            File.WriteAllText($"./{filename}.csv", sb.ToString());
            var hash = ChecksumUtil.GetChecksum(HashingAlgoTypes.SHA256, $"./{filename}.csv");

            if (!dataContext.Ficheiros.Any(x => x.Hash == hash)) {
                Ficheiro file = new Ficheiro();
                file.Hash = hash;
                dataContext.Ficheiros.Add(file);
                dataContext.SaveChanges();
                var lista = CsvParser.CsvToList($"./{filename}.csv", ';');
            } else {
                Console.WriteLine("Erro: Ficheiro já processado!");
                bRec = Encoding.UTF8.GetBytes("Erro: Ficheiro já processado!");
                client.GetStream().Write(bRec, 0, bRec.Length);
            }

            File.Delete($"./{filename}.csv");
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
