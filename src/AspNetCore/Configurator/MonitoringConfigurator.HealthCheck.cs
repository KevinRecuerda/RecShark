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

    public static class HealthCheckConfigurator
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, Action<IHealthChecksBuilder> healthCheckBuilder)
        {
            var builder = services.AddHealthChecks();
            healthCheckBuilder?.Invoke(builder);
            return services;
        }

        public static IEndpointConventionBuilder MapHealthChecks(this IEndpointRouteBuilder options)
        {
            return options.MapHealthChecks(
                "/health",
                new HealthCheckOptions()
                {
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
