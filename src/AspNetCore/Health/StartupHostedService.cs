namespace RecShark.AspNetCore.Health
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class StartupHostedService : BackgroundService, IHostedService
    {
        public StartupHostedService(ILogger logger)
        {
            this.Logger = logger;
        }

        public virtual string  Name         => this.GetType().Name;
        public         bool?   HasCompleted { get; set; }
        public         ILogger Logger       { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            try
            {
                await this.InnerExecute(stoppingToken);

                this.HasCompleted = !stoppingToken.IsCancellationRequested;
                if (stoppingToken.IsCancellationRequested)
                    this.Logger.LogInformation("Service {Name} has successfully completed", this.Name);
                else
                    this.Logger.LogWarning("Task has been cancelled when executing service {Name}", this.Name);
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "An error occured when running service {Name}", this.Name);
                this.HasCompleted = false;
            }
        }

        public abstract Task InnerExecute(CancellationToken stoppingToken);
    }
}