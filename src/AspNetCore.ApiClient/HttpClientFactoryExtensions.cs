namespace RecShark.AspNetCore.ApiClient
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Polly;
    using RecShark.AspNetCore.ApiClient.Security;
    using RecShark.DependencyInjection;
    using Refit;

    public static class HttpClientFactoryExtensions
    {
        public static Func<PolicyBuilder<HttpResponseMessage>, IAsyncPolicy<HttpResponseMessage>> DefaultPolicy =
            builder => builder.WaitAndRetryAsync(new[] {TimeSpan.FromMilliseconds(500)});

        public static IHttpClientBuilder AddApiClient<T, TImpl>(
            this IServiceCollection services,
            ApiClientConfig         config,
            Action<HttpClient>      configure          = null,
            bool                    useDefaultPolicies = true)
            where T : class
            where TImpl : class, T
        {
            return services.AddClientConfig<T>(config)
                           .AddHttpClient<T, TImpl>()
                           .ConfigureClient(config, configure)
                           .AddErrorPolicies(useDefaultPolicies);
        }

        public static IHttpClientBuilder AddRefitApiClient<T>(
            this IServiceCollection services,
            ApiClientConfig         config,
            Action<HttpClient>      configure          = null,
            bool                    useDefaultPolicies = true)
            where T : class
        {
            return services.AddClientConfig<T>(config)
                           .AddRefitClient<T>()
                           .ConfigureClient(config, configure)
                           .AddErrorPolicies(useDefaultPolicies);
        }

        public static IServiceCollection AddClientConfig<T>(this IServiceCollection services, ApiClientConfig config)
            where T : class
        {
            var injectableConfig = new ApiClientConfig<T>() {Value = config};
            services.AddSingleton<IApiClientConfig<T>>(injectableConfig);

            services.Load<SecurityModule>();
            return services;
        }

        private static IHttpClientBuilder AddErrorPolicies(this IHttpClientBuilder builder, bool useDefaultPolicy)
        {
            return useDefaultPolicy
                       ? builder.AddTransientHttpErrorPolicy(DefaultPolicy)
                       : builder;
        }

        private static IHttpClientBuilder ConfigureClient(this IHttpClientBuilder builder, ApiClientConfig config, Action<HttpClient> configure)
        {
            builder = builder.ConfigureHttpClient(
                                  client =>
                                  {
                                      client.BaseAddress = new Uri(config.Url);
                                      client.SetAuthorization(config.Security);
                                      configure?.Invoke(client);
                                  })
                             .AddAuthorizationHandler(config.Code, config.Security)
                             .AddProxy(config.Proxy);
            return builder;
        }

        public static IHttpClientBuilder AddProxy(this IHttpClientBuilder builder, ProxyConfig config)
        {
            if (config == null)
                return builder;

            builder = builder.ConfigurePrimaryHttpMessageHandler(
                () =>
                {
                    var credentials = new NetworkCredential(config.UserName, config.Password);
                    var proxy       = new WebProxy(config.Url, true, null, credentials);
                    return new HttpClientHandler()
                           {
                               Proxy                 = proxy,
                               UseDefaultCredentials = false,
                               PreAuthenticate       = true
                           };
                });
            return builder;
        }
    }
}
