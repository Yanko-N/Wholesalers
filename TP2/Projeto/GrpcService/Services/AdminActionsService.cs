using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcService.Services {
    public class AdminActionsService : AdminActions.AdminActionsBase {
        public readonly dataContext dataContext = new dataContext();

        public override async Task ListAllCoberturas(AdminActionsListAllCoberturas request,
            IServerStreamWriter<AdminActionsCoberturasReply> responseStream,
            ServerCallContext context) {

            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token)) {

                if (dataContext.Users.First(u => u.Username == request.Operator).isAdmin) {

                    var logs = dataContext.UserLoginLogs.OrderByDescending(d => d.Date)
                        .FirstOrDefault(u => u.User == request.Operator);

                    if (logs?.Token == request.Token) {

                        var coberturas = dataContext.Coberturas.Where(c => c.Estado != "TERMINATED");

                        foreach (var c in coberturas) {
                            var temp = new AdminActionsCoberturasReply {
                                Municipio = c.Municipio,
                                Rua = c.Rua,
                                Numero = c.Numero,
                                Apartamento = c.Apartamento,
                                Estado = c.Estado,
                                Operator = c.Operador,
                            };
                            await responseStream.WriteAsync(temp);
                        }
                    }
                }
            }
        }

        public override async Task ListCoberturasOperator(AdminActionsCoberturasOperatorRequest request,
            IServerStreamWriter<AdminActionsCoberturasReply> responseStream,
            ServerCallContext context) {

            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token)) {

                if (dataContext.Users.First(u => u.Username == request.Operator).isAdmin) {

                    var logs = dataContext.UserLoginLogs.OrderByDescending(d => d.Date)
                        .FirstOrDefault(u => u.User == request.Operator);

                    if (logs?.Token == request.Token) {
                        var coberturas =
                            dataContext.Coberturas.Where(
                                c => c.Estado != "TERMINATED" && c.Operador == request.Operatorsearch);

                        foreach (var c in coberturas) {
                            var temp = new AdminActionsCoberturasReply {
                                Municipio = c.Municipio,
                                Rua = c.Rua,
                                Numero = c.Numero,
                                Apartamento = c.Apartamento,
                                Estado = c.Estado,
                                Operator = c.Operador
                            };
                            await responseStream.WriteAsync(temp);
                        }
                    }
                }
            }
        }

        public override async Task ListServices(AdminActionsServicesRequest request,
            IServerStreamWriter<AdminActionsServicesReply> responseStream, ServerCallContext context) {
            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token)) {
                if (dataContext.Users.First(u => u.Username == request.Operator).isAdmin) {
                    var logs = dataContext.UserLoginLogs.OrderByDescending(d => d.Date)
                        .FirstOrDefault(u => u.User == request.Operator);
                    if (logs?.Token == request.Token) {
                        var services = new List<OperatorActionEvents>();
                        if (request.Active)
                            services = dataContext.OperatorActionEvents.Include(s => s.Cobertura)
                                .Include(s => s.Operator)
                                .Where(s => s.Action == "ACTIVATE").ToList();
                        if (request.Deactivated)
                            services.AddRange(dataContext.OperatorActionEvents.Include(s => s.Cobertura)
                                .Include(s => s.Operator).Where(s => s.Action == "DEACTIVATE").ToList());
                        if (request.Terminated)
                            services.AddRange(dataContext.OperatorActionEvents.Include(s => s.Cobertura)
                                .Include(s => s.Operator).Where(s => s.Action == "TERMINATE").ToList());
                        if (request.Reserved)
                            services.AddRange(dataContext.OperatorActionEvents.Include(s => s.Cobertura)
                                .Include(s => s.Operator).Where(s => s.Action == "RESERVE").ToList());

                        services = services.OrderByDescending(d => d.Date).ToList();

                        foreach (var s in services) {

                            AdminActionsServicesReply temp = new AdminActionsServicesReply {
                                Municipio = s.Cobertura.Municipio,
                                Rua = s.Cobertura.Rua,
                                Numero = s.Cobertura.Numero,
                                Apartamento = s.Cobertura.Apartamento,
                                Operator = s.Operator.Username,
                                Action = s.Action,
                                Timestamp = s.Date.ToString()
                            };
                            await responseStream.WriteAsync(temp);
                        }
                    }
                }
            }
        }
    }
}