//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using Grpc.Net.Client;
using GrpcService;
using System;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var user = new AuthUser();
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var client = new Auth.AuthClient(channel);

            Console.Write("Username: ");
            user.Username = Console.ReadLine();
            Console.Write("Password: ");
            user.Password = Utils.ReadPassword();


            AuthReply reply = await client.LogInAsync(user);
            Console.WriteLine($"{reply.Status} - {reply.Message}");


            Console.ReadKey();
        }
    }
}
