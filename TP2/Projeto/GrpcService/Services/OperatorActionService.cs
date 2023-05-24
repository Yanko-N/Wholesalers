using Azure;
using Grpc.Core;
using GrpcService.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace GrpcService.Services
{
    public class OperatorActionService : OperatorActions.OperatorActionsBase
    {
        public dataContext DbContext = new dataContext();

        public override Task<OperatorActionsReply> Activate(OperatorActionsRequest request, ServerCallContext context)
        {

            return base.Activate(request, context);
        }

        public override Task<OperatorActionsReply>
            Deactivate(OperatorActionsRequest request, ServerCallContext context)
        {
            return base.Deactivate(request, context);
        }

        public override Task<OperatorActionsReserveReply> Reserve(OperatorActionsReserveRequest request,
            ServerCallContext context)
        {

            return base.Reserve(request, context);
        }

        public override Task<OperatorActionsReply> Terminate(OperatorActionsRequest request, ServerCallContext context)
        {
            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token))
            {
                var logs = DbContext.UserLoginLogs.OrderByDescending(d => d.Date).FirstOrDefault(u => u.User == request.Operator);

                if (logs?.Token == request.Token)
                {

                    //Aqui será feito a terminação duma morada
                    var uid = DbContext.UIDS.Include(m => m.UID).FirstOrDefault(m => m.UID == request.Uid);
                    var morada = DbContext.Coberturas.FirstOrDefault(m => m.Id == uid.Cobertura.Id);

                    if (morada != null)
                    {
                        morada.Estado = "TERMINATED";
                        //Apos a Terminação será adicionado aos LOGS 
                        var logOperatorEvent = new OperatorActionEvents
                        {
                            Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                            Action = "TERMINATED",
                            Cobertura = morada,
                            Date = DateTime.Now
                        };

                        //Falta Agora Supostamente Enviar a Mensagem pelo RABBIT MQ
                        var message = $"The {request.Operator} was TERMINATED the adress {morada.Numero},{morada.Apartamento},{morada.Municipio}";
                        RabbitService.SendMessage(request.Operator, message);



                        DbContext.OperatorActionEvents.Add(logOperatorEvent);
                        DbContext.Update(morada);

                        DbContext.SaveChangesAsync();

                        var response = new OperatorActionsReply
                        {
                            Status = "OK",
                            Et = 3
                        };
                        return Task.FromResult(response);
                    }
                    else
                    {
                        var response = new OperatorActionsReply
                        {
                            Status = "Error",
                            Et = 3
                        };
                        return Task.FromResult(response);
                    }
                }
                else
                {
                    var response = new OperatorActionsReply
                    {
                        Status = "ERROR",
                        Et = 3
                    };
                    return Task.FromResult(response);

                }
            }
            else
            {
                var response = new OperatorActionsReply
                {
                    Status = "ERROR",
                    Et = 3
                };
                return Task.FromResult(response);

            }

        }

        public override async Task ListUid(OperatorActionUidRequest request,
            IServerStreamWriter<OperatorActionUidReply> responseStream, ServerCallContext context)
        {

            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token))
            {

                var logs = DbContext.UserLoginLogs.OrderByDescending(d => d.Date)
                    .FirstOrDefault(u => u.User == request.Operator);

                if (logs?.Token == request.Token)
                {
                    List<Uid> lista = DbContext.UIDS.Include(o => o.Operator)
                        .Where(o => o.Operator.Username == request.Operator).ToList();

                    foreach (var item in lista)
                    {
                        var temp = new OperatorActionUidReply
                        {
                            Apartamento = item.Cobertura.Apartamento,
                            Municipio = item.Cobertura.Municipio,
                            Numero = item.Cobertura.Numero,
                            Rua = item.Cobertura.Rua,
                            Uid = item.UID
                        };
                        await responseStream.WriteAsync(temp);
                    }

                }

            }
        }
    }
}
