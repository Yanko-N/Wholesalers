using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

class Client
{
    static void Main()
    {
        // Configuração da conexão com o RabbitMQ
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();


        using var channel = connection.CreateModel();

        // Declaração da exchange do tipo "topic"
        channel.ExchangeDeclare("EVENT", ExchangeType.Topic);

        // Criação de uma fila exclusiva e vinculação à exchange com uma chave de roteamento específica
        var queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queueName, "EVENT", "");

        // Configuração do consumidor para receber as mensagens
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine("Mensagem recebida: {0}", message);
        };
        channel.BasicConsume(queueName, true, consumer);

       

    }
}

