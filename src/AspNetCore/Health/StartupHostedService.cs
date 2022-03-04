namespace RecShark.AspNetCore.Health
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public interface IStartupHostedService : IHostedService
    {
        public bool HasCompleted { get; set; }
    }

    public abstract class StartupHostedService : BackgroundService, IStartupHostedService
    {
        public bool HasCompleted { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.InnerExecute(stoppingToken);

            this.HasCompleted = true;
        }

        public abstract Task InnerExecute(CancellationToken stoppingToken);
    }
}