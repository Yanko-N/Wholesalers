using GrpcService.Services;

namespace GrpcService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<CustomerService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Comunication with gRPC endpoints must be made through");
                });
            });
        }
    }
}
