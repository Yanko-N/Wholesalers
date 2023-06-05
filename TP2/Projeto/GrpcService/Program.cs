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

        RabbitService.CreateTopics();

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



}





