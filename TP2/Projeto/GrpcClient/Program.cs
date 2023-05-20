//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using Grpc.Net.Client;
using GrpcService;
using System;

namespace GrpcClient {
    class Program {
        static async Task Main(string[] args) {

            //Protos e Grpc Channel
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var authClient = new Auth.AuthClient(channel);
            var adminClient = new AdminActions.AdminActionsClient(channel);
            var operatorClient = new OperatorActions.OperatorActionsClient(channel);

            //Main Menu System
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
                var user = new AuthUser();
                AuthReply reply = new AuthReply();

                switch (choice) {
                    case "1":
                        Console.Clear();
                        Console.Write("Username: ");
                        user.Username = Console.ReadLine();
                        Console.Write("Password: ");
                        user.Password = Utils.ReadPassword();

                        reply = await authClient.LogInAsync(user);
                        Console.WriteLine($"{reply.Status} - {reply.Message}");
                        Console.ReadLine();

                        break;

                    case "2":
                        string confirm;
                        do {
                            Console.Clear();
                            Console.Write("Username: ");
                            user.Username = Console.ReadLine();
                            Console.Write("Password: ");
                            user.Password = Utils.ReadPassword();
                            Console.Write("Confirm Password: ");
                            confirm = Utils.ReadPassword();
                            if (user.Password != confirm)
                                Console.WriteLine("Passwords do not match!");
                        } while (user.Password != confirm);

                        reply = await authClient.RegisterAsync(user);
                        Console.WriteLine($"{reply.Status} - {reply.Message}");
                        Console.ReadLine();

                        break;
                    case "3":
                        exit = true;
                        Console.WriteLine("Exiting the program...");

                        break;
                    
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.ReadLine();
                        break;
                }
            }




            using (var call = adminClient.ListAllCoberturas(new AdminActionsListAllCoberturas())) {
                while (await call.ResponseStream.MoveNext(CancellationToken.None)) {
                    var curr = call.ResponseStream.Current;

                    Console.WriteLine($"{curr.Operator} - {curr.Municipio} {curr.Rua} {curr.Numero} {curr.Apartamento ?? ""} {curr.Estado}");

                }

            }


            Console.ReadLine();
        }
    }
}
