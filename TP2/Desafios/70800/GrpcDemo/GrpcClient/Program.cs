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
            //var input = new HelloRequest { Name = "Santola" };
            //var channel = GrpcChannel.ForAddress("https://localhost:7275");
            //var client = new Greeter.GreeterClient(channel);

            //var reply = await client.SayHelloAsync(input);
            //Console.WriteLine(reply.Message);

            Console.ReadLine();
        }
    }
}