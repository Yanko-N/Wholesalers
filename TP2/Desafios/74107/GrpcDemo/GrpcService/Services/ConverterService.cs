using Grpc.Core;

namespace GrpcService.Services {
    public class ConverterService : Converter.ConverterBase{
        private readonly ILogger<ConverterService> _logger;
        public ConverterService(ILogger<ConverterService> logger) {
            _logger = logger;
        }

        //Temperature
        public override Task<Out> C2F(In request, ServerCallContext context) {
            return Task.FromResult(new Out{Msg = (request.In_ * 1.8) + 32 });
        }

        public override Task<Out> F2C(In request, ServerCallContext context) {
            return Task.FromResult(new Out{Msg = (request.In_ - 32) * .5556});
        }

        //Money
        public override Task<Out> D2E(In request, ServerCallContext context) {
            return Task.FromResult(new Out{Msg = request.In_ * 0.91});
        }

        public override Task<Out> E2D(In request, ServerCallContext context) {
            return Task.FromResult(new Out { Msg = request.In_ * 1.1 });
        }

        //Distance
        public override Task<Out> M2K(In request, ServerCallContext context) {
            return Task.FromResult(new Out { Msg = request.In_ * 1.609 });
        }

        public override Task<Out> K2M(In request, ServerCallContext context) {
            return Task.FromResult(new Out { Msg = request.In_ * 0.621 });
        }
    }
}
        
