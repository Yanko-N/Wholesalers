using Grpc.Core;
using GrpcService.Protos;

namespace GrpcService.Services
{
    public class conversorService : converterProtocolo.converterProtocoloBase
    {
        private readonly ILogger<conversorService> _logger;

        public conversorService(ILogger<conversorService> logger)
        {
            _logger = logger;
        }

        public Task<Out> Euro2Dollar(In request, ServerCallContext context)
        {
            var result = request.In_ * 1.1;
            return Task.FromResult(new Out { Msg = result });
        }
        public Task<Out> Dollar2Euro(In request, ServerCallContext context)
        {
            var result = request.In_ * 0.91;
            return Task.FromResult(new Out { Msg = result });
        }
        public Task<Out> Miles2Km(In request, ServerCallContext context)
        {
            var result = request.In_ * 1.6;
            return Task.FromResult(new Out { Msg = result });
        }
        public Task<Out> Km2Miles(In request, ServerCallContext context)
        {
            var result = request.In_ * 0.62;
            return Task.FromResult(new Out { Msg = result });
        }
        public Task<Out> Celsius2Fahrenheint(In request, ServerCallContext context)
        {
            var result = request.In_ * 1.8 + 32;
            return Task.FromResult(new Out { Msg = result });
        }

        public Task<Out> Fahrenheint2Celsius(In request, ServerCallContext context)
        {
            var result = (request.In_ - 32) * .5556;
            return Task.FromResult(new Out { Msg = result });
        }
   
    }
}
