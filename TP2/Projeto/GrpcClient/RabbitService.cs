using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace GrpcClient
{
    internal class RabbitService
    {

        // Configuração da conexão com o RabbitMQ
        public static ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
        public static IConnection connection = factory.CreateConnection();


        public static IModel channelRabbit = connection.CreateModel();


       public static void ConnectRabitMQ(string topic)
        {

            channelRabbit.ExchangeDeclare(exchange: topic, type: ExchangeType.Topic);
            // declare a server-named queue


            // Criação de uma fila exclusiva e vinculação à exchange com uma chave de roteamento específica
            var queueName = channelRabbit.QueueDeclare().QueueName;
            channelRabbit.QueueBind(queueName, topic, "");

            // Configuração do consumidor para receber as mensagens
            var consumer = new EventingBasicConsumer(channelRabbit);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("\nMensagem recebida: {0}", message);
            };
            channelRabbit.BasicConsume(queueName, true, consumer);
        }

    }
}