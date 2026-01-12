using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Api.Health
{
    public class HealthCheckExtensions
    {
        public void AddHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks();
        }
    }
}
