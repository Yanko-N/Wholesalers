using Azure;
using Azure.Core;
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
            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token))
            {
                var logs = DbContext.UserLoginLogs.OrderByDescending(d => d.Date).FirstOrDefault(u => u.User == request.Operator);

                if (logs?.Token == request.Token)
                {
                    var UID = DbContext.UIDS.Include(m => m.UID).Include(m => m.Cobertura).FirstOrDefault(u => u.UID == request.Uid);
                    var morada = DbContext.Coberturas.FirstOrDefault(m => m.Id == UID.Cobertura.Id);
                    if (morada != null && (morada.Estado == "RESERVED" || morada.Estado == " DEACTIVATED"))
                    {

                        Thread thread = new Thread(() => Active(morada, request));
                        thread.Start();
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
                            Status = "ERROR",
                            Et = 0
                        };

                        return Task.FromResult(response);
                    }


                }
                else
                {
                    var response = new OperatorActionsReply
                    {
                        Status = "ERROR",
                        Et = 0
                    };

                    return Task.FromResult(response);
                }
            }
            else
            {
                var response = new OperatorActionsReply
                {
                    Status = "ERROR",
                    Et = 0
                };

                return Task.FromResult(response);
            }
        }

        public override Task<OperatorActionsReply> Deactivate(OperatorActionsRequest request, ServerCallContext context)
        {
            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token))
            {
                var logs = DbContext.UserLoginLogs.OrderByDescending(d => d.Date).FirstOrDefault(u => u.User == request.Operator);

                if (logs?.Token == request.Token)
                {
                    var UID = DbContext.UIDS.Include(m => m.UID).Include(m => m.Cobertura).FirstOrDefault(u => u.UID == request.Uid);
                    var morada = DbContext.Coberturas.FirstOrDefault(m => m.Id == UID.Cobertura.Id);
                    if (morada != null && (morada.Estado == " ACTIVE"))
                    {
                        Thread thread = new Thread(() => Desativar(morada, request));
                        thread.Start();

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
                            Status = "ERROR",
                            Et = 0
                        };

                        return Task.FromResult(response);
                    }


                }
                else
                {
                    var response = new OperatorActionsReply
                    {
                        Status = "ERROR",
                        Et = 0
                    };

                    return Task.FromResult(response);
                }
            }
            else
            {
                var response = new OperatorActionsReply
                {
                    Status = "ERROR",
                    Et = 0
                };

                return Task.FromResult(response);
            }
        }

        public override Task<OperatorActionsReserveReply> Reserve(OperatorActionsReserveRequest request, ServerCallContext context)
        {

            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token))
            {
                var logs = DbContext.UserLoginLogs.OrderByDescending(d => d.Date).FirstOrDefault(u => u.User == request.Operator);

                if (logs?.Token == request.Token)
                {

                    //Aqui será feito a reserva duma morada
                    if (request.Apartamento == "")
                    {
                        request.Apartamento = null;
                    }
                    var morada = DbContext.Coberturas.FirstOrDefault(m => m.Rua == request.Rua && m.Apartamento == request.Apartamento && m.Numero == request.Numero && m.Operador == request.Operator && m.Municipio == request.Municipio);

                    if (morada != null)
                    {
                        Thread thread= new Thread(()=> Reservar(morada, request));
                        thread.Start();

                        var response = new OperatorActionsReserveReply
                        {
                            Status = "OK",

                        };
                        return Task.FromResult(response);
                    }
                    else
                    {
                        var response = new OperatorActionsReserveReply
                        {
                            Status = "ERROR",

                        };
                        return Task.FromResult(response);
                    }
                }
                else
                {
                    var response = new OperatorActionsReserveReply
                    {
                        Status = "ERROR",

                    };
                    return Task.FromResult(response);

                }
            }
            else
            {
                var response = new OperatorActionsReserveReply
                {
                    Status = "ERROR",

                };
                return Task.FromResult(response);

            }
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

                    if (morada != null && morada.Estado == "ACTIVE")
                    {

                        Thread thread = new Thread(() => Terminar(morada, request));
                        thread.Start();

                       

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

        public override async Task ListUid(OperatorActionUidRequest request, IServerStreamWriter<OperatorActionUidReply> responseStream, ServerCallContext context)
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


        public void Active(Cobertura morada,OperatorActionsRequest request)
        {

            Thread.Sleep(3);
            //N está ASSINCRONO
            morada.Estado = "ACTIVE";

            //Apos a reserva será adicionado aos LOGS 
            var logOperatorEvent = new OperatorActionEvents
            {
                Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                Action = "ACTIVE",
                Cobertura = morada,
                Date = DateTime.Now
            };

            var message = $"The {request.Operator} was ACTIVATED the adress {morada.Numero},{morada.Apartamento},{morada.Municipio}";




            DbContext.OperatorActionEvents.Add(logOperatorEvent);
            DbContext.Update(morada);

            DbContext.SaveChangesAsync();


            RabbitService.SendMessage(request.Operator, message);
        }
        public  void Desativar(Cobertura morada,OperatorActionsRequest request)
        {
            Thread.Sleep(3);
            //N está ASSINCRONO
            morada.Estado = "DEACTIVATED";

            //Apos a reserva será adicionado aos LOGS 
            var logOperatorEvent = new OperatorActionEvents
            {
                Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                Action = "DEACTIVATED",
                Cobertura = morada,
                Date = DateTime.Now
            };

            var message = $"The {request.Operator} was DEACTIVATED the adress {morada.Numero},{morada.Apartamento},{morada.Municipio}";




            DbContext.OperatorActionEvents.Add(logOperatorEvent);
            DbContext.Update(morada);

            DbContext.SaveChangesAsync();


            RabbitService.SendMessage(request.Operator, message);
        }
        public  void Reservar(Cobertura morada,OperatorActionsReserveRequest request)
        {
            Thread.Sleep(3);
            morada.Modalidade = request.Modalidade;
            morada.Estado = "RESERVED";
            //Apos a reserva será adicionado aos LOGS 
            var logOperatorEvent = new OperatorActionEvents
            {
                Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                Action = "RESERVE",
                Cobertura = morada,
                Date = DateTime.Now
            };

            var message = $"The {request.Operator} was RESERVED the adress {morada.Numero},{morada.Apartamento},{morada.Municipio}";




            DbContext.OperatorActionEvents.Add(logOperatorEvent);
            DbContext.Update(morada);

            DbContext.SaveChangesAsync();


            RabbitService.SendMessage(request.Operator, message);
        }
        public void Terminar(Cobertura morada, OperatorActionsRequest request)
        {
            Thread.Sleep(3);
            morada.Estado = "TERMINATED";
            //Apos a Terminação será adicionado aos LOGS 
            var logOperatorEvent = new OperatorActionEvents
            {
                Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                Action = "TERMINATE",
                Cobertura = morada,
                Date = DateTime.Now
            };

            var message = $"The {request.Operator} was TERMINATED the adress {morada.Numero},{morada.Apartamento},{morada.Municipio}";

            DbContext.OperatorActionEvents.Add(logOperatorEvent);
            DbContext.Update(morada);

            DbContext.SaveChangesAsync();

            RabbitService.SendMessage(request.Operator, message);
        }

    }
}
