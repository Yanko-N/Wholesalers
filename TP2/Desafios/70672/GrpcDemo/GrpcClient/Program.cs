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
            var input = new In { In_ = 10};
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var client = new Converter.ConverterClient(channel);

            var reply = await client.C2FAsync(input);
            Console.WriteLine(reply.Msg);
            reply = await client.F2CAsync(input);
            Console.WriteLine(reply.Msg);
            reply = await client.D2EAsync(input);
            Console.WriteLine(reply.Msg);
            reply = await client.E2DAsync(input);
            Console.WriteLine(reply.Msg);
            reply = await client.K2MAsync(input);
            Console.WriteLine(reply.Msg);
            reply = await client.M2KAsync(input);
            Console.WriteLine(reply.Msg);

            Console.ReadLine();
        }
    }
}
