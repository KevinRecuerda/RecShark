namespace RecShark.AspNetCore.Health
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class StartupHealthChecker : IHealthCheck
    {
        public StartupHealthChecker(IEnumerable<IStartupHostedService> services)
        {
            this.Services = services.ToList();
        }

        private List<IStartupHostedService> Services { get; }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (this.Services.Any(s => !s.HasCompleted))
                return Task.FromResult(HealthCheckResult.Unhealthy("The startup tasks are still running."));

            return Task.FromResult(HealthCheckResult.Healthy("All startup tasks are completed."));
        }
    }
}