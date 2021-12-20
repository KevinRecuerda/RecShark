namespace RecShark.AspNetCore.ApiClient.Security
{
    using Microsoft.Extensions.DependencyInjection;

    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddAuthorizationHandler(this IHttpClientBuilder builder, string name, SecurityConfig config)
        {
            return config?.Type switch
            {
                SecurityConfig.SecurityType.Oauth2 => builder.AddAccessTokenHandler(name, config),
                _                                  => builder
            };
        }

        public static IHttpClientBuilder AddAccessTokenHandler(this IHttpClientBuilder builder, string code, SecurityConfig securityConfig)
        {
            builder.AddHttpMessageHandler(
                services =>
                {
                    var tokenProviderFactory = services.GetRequiredService<IAccessTokenProviderFactory>();
                    var tokenProvider        = tokenProviderFactory.Create(code, securityConfig);

                    var handler = ActivatorUtilities.CreateInstance<AccessTokenDelegatingHandler>(services, tokenProvider);
                    return handler;
                });

            return builder;
        }
    }
}
