namespace RecShark.AspNetCore.Health
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public interface IStartupHostedService : IHostedService
    {
        public bool? HasCompleted { get; set; }
    }

    public abstract class StartupHostedService : BackgroundService, IStartupHostedService
    {
        public StartupHostedService(ILogger logger)
        {
            this.Logger = logger;
        }

        public ILogger Logger       { get; set; }
        public bool?   HasCompleted { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => this.InnerExecute(stoppingToken), stoppingToken)
                      .ContinueWith(
                           t =>
                           {
                               this.HasCompleted = t.IsCompletedSuccessfully;

                               var type = this.GetType().Name;
                               if (t.IsCanceled)
                                   this.Logger.LogWarning("Task has been cancelled when executing service {Type}", type);

                               if (t.IsFaulted)
                                   this.Logger.LogError(t.Exception, "An error occured when running service {Type}", type);
                           });
        }

        public abstract Task InnerExecute(CancellationToken stoppingToken);
    }
}