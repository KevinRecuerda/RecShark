namespace RecShark.AspNetCore.Configurator
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class MonitoringConfigurator
    {
        public static IApplicationBuilder UseMonitoring(
            this IApplicationBuilder                app,
            IHostApplicationLifetime                applicationLifetime,
            Action<IDiagnosticContext, HttpContext> enrichDiagnosticContext = null)
        {
            app.UseLogging(applicationLifetime, enrichDiagnosticContext);
            return app;
        }

        public static IServiceCollection AddMonitoring(
            this IServiceCollection      services,
            IConfiguration               configuration,
            Action<IHealthChecksBuilder> healthCheckBuilder = null,
            bool                         useLoggerJsonFormatter = false,
            Action<LoggerConfiguration>  loggerConfigurator = null)
        {
            services.AddHealthChecks(healthCheckBuilder);
            services.AddLogging(configuration, useLoggerJsonFormatter, loggerConfigurator);
            return services;
        }
    }
}
