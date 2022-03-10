namespace RecShark.AspNetCore.Health
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public interface IStartupHostedService : IHostedService
    {
        public string Name         { get; }
        public bool?  HasCompleted { get; set; }
    }

    public abstract class StartupHostedService : BackgroundService, IStartupHostedService
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
            await Task.Run(() => this.InnerExecute(stoppingToken), stoppingToken)
                      .ContinueWith(
                           t =>
                           {
                               this.HasCompleted = t.IsCompletedSuccessfully;
                               if (t.IsCompletedSuccessfully)
                                   this.Logger.LogInformation("Service {Name} has successfully completed", this.Name);

                               if (t.IsCanceled)
                                   this.Logger.LogWarning("Task has been cancelled when executing service {Name}", this.Name);

                               if (t.IsFaulted)
                                   this.Logger.LogError(t.Exception, "An error occured when running service {Name}", this.Name);
                           });
        }

        public abstract Task InnerExecute(CancellationToken stoppingToken);
    }
}