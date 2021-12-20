namespace RecShark.AspNetCore.ApiClient.Security
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public interface IAccessTokenProvider
    {
        Task<string> GetAccessToken(CancellationToken cancellationToken);
    }

    public class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly ILogger<AccessTokenProvider> logger;
        private readonly IAuthorityApiClient          authorityApiClient;
        private readonly SecurityConfig               securityConfig;
        private readonly TimeSpan                     earlyRefreshPeriod;
        private readonly object                       locker = new object();

        public AccessTokenProvider(
            ILogger<AccessTokenProvider> logger,
            IAuthorityApiClient authorityApiClient,
            SecurityConfig      securityConfig,
            TimeSpan?           earlyRefreshPeriod = null)
        {
            this.logger             = logger;
            this.authorityApiClient = authorityApiClient;
            this.securityConfig     = securityConfig;
            this.earlyRefreshPeriod = earlyRefreshPeriod ?? TimeSpan.FromSeconds(20);
        }

        public Token Token { get; set; }

        public async Task<string> GetAccessToken(CancellationToken cancellationToken)
        {
            if (this.IsExpired())
                await this.RequestTokenConcurrently(cancellationToken);

            return this.Token?.AccessToken;
        }

        public bool IsExpired()
        {
            var safeExpiration = this.Token?.Expiration.Add(-this.earlyRefreshPeriod);
            return safeExpiration == null || safeExpiration <= DateTimeOffset.UtcNow;
        }

        public Task RequestTokenConcurrently(CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () =>
                {
                    lock (this.locker)
                    {
                        if (this.IsExpired())
                            this.Token = this.RequestToken(cancellationToken).Result;
                    }
                },
                cancellationToken);
        }

        private async Task<Token> RequestToken(CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogDebug("[OAUTH2] requesting token...");
                var token = await this.authorityApiClient.RequestToken(this.securityConfig, cancellationToken);
                this.logger.LogDebug($"[OAUTH2] token retrieved: {token}");
                return token;
            }
            catch (Exception exception)
            {
                this.logger.LogError($"[OAUTH2] token retrieval failed: {exception.Message}");
                return null;
            }
        }
    }
}
