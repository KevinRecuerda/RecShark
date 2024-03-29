﻿namespace RecShark.AspNetCore.Health
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

                this.HasCompleted = true;
                this.Logger.LogInformation("Startup service {Name} completed", this.Name);
            }
            catch (OperationCanceledException)
            {
                this.HasCompleted = false;
                this.Logger.LogWarning("Startup service {Name} cancelled", this.Name);
            }
            catch (Exception e)
            {
                this.HasCompleted = false;
                this.Logger.LogError(e, "Startup service {Name} failed", this.Name);
            }
        }

        public abstract Task InnerExecute(CancellationToken stoppingToken);
    }
}