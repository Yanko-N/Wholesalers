//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using Microsoft.VisualBasic;
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

            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var customerClient = new Customer.CustomerClient(channel);

            var clientRequested = new CustomerLookupModel { UserId = 2 };

            var customer = await customerClient.GetCustomerInfoAsync(clientRequested);

            Console.WriteLine($"{customer.FirstName} {customer.LastName}");

            Console.WriteLine();
            Console.WriteLine("New Customer List");
            Console.WriteLine();

            using (var call = customerClient.GetNewCustomers(new NewCustomerRequest()))
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var currentCustomer = call.ResponseStream.Current;

                    Console.WriteLine($"{currentCustomer.LastName} {currentCustomer.LastName} {currentCustomer.EmailAddress} {currentCustomer.Age} {currentCustomer.IsAlive} ");

                }

            }

                Console.ReadLine();
        }
    }
}