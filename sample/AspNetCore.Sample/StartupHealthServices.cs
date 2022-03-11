namespace RecShark.AspNetCore.Sample
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using RecShark.AspNetCore.Health;

    public class ShortStartupHealthService : StartupHostedService
    {
        public ShortStartupHealthService(ILogger<ShortStartupHealthService> logger) : base(logger) { }

        public override Task InnerExecute(CancellationToken stoppingToken)
        {
            return Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

    public class FailStartupHealthService : StartupHostedService
    {
        public FailStartupHealthService(ILogger<FailStartupHealthService> logger) : base(logger) { }

        public override Task InnerExecute(CancellationToken stoppingToken)
        {
            throw new NotImplementedException("Exception should be caught");
        }
    }

    public class FailAsyncStartupHealthService : StartupHostedService
    {
        public FailAsyncStartupHealthService(ILogger<FailAsyncStartupHealthService> logger) : base(logger) { }

        public override async Task InnerExecute(CancellationToken stoppingToken)
        {
            void Action() => throw new NotImplementedException("Exception should be caught");

            await Task.Run(Action, stoppingToken);
        }
    }

    public class CanceledStartupHealthService : StartupHostedService
    {
        public CanceledStartupHealthService(ILogger<CanceledStartupHealthService> logger) : base(logger) { }

        public override Task InnerExecute(CancellationToken stoppingToken)
        {
            var s = new CancellationTokenSource();
            s.Cancel();
            return Task.FromCanceled(s.Token);
        }
    }
}