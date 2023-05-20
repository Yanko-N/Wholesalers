//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using Grpc.Net.Client;
using GrpcService;
using GrpcService.Protos;
using System;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var input = new In { In_ = 10 };
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var client = new converterProtocolo.converterProtocoloClient(channel);

            var reply = client.Celsius2Fahrenheint(input);
            Console.WriteLine(reply.Msg);
            reply = client.Fahrenheint2Celsius(input);
            Console.WriteLine(reply.Msg);
            reply = client.Dollar2Euro(input);
            Console.WriteLine(reply.Msg);
            reply = client.Euro2Dollar(input);
            Console.WriteLine(reply.Msg);
            reply = client.Km2Miles(input);
            Console.WriteLine(reply.Msg);
            reply = client.Miles2Km(input);
            Console.WriteLine(reply.Msg);

            Console.ReadLine();
        }
    }
}
