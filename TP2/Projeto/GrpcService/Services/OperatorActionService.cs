using Grpc.Core;
using GrpcService.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace GrpcService.Services {
    public class OperatorActionService : OperatorActions.OperatorActionsBase {
        public dataContext DbContext = new dataContext();

        public override Task<OperatorActionsReply> Activate(OperatorActionsRequest request, ServerCallContext context) {

            return base.Activate(request, context);
        }

        public override Task<OperatorActionsReply>
            Deactivate(OperatorActionsRequest request, ServerCallContext context) {
            return base.Deactivate(request, context);
        }

        public override Task<OperatorActionsReserveReply> Reserve(OperatorActionsReserveRequest request,
            ServerCallContext context) {

            return base.Reserve(request, context);
        }

        public override Task<OperatorActionsReply>
            Terminate(OperatorActionsRequest request, ServerCallContext context) {
            return base.Terminate(request, context);
        }

        public override async Task ListUid(OperatorActionUidRequest request,
            IServerStreamWriter<OperatorActionUidReply> responseStream, ServerCallContext context) {
            if (!String.IsNullOrWhiteSpace(request.Operator) && !String.IsNullOrWhiteSpace(request.Token)) {
                var logs = DbContext.UserLoginLogs.OrderByDescending(d => d.Date)
                    .FirstOrDefault(u => u.User == request.Operator);
                if (logs?.Token == request.Token) {
                    List<Uid> lista = DbContext.UIDS.Include(o => o.Operator)
                        .Where(o => o.Operator.Username == request.Operator).ToList();
                    foreach (var item in lista) {
                        var temp = new OperatorActionUidReply {
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
