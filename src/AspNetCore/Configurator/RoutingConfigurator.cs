using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using RecShark.AspNetCore.Extensions;

namespace RecShark.AspNetCore.Configurator
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.FileProviders;

    /// <remarks>See also <seealso cref="DefaultRouteAttribute"/></remarks>
    public static class RoutingConfigurator
    {
        public static IServiceCollection AddOA3Routing(this IServiceCollection services)
        {
            services.AddRouting(
                options =>
                {
                    options.LowercaseUrls         = true;
                    options.LowercaseQueryStrings = true;
                });

            // https://github.com/microsoft/aspnet-api-versioning
            // https://github.com/microsoft/aspnet-api-versioning/wiki/New-Services-Quick-Start#aspnet-core
            // https://github.com/microsoft/aspnet-api-versioning/wiki/API-Documentation#aspnet-core
            // https://github.com/microsoft/aspnet-api-versioning/wiki/Version-Advertisement
            services.AddApiVersioning(
                options =>
                {
                    options.ApiVersionReader                    = new UrlSegmentApiVersionReader();
                    options.AssumeDefaultVersionWhenUnspecified = true;
                });

            services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat           = "'v'V";
                    options.SubstituteApiVersionInUrl = true;
                });

            return services;
        }

        public static IApplicationBuilder UseEmbeddedFiles(this IApplicationBuilder app, Type type)
        {
            var options = new StaticFileOptions()
            {
                FileProvider = new ManifestEmbeddedFileProvider(type.Assembly, "wwwroot"),
                RequestPath  = new PathString("")
            };
            return app.UseStaticFiles(options);
        }
    }
}
