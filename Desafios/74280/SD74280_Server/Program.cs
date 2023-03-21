using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;

namespace SD74280_Server
{
    internal class Program
    {
        public static Mutex mutex = new Mutex();
        static ConcurrentDictionary<TcpClient, Guid> clients = new ConcurrentDictionary<TcpClient, Guid>();
        static void Main(string[] args)
        {
            //A classe TCPListener implementa os métodos da classe Socket utilizando o protócolo TCP, permitindo uma maior abstração das etapas tipicamente associadas ao Socket.
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 1337);
            Console.WriteLine($"Listening on: {((IPEndPoint)ServerSocket.LocalEndpoint).Address}:{((IPEndPoint)ServerSocket.LocalEndpoint).Port}");
            
            //Codigo da parte do ficheiro
            int nThreads = 3;
            


            for (int i = 1; i <= 3; i++)
            {
                Thread newThread = new Thread(() => { Program.WriteFile(); });
                newThread.Name = String.Format("Thread {0}", i-1);
                newThread.Start();
            }
                

                
            
                
            

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
        public static void WriteFile()
        {
            Console.WriteLine("the Thread {0} is waiting to enter",Thread.CurrentThread.Name);
            mutex.WaitOne();
            
            Console.WriteLine("the Thread {0} entered", Thread.CurrentThread.Name);
            WriteinFile(Thread.CurrentThread.Name + "<-- :)");

            mutex.ReleaseMutex();
            Console.WriteLine("The Thread {0} as left", Thread.CurrentThread.Name);
        }
        public static void SendMessages(TcpClient client)
        {
            string sms = Console.ReadLine();

            while (!String.IsNullOrEmpty(sms))
            {
                broadcast(client, sms);
                sms = Console.ReadLine();
            }


        }
        public static void Data(TcpClient client)
        {
            DateTime date = DateTime.Now;
            broadcast(client, date.ToString());
        }
        public static void NumeroAleatorio(TcpClient client)
        {
            Random random = new Random();
            int number = random.Next();
            broadcast(client, "numero aleatorio:" + number.ToString());
        }
        public static void Soma(TcpClient client, string data)
        {
            int soma = 0;
            string[] string_numbers = data.Split(' ');
            int number;
            foreach (var s in string_numbers)
            {
                if (int.TryParse(s, out number))
                {
                    soma += number;
                }

            }
            string finalMessage = soma.ToString() + " Somado";
            broadcast(client, finalMessage);
        }
        public static void handle_client(TcpClient client)
        {
            // Neste método, é iniciada a gestão da comunicação do servidor com o cliente

            while (true)
            {
                //ciclo infinito de receção de mensagens por parte do cliente, até o cliente ter enviado uma mensagem vazia (só clicou no 'Enter')
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }


                //Envio da mensagem de confirmação do servidor de volta para o cliente
                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(client, data);

                


                Thread serverSendMessages = new Thread(() => { Program.SendMessages(client); });


                serverSendMessages.Start();
            }

            // código para desligar a conexão com o cliente
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

      
        public static void WriteinFile(string text)
        {
            StreamReader streamReader = new StreamReader("C:/Users/Vitor Novo/source/repos/SistemasDistribuidos/Desafios/74280/SD74280_Server/file.txt");
            string fileText = streamReader.ReadToEnd();
            streamReader.Close();


            StreamWriter streamWriter = new StreamWriter("C:/Users/Vitor Novo/source/repos/SistemasDistribuidos/Desafios/74280/SD74280_Server/file.txt");
            streamWriter.WriteLine(fileText + text);
            streamWriter.Close();

            //Console.WriteLine(fileText + text + "\n this a above is the text in the file");
        }
        public static void broadcast(TcpClient client, string data)
        {
            mutex.WaitOne();
            foreach (var c in clients)
            {
                NetworkStream stream = c.Key.GetStream();
                byte[] buffer;
                if (c.Key == client)
                    buffer = Encoding.ASCII.GetBytes(data + " - Server Acknowledgement" + Environment.NewLine);
                else
                    buffer = Encoding.ASCII.GetBytes(clients[client] + ": " + data);
                stream.Write(buffer, 0, buffer.Length);
            }
            mutex.ReleaseMutex();
        }
    }
}
