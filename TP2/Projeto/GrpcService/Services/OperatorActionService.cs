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
                    var UID = DbContext.UIDS.Include(m => m.Cobertura).FirstOrDefault(u => u.UID == request.Uid);
                    var morada = DbContext.Coberturas.FirstOrDefault(m => m.Id == UID.Cobertura.Id);
                    if (morada != null && (morada.Estado == "RESERVED" || morada.Estado == "DEACTIVATED"))
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
                            Status = "ERROR The adress isn't Reserved or is Deactivated",
                            Et = 0
                        };

                        return Task.FromResult(response);
                    }


                }
                else
                {
                    var response = new OperatorActionsReply
                    {
                        Status = "ERROR the Login no longer is accepted",
                        Et = 0
                    };

                    return Task.FromResult(response);
                }
            }
            else
            {
                var response = new OperatorActionsReply
                {
                    Status = "ERROR Bad Request",
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
                    var UID = DbContext.UIDS.Include(m => m.Cobertura).FirstOrDefault(u => u.UID == request.Uid);
                    var morada = DbContext.Coberturas.FirstOrDefault(m => m.Id == UID.Cobertura.Id);
                    if (morada != null && (morada.Estado == "ACTIVE"))
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
                            Status = "ERROR the adress isn't Active",
                            Et = 0
                        };

                        return Task.FromResult(response);
                    }


                }
                else
                {
                    var response = new OperatorActionsReply
                    {
                        Status = "ERROR the Login is no longer accepted",
                        Et = 0
                    };

                    return Task.FromResult(response);
                }
            }
            else
            {
                var response = new OperatorActionsReply
                {
                    Status = "ERROR Bad Request",
                    Et = 0
                };

                return Task.FromResult(response);
            }
        }

        public override async Task<OperatorActionsReserveReply> Reserve(OperatorActionsReserveRequest request,ServerCallContext context) {
                if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token)) {
                var logs = DbContext.UserLoginLogs.OrderByDescending(d => d.Date)
                    .FirstOrDefault(u => u.User == request.Operator);

                if (logs?.Token == request.Token) {

                    //Aqui será feito a reserva duma morada
                    string? apt;
                    if (request.Apartamento == "") {
                        apt = "";
                    } else apt = request.Apartamento;

                    if (DbContext.Coberturas.Any(m =>
                            m.Rua == request.Rua && m.Apartamento == apt && m.Numero == request.Numero && m.Municipio == request.Municipio &&
                            m.Estado != "TERMINATED"))
                    {
                        var response = new OperatorActionsReserveReply
                        {
                            Status = "ERROR",

                        };
                        return await Task.FromResult(response);
                    }
                    var morada = DbContext.Coberturas.FirstOrDefault(m =>
                            m.Rua == request.Rua && m.Apartamento == apt && m.Numero == request.Numero &&
                             m.Municipio == request.Municipio &&
                            m.Estado == "TERMINATED");
                    

                    if (morada != null) {

                        var uid = DbContext.UIDS.Include(c => c.Cobertura).FirstOrDefault(c => c.Cobertura == morada);
                        if(uid != null) DbContext.UIDS.Remove(uid);
                        morada.Operador = request.Operator;
                        morada.Modalidade = request.Modalidade;
                        morada.Estado = "RESERVED";
                        var logOperatorEvent = new OperatorActionEvents {
                            Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                            Action = "RESERVE",
                            Cobertura = morada,
                            Date = DateTime.Now
                        };

                        var entryUID = new Uid {
                            Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                            Cobertura = morada,
                            UID = Guid.NewGuid().ToString().Replace("-", "")
                        };

                        DbContext.OperatorActionEvents.Add(logOperatorEvent);
                        DbContext.UIDS.Add(entryUID);
                        DbContext.Update(morada);

                        await DbContext.SaveChangesAsync();

                            var response = new OperatorActionsReserveReply {
                            Status = "OK",
                            Uid = DbContext.UIDS.Include(m => m.Cobertura).FirstOrDefault(m => m.Cobertura == morada)
                                .UID

                        };
                            return await Task.FromResult(response);

                 
                       



                    } else {
                        var response = new OperatorActionsReserveReply {
                            Status = "ERROR",

                        };
                        return await Task.FromResult(response);
                    }
                    
                } else {
                    var response = new OperatorActionsReserveReply {
                        Status = "ERROR",

                    };
                    return await Task.FromResult(response);

                }
            } else {
                var response = new OperatorActionsReserveReply {
                    Status = "ERROR",

                };
                return await Task.FromResult(response);
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
                    var uid = DbContext.UIDS.Include(c=> c.Cobertura).FirstOrDefault(m => m.UID == request.Uid);
                    var morada = DbContext.Coberturas.FirstOrDefault(m => m.Id == uid.Cobertura.Id);

                    if (morada != null && morada.Estado == "DEACTIVATED")
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
                            Status = "Error the adress ins't Deactivated",
                            Et = 0
                        };
                        return Task.FromResult(response);
                    }
                }
                else
                {
                    var response = new OperatorActionsReply
                    {
                        Status = "ERROR The login is no longer accepted",
                        Et = 0
                    };
                    return Task.FromResult(response);

                }
            }
            else
            {
                var response = new OperatorActionsReply
                {
                    Status = "ERROR Bad Request",
                    Et = 0
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
                    List<Uid> lista = DbContext.UIDS.Include(c=> c.Cobertura).Include(o => o.Operator)
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

            Thread.Sleep(3000);
            //N está ASSINCRONO
            morada.Estado = "ACTIVE";

            //Apos a reserva será adicionado aos LOGS 
            var logOperatorEvent = new OperatorActionEvents
            {
                Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                Action = "ACTIVATE",
                Cobertura = morada,
                Date = DateTime.Now
            };

            var message = $"The {request.Operator} was ACTIVATED the adress {morada.Numero},{morada.Rua},{morada.Municipio},";





            DbContext.OperatorActionEvents.Add(logOperatorEvent);
            DbContext.Update(morada);

            DbContext.SaveChangesAsync();


            RabbitService.SendMessage(request.Operator, message);
        }
        public  void Desativar(Cobertura morada,OperatorActionsRequest request)
        {
            Thread.Sleep(3000);
            
            morada.Estado = "DEACTIVATED";

            //Apos a reserva será adicionado aos LOGS 
            var logOperatorEvent = new OperatorActionEvents
            {
                Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                Action = "DEACTIVATE",
                Cobertura = morada,
                Date = DateTime.Now
            };

            var message = $"The {request.Operator} was DEACTIVATED the adress {morada.Numero},{morada.Rua},{morada.Municipio},";




            DbContext.OperatorActionEvents.Add(logOperatorEvent);
            DbContext.Update(morada);

            DbContext.SaveChangesAsync();


            RabbitService.SendMessage(request.Operator, message);
        }
        public void Terminar(Cobertura morada, OperatorActionsRequest request)
        {
            Thread.Sleep(3000);
            morada.Estado = "TERMINATED";
            //Apos a Terminação será adicionado aos LOGS 
            var logOperatorEvent = new OperatorActionEvents
            {
                Operator = DbContext.Users.FirstOrDefault(u => u.Username == request.Operator),
                Action = "TERMINATE",
                Cobertura = morada,
                Date = DateTime.Now
            };

            var message = $"The {request.Operator} was TERMINATE the adress {morada.Numero},{morada.Rua},{morada.Municipio},";


            DbContext.OperatorActionEvents.Add(logOperatorEvent);
            DbContext.Update(morada);

            DbContext.SaveChangesAsync();

            RabbitService.SendMessage(request.Operator, message);
        }

    }
}
