using RabbitMQ.Client;
using System.Text;

namespace GrpcService.Services
{
    public static class RabbitService
    {
        public static readonly dataContext dB = new dataContext();
        public static ConnectionFactory factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        public static IConnection connection = factory.CreateConnection();
        public static IModel channel = connection.CreateModel();

        /// <summary>
        /// Função para gerar os topicos para o Rabbit MQ
        /// </summary>
        public static void CreateTopics()
        {
            List<string> operadoras = dB.Coberturas.Select(x => x.Operador).Distinct().ToList();

            foreach (var op in operadoras)
            {
                // Declaração da exchange do tipo "topic"
                channel.ExchangeDeclare(op, ExchangeType.Topic);
            }
            //QUEUE PARA TODOS
            channel.QueueDeclare(queue: "",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

        }

        /// <summary>
        /// Função para enviar mensagem para uma certo topico
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        public static void SendMessage(string topic, string? message)
        {
            var newMessage = $"[Server] Sent {message}";
            var body = Encoding.UTF8.GetBytes(newMessage);

            channel.BasicPublish(exchange: topic,
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine(newMessage);
        }

    }
}
