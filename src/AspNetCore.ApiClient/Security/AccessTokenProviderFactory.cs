namespace RecShark.AspNetCore.ApiClient.Security
{
    using System;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;

    public interface IAccessTokenProviderFactory
    {
        IAccessTokenProvider Create(string name, SecurityConfig securityConfig);
    }

    public class AccessTokenProviderFactory : IAccessTokenProviderFactory
    {
        private readonly IMemoryCache     memoryCache;
        private readonly IServiceProvider serviceProvider;

        public AccessTokenProviderFactory(IMemoryCache memoryCache, IServiceProvider serviceProvider)
        {
            this.memoryCache     = memoryCache;
            this.serviceProvider = serviceProvider;
        }

        public IAccessTokenProvider Create(string name, SecurityConfig securityConfig)
        {
            if (!this.memoryCache.TryGetValue(name, out IAccessTokenProvider provider))
            {
                provider = ActivatorUtilities.CreateInstance<AccessTokenProvider>(this.serviceProvider, securityConfig);

                this.memoryCache.Set(name, provider);
            }

            return provider;
        }
    }
}
