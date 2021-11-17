namespace RecShark.AspNetCore.Configurator
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using RecShark.AspNetCore.Extensions;

    public static class SecurityConfigurator
    {
        public static IApplicationBuilder UseSecurity(this IApplicationBuilder app)
        {
            app.UseCors()
               .UseAuthentication()
               .UseAuthorization();

            return app;
        }

        public static IServiceCollection AddCorsWildcard(
            this IServiceCollection services,
            string[]                allowOrigins   = null,
            string[]                exposedHeaders = null)
        {
            services.AddCors(
                options =>
                {
                    options.AddDefaultPolicy(
                        builder =>
                        {
                            builder.WithOrigins(allowOrigins)
                                   .SetSubDomainsAllowed()
                                   .WithExposedHeaders(exposedHeaders ?? new[] { "Content-Disposition" })
                                   .AllowAnyMethod()
                                   .AllowAnyHeader()
                                   .AllowCredentials();
                        });
                });
            return services;
        }

        public static IServiceCollection AddAuthorizationScope(this IServiceCollection services, string[] requiredScopes, string[] optionalScopes)
        {
            services.AddAuthorization(
                options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireScopes(requiredScopes).Build();

                    foreach (var scope in optionalScopes)
                        options.AddPolicy(scope, policy => policy.RequireScopes(scope));
                });
            return services;
        }
    }
}
