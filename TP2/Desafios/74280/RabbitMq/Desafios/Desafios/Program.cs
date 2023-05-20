using RabbitMQ.Client;
using System;
using System.Text;


class Publisher
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Declaração da exchange do tipo "topic"
        channel.ExchangeDeclare("EVENT", ExchangeType.Topic);

        channel.QueueDeclare(queue: "",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);


        while (true)
        {
            string message = GetMessage();
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "EVENT",
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine($" [x] Sent {message}");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
        
    }

    static string GetMessage()
    {
        string msg;
        bool quit = false;
        do
        {
            Console.Clear();
            Console.WriteLine("escreva a sua mensagem:");
            msg = Console.ReadLine();
            string ack;
            Console.WriteLine("Tem certeza");
            ack = Console.ReadLine();

            if (ack.Contains("sim"))
            {
                quit = true;
            }

        } while (!quit);

        return msg;
    }
}
