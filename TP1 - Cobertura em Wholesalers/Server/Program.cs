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

        public enum FileStatus {
            OPEN,
            ERROR,
            IN_PROGRESS,
            COMPLETED
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
            string id = Guid.NewGuid().ToString();
            Console.WriteLine($"{id} Connected!");
            // Neste método, é iniciada a gestão da comunicação do servidor com o cliente
            OnClientConnected(client);
            ParseFile(client);
            CloseConnection(client);
            Console.WriteLine($"{id} Disconnected!");
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
            File.WriteAllText($"./Coberturas/{filename}.csv", sb.ToString(), Encoding.UTF8);
            var hash = ChecksumUtil.GetChecksum(HashingAlgoTypes.SHA256, $"./Coberturas/{filename}.csv");

            if (!dataContext.Ficheiros.Any(x => x.Hash == hash)) {
                bRec = Encoding.UTF8.GetBytes($"{(int)StatusCode.OK} - File Received\0\0\0");
                Console.WriteLine($"{(int)StatusCode.OK} - File Received\0\0\0");
                client.GetStream().Write(bRec, 0, bRec.Length);

                Ficheiro file = new Ficheiro();
                file.Hash = hash;
                dataContext.Ficheiros.Add(file);
                dataContext.SaveChanges();
                var lista = CsvParser.CsvToList($"./Coberturas/{filename}.csv", ';');
                if (lista[0][0] != "Operador" || lista[0][1] != "Município" || lista[0][2] != "Rua" || lista[0][3] != "Número" || lista[0][4] != "Apartamento" || lista[0][5] != "Owner") {
                    Console.WriteLine($"{(int)StatusCode.ERROR} - {StatusCode.ERROR}: Invalid File!\0\0\0");
                    bRec = Encoding.UTF8.GetBytes($"{(int)StatusCode.ERROR} - {StatusCode.ERROR}: Invalid File!\0\0\0");
                    client.GetStream().Write(bRec, 0, bRec.Length);
                    File.Delete($"./Coberturas/{filename}.csv");
                } else {
                    lista.RemoveAt(0);
                    lista = lista.OrderBy(x => x[1]).ToList();
                    lista.Insert(0, new List<string>() { "Operador", "Município", "Rua", "Número", "Apartamento", "Owner" });
                    int linha = 1;
                    dataContext.Logs.Add(new Logs() { DataInicio = DateTime.Now, Estado = FileStatus.OPEN.ToString(), Ficheiro = filename, Operador = lista[1][0] });
                    dataContext.Logs.Add(new Logs() { DataInicio = DateTime.Now, Estado = FileStatus.IN_PROGRESS.ToString(), Ficheiro = filename, Operador = lista[1][0] });
                    foreach (var item in lista) {
                        Thread.Sleep(1000);
                        if (item[0] != "Operador") {
                            Cobertura cobertura = new Cobertura();
                            cobertura.Operador = item[0];
                            cobertura.Municipio = item[1];
                            cobertura.Rua = item[2];
                            cobertura.Numero = item[3];
                            cobertura.Apartamento = item[4];
                            cobertura.Owner = Boolean.Parse(item[5]);


                            linha++;

                            if (String.IsNullOrEmpty(item[0]) || String.IsNullOrEmpty(item[1]) ||
                                String.IsNullOrEmpty(item[2]) || String.IsNullOrEmpty(item[3])) {

                                Console.WriteLine($"{(int)StatusCode.ERROR} - {StatusCode.ERROR}: Parsing error, empty or null value at line {linha}! File: {filename}.csv\0\0\0");
                                dataContext.Logs.Add(new Logs() { DataInicio = DateTime.Now, Estado = FileStatus.ERROR.ToString(), Ficheiro = filename, Operador = lista[1][0] });
                            } else {
                                dataContext.Coberturas.Add(cobertura);
                            }
                        }
                    }
                    dataContext.Logs.Add(new Logs() { DataInicio = DateTime.Now, Estado = FileStatus.COMPLETED.ToString(), Ficheiro = filename, Operador = lista[1][0] });
                    Console.WriteLine($"{(int)StatusCode.OK} - {StatusCode.OK}: File processed!\0\0\0");
                    dataContext.SaveChanges();
                }

            } else {
                Console.WriteLine($"{(int)StatusCode.ERROR} - {StatusCode.ERROR}: File already processed!\0\0\0");
                bRec = Encoding.UTF8.GetBytes(
                    $"{(int)StatusCode.ERROR} - {StatusCode.ERROR}: File already processed!\0\0\0");
                client.GetStream().Write(bRec, 0, bRec.Length);
                File.Delete($"./Coberturas/{filename}.csv");
                return;
            }
            Console.WriteLine($"Saved as '{filename}.csv'");
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
            byte[] buffer = new byte[1024];
            StringBuilder data = new StringBuilder();
            do {
                int byte_count = client.GetStream().Read(buffer, 0, buffer.Length);
                data.Append(Encoding.UTF8.GetString(buffer, 0, byte_count));
            } while (!data.ToString().Contains("\0\0\0"));

            if (data.ToString().Contains("QUIT")) {
                buffer = Encoding.UTF8.GetBytes($"{(int)StatusCode.BYE} - {StatusCode.BYE}\0\0\0");
                client.GetStream().Write(buffer, 0, buffer.Length);
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }
    }
}
