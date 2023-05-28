﻿using Grpc.Net.Client;
using GrpcService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Channels;

namespace GrpcClient {
    class Program {
        static async Task Main(string[] args) {
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var user = new AuthUser();
            string authToken = "";
            bool isAdmin;

            /* 
             //RABBIT MQ CONFIGURATIONS

             // Configuração da conexão com o RabbitMQ
             var factory = new ConnectionFactory() { HostName = "localhost" };
             using var connection = factory.CreateConnection();


             using var channelRabbit = connection.CreateModel();

             // Declaração da exchange do tipo "topic"
             channelRabbit.ExchangeDeclare("EVENT", ExchangeType.Topic);


             ConnectQueue(channelRabbit, "");

             */


            LoginMenu(channel).ContinueWith((task) => {
                user = task.Result.user;
                isAdmin = task.Result.isAdmin ?? false;
                authToken = task.Result.authToken ?? "";
            }).Wait();

            if (authToken == "") {
                Console.WriteLine("Exiting the program...");
                Console.ReadLine();
                return;
            }



            //Protos e Grpc Channel

            var adminClient = new AdminActions.AdminActionsClient(channel);
            var operatorClient = new OperatorActions.OperatorActionsClient(channel);

            //Main Menu System




            using (var call = adminClient.ListAllCoberturas(new AdminActionsListAllCoberturas {
                Operator = user.Username,
                Token = authToken
            })) {
                while (await call.ResponseStream.MoveNext(CancellationToken.None)) {
                    var curr = call.ResponseStream.Current;

                    Console.WriteLine($"{curr.Operator} - {curr.Municipio} {curr.Rua} {curr.Numero} {curr.Apartamento ?? ""} {curr.Estado}");

                }

            }


            Console.ReadLine();
        }


        static async Task<(AuthUser user, bool? isAdmin, string? authToken)> LoginMenu(GrpcChannel channel) {
            var user = new AuthUser();
            var authClient = new Auth.AuthClient(channel);
            string? authToken = null;
            bool? isAdmin = null;
            bool exit = false;
            while (!exit) {
                Console.Clear();
                Console.WriteLine("Welcome");
                Console.WriteLine("Please select an option:");
                Console.WriteLine("1. Log In");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();
                //GRPC de Login

                AuthReply reply = new AuthReply();

                switch (choice) {
                    case "1":   //LOGIN
                        Console.Clear();

                        Console.Write("Username: ");
                        user.Username = Console.ReadLine();
                        Console.Write("Password: ");
                        user.Password = Utils.ReadPassword();

                        reply = await authClient.LogInAsync(user);
                        Console.WriteLine($"{reply.Status} - {reply.Message}");
                        if (reply.Status == "OK") {
                            isAdmin = reply.IsAdmin;
                            authToken = reply.Token;
                            exit = true;
                        }
                        Console.ReadLine();
                        Console.Clear();

                        break;

                    case "2":   //REGISTO
                        string confirm;
                        do {
                            Console.Clear();

                            Console.Write("Username: ");
                            user.Username = Console.ReadLine();
                            Console.Write("Password: ");
                            user.Password = Utils.ReadPassword();
                            Console.Write("Confirm Password: ");
                            confirm = Utils.ReadPassword();

                            if (user.Password != confirm) //se as passwords dão match
                                Console.WriteLine("Passwords do not match!");
                            Console.ReadLine();
                        } while (user.Password != confirm);

                        reply = await authClient.RegisterAsync(user);
                        Console.WriteLine($"{reply.Status} - {reply.Message}");
                        Console.ReadLine();

                        break;
                    case "3":
                        exit = true;
                        Console.WriteLine("Exiting the program...");
                        isAdmin = null;
                        authToken = null;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.ReadLine();
                        break;
                }
            }


            return (user, isAdmin, authToken);
        }

        static void ConnectRabitMQ(IModel channelRabbit, string topic) {
            // Criação de uma fila exclusiva e vinculação à exchange com uma chave de roteamento específica
            var queueName = channelRabbit.QueueDeclare().QueueName;
            channelRabbit.QueueBind(queueName, topic, "");

            // Configuração do consumidor para receber as mensagens
            var consumer = new EventingBasicConsumer(channelRabbit);
            consumer.Received += (model, ea) => {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("\nMensagem recebida: {0}", message);
            };
            channelRabbit.BasicConsume(queueName, true, consumer);
        }

    }

}
