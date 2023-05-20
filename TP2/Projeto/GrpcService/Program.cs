using GrpcService.Services;
using System.Text;
using RabbitMQ.Client;
using GrpcService;

var builder = WebApplication.CreateBuilder(args);



//Rabbit MQ 
var factory = new ConnectionFactory()
{
    HostName = "localhost"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declaração da exchange do tipo "topic"
channel.ExchangeDeclare("EVENT", ExchangeType.Topic);


//QUEUE PARA TODOS
channel.QueueDeclare(queue: "",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

/*
        SE FOR PRECISO OUTRAS ROUTES É NECESSÁRIO ADICIONAR 
        Secalhar ir a DB e verificar as operadoras existentes e criar uma queue/ route para casa operadora
*/

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AuthService>();
app.MapGrpcService<AdminActionsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


//Função para enviar mensagem para uma certa queue ou para todos
void sendMessage(IModel channel,string ? queue,string? message)
{
    if(queue == null)
    {
        queue = "";
    }
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "EVENT",
                         routingKey: queue,
                         basicProperties: null,
                         body: body);
    Console.WriteLine($" [Server] Sent {message}");
}