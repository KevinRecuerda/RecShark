namespace RecShark.AspNetCore.Health
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using RecShark.Extensions;

    public class StartupHealthChecker : IHealthCheck
    {
        public StartupHealthChecker(IEnumerable<IHostedService> services)
        {
            this.Services = services.OfType<StartupHostedService>().ToList();
        }

        private List<StartupHostedService> Services { get; }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new ReadOnlyDictionary<string, object>(
                this.Services.ToDictionary(
                    s => s.Name,
                    s => (object) s.HasCompleted.ToString("Completed", "Failed", "Running")));

            var healthCheckResult = this.Services.All(s => s.HasCompleted == true)
                                        ? HealthCheckResult.Healthy(data: data)
                                        : HealthCheckResult.Unhealthy(data: data);
            return Task.FromResult(healthCheckResult);
        }
    }
}