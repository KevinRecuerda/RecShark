namespace RecShark.AspNetCore.Configurator
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RecShark.AspNetCore.Health;

    public static class HealthCheckConfigurator
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, Action<IHealthChecksBuilder> healthCheckBuilder)
        {
            var builder = services.AddHealthChecks();
            healthCheckBuilder?.Invoke(builder);
            return services;
        }

        public static void AddStartupHealthService<TStartupService>(this IServiceCollection services)
            where TStartupService : BaseHealthStartupService
        {
            services.AddSingleton<IHealthStartupService, TStartupService>();
            services.AddHostedService(sp => sp.GetService<IHealthStartupService>() as TStartupService);
        }

        public static IApplicationBuilder MapHealthChecks(this IApplicationBuilder app)
        {
            return app.UseEndpoints(
                options =>
                {
                    options.MapHealthChecks("/healthz/live",    _ => false);
                    options.MapHealthChecks("/healthz/startup", "startup");
                    options.MapHealthChecks("/healthz/ready",   "ready");
                });
        }
        
        public static IEndpointConventionBuilder MapHealthChecks(this IEndpointRouteBuilder options)
        {
            return options.MapHealthChecks("/health");
        }

        public static IEndpointConventionBuilder MapHealthChecks(this IEndpointRouteBuilder options, string pattern, params string[] tags)
        {
            return options.MapHealthChecks(pattern, check => !tags.Any() || tags.Any(tag => check.Tags.Contains(tag)));
        }

        public static IEndpointConventionBuilder MapHealthChecks(
            this IEndpointRouteBuilder          options,
            string                              pattern,
            Func<HealthCheckRegistration, bool> predicate = null)
        {
            return options.MapHealthChecks(
                pattern,
                new HealthCheckOptions
                {
                    Predicate      = predicate,
                    ResponseWriter = WriteResponse
                });
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var results = result.Entries.Select(
                pair =>
                    new JProperty(
                        pair.Key,
                        new JObject(
                            new JProperty("status",      pair.Value.Status.ToString()),
                            new JProperty("description", pair.Value.Description),
                            new JProperty("data",        new JObject(pair.Value.Data.Select(p => new JProperty(p.Key, p.Value)))))));

            var json = new JObject(
                new JProperty("status",  result.Status.ToString()),
                new JProperty("results", new JObject(results)));

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
        }
    }
}