namespace RecShark.AspNetCore.Health
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using RecShark.Extensions;

    public class StartupHealthChecker : IHealthCheck
    {
        public StartupHealthChecker(IEnumerable<IStartupHostedService> services)
        {
            this.Services = services.ToList();
        }

        private List<IStartupHostedService> Services { get; }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new ReadOnlyDictionary<string, object>(
                this.Services.ToDictionary(
                    s => s.GetType().Name,
                    s => (object) s.HasCompleted.ToString("Completed", "Failed", "Running")));

            var healthCheckResult = this.Services.Any(s => s.HasCompleted == true)
                                        ? HealthCheckResult.Healthy(data: data)
                                        : HealthCheckResult.Unhealthy(data: data);
            return Task.FromResult(healthCheckResult);
        }
    }
}