namespace RecShark.AspNetCore.ApiClient.Security
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using RecShark.DependencyInjection;

    public class SecurityModule : DIModule
    {
        public override void Load(IServiceCollection services)
        {
            services.TryAddSingleton<IAccessTokenProviderFactory, AccessTokenProviderFactory>();
            services.TryAddTransient<IAuthorityApiClient, AuthorityApiClient>();
            services.AddMemoryCache();
        }
    }
}
