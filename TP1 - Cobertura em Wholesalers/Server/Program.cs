using Aula_2___Sockets;
using Aula_2___Sockets.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Aula_2___Sockets___Server {


    class Program {
        public static dataContext dataContext = new dataContext();
        public enum StatusCode {
            OK = 100,
            ERROR = 300,
            BYE = 400
        }


        static void Main(string[] args) {
            //A classe TCPListener implementa os métodos da classe Socket utilizando o protócolo TCP, permitindo uma maior abstração das etapas tipicamente associadas ao Socket.
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 1337);
            Console.WriteLine($"Listening on: {((IPEndPoint)ServerSocket.LocalEndpoint).Address}:{((IPEndPoint)ServerSocket.LocalEndpoint).Port}");

            //A chamada ao método "Start" inicia o Socket para ficar à escuta de novas conexões por parte dos clientes
            ServerSocket.Start();
            Thread thread = new Thread(() => {
                Program.MainThread(ServerSocket);
            });
            thread.Start();
        }

        public static void MainThread(TcpListener ServerSocket) {
            while (true) {
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

        public static void handle_client(TcpClient client) {
            // Neste método, é iniciada a gestão da comunicação do servidor com o cliente
            OnClientConnected(client);
            ParseFile(client);


            CloseConnection(client);
        }


        public static void ParseFile(TcpClient client) {
            byte[] bRec = new byte[1024];
            int n;
            var sb = new StringBuilder();
            string filename = Guid.NewGuid().ToString();

            do {
                n = client.GetStream().Read(bRec, 0, bRec.Length);
                sb.Append(Encoding.UTF8.GetString(bRec, 0, n));
            } while (!Encoding.UTF8.GetString(bRec, 0, n).Contains("\0\0\0"));
            sb.Replace("\0\0\0", "");
            File.WriteAllText($"./{filename}.csv", sb.ToString());
            var hash = ChecksumUtil.GetChecksum(HashingAlgoTypes.SHA256, $"./{filename}.csv");

            if (!dataContext.Ficheiros.Any(x => x.Hash == hash)) {
                bRec = Encoding.UTF8.GetBytes($"{(int)StatusCode.OK} - Ficheiro Recebido\0\0\0");
                Console.WriteLine($"{(int)StatusCode.OK} - Ficheiro Recebido\0\0\0");
                client.GetStream().Write(bRec, 0, bRec.Length);

                Ficheiro file = new Ficheiro();
                file.Hash = hash;
                dataContext.Ficheiros.Add(file);
                dataContext.SaveChanges();
                var lista = CsvParser.CsvToList($"./{filename}.csv", ';');

            } else {
                Console.WriteLine($"{(int)StatusCode.ERROR} - {StatusCode.ERROR}: Ficheiro já processado!\0\0\0");
                bRec = Encoding.UTF8.GetBytes($"{(int)StatusCode.ERROR} - {StatusCode.ERROR}: Ficheiro já processado!\0\0\0");
                client.GetStream().Write(bRec, 0, bRec.Length);
            }

            File.Delete($"./{filename}.csv");
        }

        public static List<Cobertura> GetDataModelsMunicipio() {
            using (var context = new dataContext()) {
                List<Cobertura> data = context.Coberturas.OrderBy(x => x.Municipio).OrderBy(x => x.Rua).ToList();
                return data;
            };

        }

        public static List<Cobertura> GetDataModelMunicipio(string municipio) {
            using (var context = new dataContext()) {
                List<Cobertura> data = context.Coberturas.Where(x => x.Municipio.Contains(municipio)).OrderBy(x => x.Rua).ToList();
                return data;
            }
        }

        public static void OnClientConnected(TcpClient client) {
            var bytes = Encoding.UTF8.GetBytes($"{(int)StatusCode.OK} - {StatusCode.OK}\0\0\0");
            client.GetStream().Write(bytes, 0, bytes.Length);
        }

        public static void CloseConnection(TcpClient client) {
            //TODO: QUIT -> 400 BYE

            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
