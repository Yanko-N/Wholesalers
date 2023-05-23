using GrpcService.Services;
using System.Text;
using RabbitMQ.Client;
using GrpcService;



public class ServerService
{

    static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        var dB = new dataContext();

        //#region RABBITMQ
        //var factory = new ConnectionFactory()
        //{
        //    HostName = "localhost"
        //};

        //using var connection = factory.CreateConnection();
        //using var channel = connection.CreateModel();



        //List<string> operadoras = dB.Coberturas.Select(x => x.Operador).Distinct().ToList();

        //foreach (var op in operadoras)
        //{
        //    // Declaração da exchange do tipo "topic"
        //    channel.ExchangeDeclare(op, ExchangeType.Topic);
        //}
        ////QUEUE PARA TODOS
        //channel.QueueDeclare(queue: "",
        //                     durable: false,
        //                     exclusive: false,
        //                     autoDelete: false,
        //                     arguments: null);

        //#endregion

        // Add services to the container.
        builder.Services.AddGrpc();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<AuthService>();
        app.MapGrpcService<AdminActionsService>();
        app.MapGrpcService<OperatorActionService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }

    
    /// <summary>
    /// Função para enviar mensagem para uma certo topico
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="topic"></param>
    /// <param name="message"></param>
    static void sendMessage(IModel channel, string topic, string? message)
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





