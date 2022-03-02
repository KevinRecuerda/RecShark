namespace RecShark.AspNetCore.Health
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public interface IHealthStartupService
    {
        public bool HasCompleted { get; set; }
    }

    public abstract class BaseHealthStartupService : BackgroundService, IHealthStartupService
    {
        public bool HasCompleted { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.InnerExecute();

            this.HasCompleted = true;
        }

        public abstract Task InnerExecute();
    }
}